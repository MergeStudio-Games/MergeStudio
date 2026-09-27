using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MergeStudio.Editor
{
    public static class MixoKitchenVisualQaBuild
    {
        public static void BuildWindows()
        {
            string output = Environment.GetEnvironmentVariable("MIXO_VISUAL_QA_BUILD");
            if (string.IsNullOrWhiteSpace(output)) output = Path.Combine("Build", "VisualQA", "MixoKitchen.exe");
            output = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            EditorUserBuildSettings.SwitchActiveBuildTarget(NamedBuildTarget.Standalone, BuildTarget.StandaloneWindows64);
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressableAssetSettings.PlayerBuildOption previous = settings == null
                ? AddressableAssetSettings.PlayerBuildOption.PreferencesValue
                : settings.BuildAddressablesWithPlayerBuild;
            if (settings != null) settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = output,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development | BuildOptions.StrictMode
                });
            }
            finally
            {
                if (settings != null) settings.BuildAddressablesWithPlayerBuild = previous;
            }
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Visual QA build failed: " + report.summary.result);
        }
    }
}
