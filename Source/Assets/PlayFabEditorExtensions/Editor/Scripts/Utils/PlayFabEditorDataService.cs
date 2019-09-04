using PlayFab.PfEditor.EditorModels;
using PlayFab.PfEditor.Json;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PlayFab.PfEditor
{
    [InitializeOnLoad]
    public class PlayFabEditorDataService : UnityEditor.Editor
    {
        public static Title ActiveTitle
        {
            get
            {
                if (PlayFabEditorPrefsSO.Instance.StudioList != null && PlayFabEditorPrefsSO.Instance.StudioList.Count > 0)
                {
                    if (string.IsNullOrEmpty(PlayFabEditorPrefsSO.Instance.SelectedStudio) || PlayFabEditorPrefsSO.Instance.SelectedStudio == PlayFabEditorHelper.STUDIO_OVERRIDE)
                        return new Title { Id = PlayFabEditorSDKTools.PlayFabSettings.TitleId, SecretKey = PlayFabEditorSDKTools.PlayFabSettings.DeveloperSecretKey, GameManagerUrl = PlayFabEditorHelper.GAMEMANAGER_URL };

                    if (string.IsNullOrEmpty(PlayFabEditorPrefsSO.Instance.SelectedStudio) || string.IsNullOrEmpty(PlayFabEditorSDKTools.PlayFabSettings.TitleId))
                        return null;

                    int studioIndex; int titleIndex;
                    if (DoesTitleExistInStudios(PlayFabEditorSDKTools.PlayFabSettings.TitleId, out studioIndex, out titleIndex))
                        return PlayFabEditorPrefsSO.Instance.StudioList[studioIndex].Titles[titleIndex];
                }
                return null;
            }
        }

        public static void SaveEnvDetails(bool updateToScriptableObj = true)
        {
            UpdateScriptableObject();
        }

        private static void UpdateScriptableObject()
        {
            var playfabSettingsType = PlayFabEditorSDKTools.PlayFabSettings;
            if (playfabSettingsType == null || !PlayFabEditorSDKTools.IsInstalled || !PlayFabEditorSDKTools.isSdkSupported)
                return;

            EditorUtility.SetDirty(playfabSettingsType);
            PlayFabEditorPrefsSO.Save();
            AssetDatabase.SaveAssets();
        }

        public static bool DoesTitleExistInStudios(string searchFor) //out Studio studio
        {
            if (PlayFabEditorPrefsSO.Instance.StudioList == null)
                return false;
            searchFor = searchFor.ToLower();
            foreach (var studio in PlayFabEditorPrefsSO.Instance.StudioList)
                if (studio.Titles != null)
                    foreach (var title in studio.Titles)
                        if (title.Id.ToLower() == searchFor)
                            return true;
            return false;
        }

        private static bool DoesTitleExistInStudios(string searchFor, out int studioIndex, out int titleIndex) //out Studio studio
        {
            studioIndex = 0; // corresponds to our _OVERRIDE_
            titleIndex = -1;

            if (PlayFabEditorPrefsSO.Instance.StudioList == null)
                return false;

            for (var studioIdx = 0; studioIdx < PlayFabEditorPrefsSO.Instance.StudioList.Count; studioIdx++)
            {
                for (var titleIdx = 0; titleIdx < PlayFabEditorPrefsSO.Instance.StudioList[studioIdx].Titles.Length; titleIdx++)
                {
                    if (PlayFabEditorPrefsSO.Instance.StudioList[studioIdx].Titles[titleIdx].Id.ToLower() == searchFor.ToLower())
                    {
                        studioIndex = studioIdx;
                        titleIndex = titleIdx;
                        return true;
                    }
                }
            }

            return false;
        }

        public static void RefreshStudiosList(bool onlyIfNull = false)
        {
            if (string.IsNullOrEmpty(PlayFabEditorPrefsSO.Instance.DevAccountToken))
                return; // Can't load studios when not logged in
            if (onlyIfNull && PlayFabEditorPrefsSO.Instance.StudioList != null)
                return; // Don't spam load this, only load it the first time

            if (PlayFabEditorPrefsSO.Instance.StudioList != null)
                PlayFabEditorPrefsSO.Instance.StudioList.Clear();
            PlayFabEditorApi.GetStudios(new GetStudiosRequest(), (getStudioResult) =>
            {
                if (PlayFabEditorPrefsSO.Instance.StudioList == null)
                    PlayFabEditorPrefsSO.Instance.StudioList = new List<Studio>();
                foreach (var eachStudio in getStudioResult.Studios)
                    PlayFabEditorPrefsSO.Instance.StudioList.Add(eachStudio);
                PlayFabEditorPrefsSO.Instance.StudioList.Add(Studio.OVERRIDE);
                PlayFabEditorPrefsSO.Save();
            }, PlayFabEditorHelper.SharedErrorCallback);
        }
    }
}
