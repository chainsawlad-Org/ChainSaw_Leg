using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ChainSawLeg.Application.Editor
{
    [InitializeOnLoad]
    internal static class NoMetaFile
    {
        private const string SettingsResourceName = "TMP Settings";

        static NoMetaFile()
        {
            EditorApplication.delayCall += EnsureEssentialsImported;
        }

        private static void EnsureEssentialsImported()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += EnsureEssentialsImported;
                return;
            }

            TMP_Settings settings = Resources.Load<TMP_Settings>(SettingsResourceName);
            if (settings != null && IsCurrentVersion(settings))
            {
                return;
            }

            TMP_PackageResourceImporter.ImportResources(importEssentials: true, importExamples: false, interactive: false);
        }

        private static bool IsCurrentVersion(TMP_Settings settings)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

            FieldInfo assetVersionField = typeof(TMP_Settings).GetField("assetVersion", flags);
            FieldInfo currentVersionField = typeof(TMP_Settings).GetField("s_CurrentAssetVersion", flags);

            string assetVersion = assetVersionField?.GetValue(settings) as string;
            string currentVersion = currentVersionField?.GetValue(null) as string;

            return !string.IsNullOrWhiteSpace(assetVersion) && assetVersion == currentVersion;
        }
    }
}
