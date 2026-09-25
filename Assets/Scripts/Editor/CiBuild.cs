using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Android;
using Unity.Burst;
using UnityEngine;

namespace MergeStudio.Editor
{
    /// <summary>
    /// Reproducible local/CI build entry point. GameCI can use its default
    /// builder, while this method makes a local AAB smoke build auditable.
    /// </summary>
    public static class CiBuild
    {
        public static void BuildAndroid()
        {
            BuildAndroidPlayer(true);
        }

        public static void BuildAndroidApk()
        {
            BuildAndroidPlayer(false);
        }

        private static void BuildAndroidPlayer(bool appBundle)
        {
            ProjectSetup.Configure();
            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = appBundle;
            ConfigureAndroidToolchain();

            if (IsEnabled(Environment.GetEnvironmentVariable("MERGESTUDIO_DISABLE_BURST")))
            {
                BurstCompiler.Options.EnableBurstCompilation = false;
                Debug.LogWarning("MERGESTUDIO_DISABLE_BURST is enabled; this is a local smoke-build mode only.");
            }

            string path = Environment.GetEnvironmentVariable("MERGESTUDIO_BUILD_PATH");
            if (string.IsNullOrWhiteSpace(path))
                path = Path.Combine("Build", appBundle ? "MixoKitchen.aab" : "MixoKitchen.apk");
            path = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No enabled scenes are configured.");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.StrictMode
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Android build failed: {report.summary.result}");
            Debug.Log($"Android {(appBundle ? "App Bundle" : "APK")} created: {path} ({report.summary.totalSize} bytes)");
        }

        private static bool IsEnabled(string value) =>
            string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);

        private static void ConfigureAndroidToolchain()
        {
            SetToolPath("MERGESTUDIO_ANDROID_SDK", value => AndroidExternalToolsSettings.sdkRootPath = value);
            SetToolPath("MERGESTUDIO_ANDROID_NDK", value => AndroidExternalToolsSettings.ndkRootPath = value);
            SetToolPath("MERGESTUDIO_JDK", value => AndroidExternalToolsSettings.jdkRootPath = value);
        }

        private static void SetToolPath(string variable, Action<string> setter)
        {
            string path = Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(path)) return;
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"{variable} points to a missing directory: {path}");
            setter(path);
            Debug.Log($"{variable}={path}");
        }
    }
}
