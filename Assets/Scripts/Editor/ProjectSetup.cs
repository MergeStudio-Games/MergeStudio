using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace MergeStudio.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string SetupVersion = "3";
        static ProjectSetup() { EditorApplication.delayCall += Ensure; }
        [MenuItem("MergeStudio/Ensure Project Setup")]
        public static void Ensure()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Ensure; return; }
            string setupVersion = File.Exists("Assets/Settings/SetupVersion.txt")
                ? File.ReadAllText("Assets/Settings/SetupVersion.txt").Trim()
                : string.Empty;
            if (setupVersion == SetupVersion) return;
            Configure();
            File.WriteAllText("Assets/Settings/SetupVersion.txt", SetupVersion + "\n");
            AssetDatabase.Refresh();
        }
        [MenuItem("MergeStudio/Repair Generated Configuration")]
        public static void Configure()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            UnityEditor.VersionControlSettings.mode = "Visible Meta Files";
            // Product identity belongs to each generated game; repair must preserve it.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 1080; PlayerSettings.defaultScreenHeight = 2400;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // Google Play requires API 36 for new apps and updates from
            // 2026-08-31. Keep this explicit instead of relying on the
            // machine's "Automatic" SDK selection.
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            EditorUserBuildSettings.buildAppBundle = true;
            Directory.CreateDirectory("Assets/AddressableAssetsData");
            Directory.CreateDirectory("Assets/AddressablesData");
            AssetDatabase.Refresh();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                // Addressables stores its project-level pointer in Unity's
                // conventional folder even when the settings asset lives in
                // our repository-owned folder. Create it before assigning the
                // default object so a fresh clone does not fail on import.
                Directory.CreateDirectory("Assets/AddressableAssetsData");
                AssetDatabase.Refresh();
                settings = AddressableAssetSettings.Create("Assets/AddressablesData", "AddressableAssetSettings", true, true);
                AddressableAssetSettingsDefaultObject.Settings = settings;
            }
            foreach (string name in new[] { "UI_Remote", "Characters_Remote", "Events_Remote" })
            {
                var group = settings.FindGroup(name) ?? settings.CreateGroup(name, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
                schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            }
            Directory.CreateDirectory("Assets/Localization"); AssetDatabase.Refresh();
            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                var localization = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(localization, "Assets/Localization/LocalizationSettings.asset");
                LocalizationEditorSettings.ActiveLocalizationSettings = localization;
            }
            foreach (string code in new[] { "tr", "en" })
            {
                if (LocalizationEditorSettings.GetLocales().Any(l => l.Identifier.Code == code)) continue;
                var locale = Locale.CreateLocale(code); AssetDatabase.CreateAsset(locale, "Assets/Localization/" + code + ".asset");
                LocalizationEditorSettings.AddLocale(locale);
            }
            var collection = LocalizationEditorSettings.GetStringTableCollection("UI") ?? LocalizationEditorSettings.CreateStringTableCollection("UI", "Assets/Localization/Tables");
            string[] keys = { "continue", "settings", "shop", "play", "energy", "orders", "close", "spawn", "gold", "merge_instruction", "first_order_complete", "energy_offer", "sound_off", "sound_on" };
            string[] tr = { "Devam Et", "Ayarlar", "Ma\u011faza", "Oyna", "Enerji", "Sipari\u015fler", "Kapat", "E\u015fya \u00dcret", "Alt\u0131n", "E\u015fya \u00fcret. Ayn\u0131 seviyeye dokun veya s\u00fcr\u00fckleyerek birle\u015ftir. Sipari\u015fini tamamla!", "\u0130lk sipari\u015f tamamland\u0131! \u0130lerlemen kaydedildi.", "+{0} enerji / {1} elmas (bakiye: {2})", "Ses: kapal\u0131", "Ses: a\u00e7\u0131k" };
            string[] en = { "Continue", "Settings", "Shop", "Play", "Energy", "Orders", "Close", "Spawn Item", "Gold", "Generate pieces. Tap two matching levels or drag to combine. Complete your order!", "First order complete! Your progress is saved.", "+{0} energy / {1} diamonds (balance: {2})", "Sound: off", "Sound: on" };
            foreach (string code in new[] { "tr", "en" })
            {
                var table = collection.GetTable(new LocaleIdentifier(code)) as StringTable;
                if (table == null) table = collection.AddNewTable(new LocaleIdentifier(code)) as StringTable;
                for (int i = 0; i < keys.Length; i++) if (table.GetEntry(keys[i]) == null) table.AddEntry(keys[i], code == "tr" ? tr[i] : en[i]);
                EditorUtility.SetDirty(table);
            }
            EditorUtility.SetDirty(collection.SharedData); EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
    public sealed class BuildPreparation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;
        public void OnPreprocessBuild(BuildReport report) => ProjectSetup.Ensure();
    }
}
