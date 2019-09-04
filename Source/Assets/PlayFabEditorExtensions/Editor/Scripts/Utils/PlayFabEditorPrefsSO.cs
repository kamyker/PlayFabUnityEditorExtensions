using PlayFab.PfEditor.EditorModels;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace PlayFab.PfEditor
{
#if UNITY_5_3_OR_NEWER
    [CreateAssetMenu(fileName = "PlayFabEditorPrefsSO", menuName = "PlayFab/Make Prefs SO", order = 1)]
#endif
    public class PlayFabEditorPrefsSO : ScriptableObject
    {
        private static PlayFabEditorPrefsSO _instance;
        public static PlayFabEditorPrefsSO Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var settingsList = Resources.LoadAll<PlayFabEditorPrefsSO>("PlayFabEditorExtensions");
                if (settingsList.Length == 1)
                    _instance = settingsList[0];
                if (_instance != null)
                    return _instance;

                _instance = CreateInstance<PlayFabEditorPrefsSO>();
                var prefsSOPath = Path.Combine(Strings.PATH_EDEX_RESOURCES_RELATIVE, "PlayFabEditorPrefsSO.asset");

                AssetDatabase.CreateAsset(_instance, prefsSOPath);
                AssetDatabase.SaveAssets();
                Debug.LogWarning("Created missing PlayFabEditorPrefsSO file");
                return _instance;
            }
        }

        public static void Save()
        {
            EditorUtility.SetDirty(_instance);
            AssetDatabase.SaveAssets();
        }

        public string DevAccountEmail;
        public string DevAccountToken;

        public List<Studio> StudioList = null; // Null means not fetched, empty is a possible return result from GetStudios
        public string SelectedStudio;

        public readonly Dictionary<string, string> TitleDataCache = new Dictionary<string, string>();
        public readonly Dictionary<string, string> InternalTitleDataCache = new Dictionary<string, string>();

        public string LocalCloudScriptPath;

        private string _latestSdkVersion;
        private string _latestEdExVersion;
        public bool PanelIsShown;

        public string EdSet_latestSdkVersion { get { return _latestSdkVersion; } set { _latestSdkVersion = value; EdSet_lastSdkVersionCheck = DateTime.UtcNow; } }
        public string EdSet_latestEdExVersion { get { return _latestEdExVersion; } set { _latestEdExVersion = value; EdSet_lastEdExVersionCheck = DateTime.UtcNow; } }

        public DateTime EdSet_lastSdkVersionCheck { get; private set; }
        public DateTime EdSet_lastEdExVersionCheck { get; private set; }

        public int curMainMenuIdx;
        public int curSubMenuIdx;
    }
}
