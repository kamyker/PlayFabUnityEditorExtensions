using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace PlayFab.PfEditor
{
    public class PlayFabEditorSDKTools : UnityEditor.Editor
    {
        private const int buttonWidth = 150;
        public static bool? isInstalled = null;
        public static bool IsInstalled
        {
            get
            {
                if (isInstalled == null)
                    CheckIfInstalledAndVersion();
                return (bool)isInstalled;
            }
        }

        private static string installedSdkVersion = string.Empty;
        private static string latestSdkVersion = string.Empty;
        private static UnityEngine.Object sdkFolder;
        private static bool isObjectFieldActive;
        private static bool isInitialized; //used to check once, gets reset after each compile;

        private static PlayFabSharedSettings playFabSettings;
        public static PlayFabSharedSettings PlayFabSettings
        {
            get
            {
                if (playFabSettings == null)
                    playFabSettings = PlayFabSharedSettings.LoadFromResources();
                return playFabSettings;
            }
        }

        public static void DrawSdkPanel()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                GetLatestSdkVersion();
                sdkFolder = FindSdkAsset();
                CheckIfInstalledAndVersion();
            }

            if (IsInstalled)
                ShowSdkInstalledMenu();
            else
                ShowSdkNotInstalledMenu();
        }

        private static void ShowSdkInstalledMenu()
        {
            isObjectFieldActive = sdkFolder == null;

            var labelStyle = new GUIStyle(PlayFabEditorHelper.uiStyle.GetStyle("titleLabel"));
            using (new UnityVertical(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                EditorGUILayout.LabelField(string.Format("SDK {0} is installed", string.IsNullOrEmpty(installedSdkVersion) ? "Unknown" : installedSdkVersion),
                    labelStyle, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth));

                if (!isObjectFieldActive)
                {
                    GUI.enabled = false;
                }
                else
                {
                    EditorGUILayout.LabelField(
                        "An SDK was detected, but we were unable to find the directory. Drag-and-drop the top-level PlayFab SDK folder below.",
                        PlayFabEditorHelper.uiStyle.GetStyle("orTxt"));
                }

                using (new UnityHorizontal(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                {
                    GUILayout.FlexibleSpace();
                    sdkFolder = EditorGUILayout.ObjectField(sdkFolder, typeof(UnityEngine.Object), false, GUILayout.MaxWidth(200));
                    GUILayout.FlexibleSpace();
                }

                if (!isObjectFieldActive)
                {
                    // this is a hack to prevent our "block while loading technique" from breaking up at this point.
                    GUI.enabled = !EditorApplication.isCompiling && PlayFabEditor.blockingRequests.Count == 0;
                }

                if (sdkFolder != null)
                {
                    using (new UnityHorizontal(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                    {

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("REMOVE SDK", PlayFabEditorHelper.uiStyle.GetStyle("textButton"), GUILayout.MinHeight(32), GUILayout.MinWidth(200)))
                        {
                            RemoveSdk();
                        }

                        GUILayout.FlexibleSpace();
                    }
                }

            }

            if (IsInstalled)
            {
                using (new UnityVertical(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
                {
                    using (new UnityHorizontal(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleClear")))
                    {
                        if (ShowSDKUpgrade())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Upgrade to " + latestSdkVersion, PlayFabEditorHelper.uiStyle.GetStyle("Button"), GUILayout.MinHeight(32)))
                            {
                                UpgradeSdk();
                            }
                            GUILayout.FlexibleSpace();
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.LabelField("You have the latest SDK!", labelStyle, GUILayout.MinHeight(32));
                            GUILayout.FlexibleSpace();
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
            {
                using (new UnityVertical(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
                {
                    EditorGUILayout.LabelField("Before making PlayFab API calls, the SDK must be configured to your PlayFab Title.", PlayFabEditorHelper.uiStyle.GetStyle("orTxt"));
                    using (new UnityHorizontal())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("SET MY TITLE", PlayFabEditorHelper.uiStyle.GetStyle("textButton")))
                        {
                            PlayFabEditor.RaiseStateUpdate(PlayFabEditor.EdExStates.GoToSettings);
                        }
                        GUILayout.FlexibleSpace();
                    }
                }
            }

            using (new UnityHorizontal(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("VIEW RELEASE NOTES", PlayFabEditorHelper.uiStyle.GetStyle("textButton"), GUILayout.MinHeight(32), GUILayout.MinWidth(200)))
                {
                    Application.OpenURL("https://api.playfab.com/releaseNotes/");
                }

                GUILayout.FlexibleSpace();
            }
        }

        private static void ShowSdkNotInstalledMenu()
        {
            using (new UnityVertical(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
            {
                var labelStyle = new GUIStyle(PlayFabEditorHelper.uiStyle.GetStyle("titleLabel"));

                EditorGUILayout.LabelField("No SDK is installed.", labelStyle, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth));
                GUILayout.Space(20);

                using (new UnityHorizontal(PlayFabEditorHelper.uiStyle.GetStyle("gpStyleGray1")))
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Refresh", PlayFabEditorHelper.uiStyle.GetStyle("Button"), GUILayout.MaxWidth(buttonWidth), GUILayout.MinHeight(32)))
                        isInitialized = false;
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Install PlayFab SDK", PlayFabEditorHelper.uiStyle.GetStyle("Button"), GUILayout.MaxWidth(buttonWidth), GUILayout.MinHeight(32)))
                        ImportLatestSDK();

                    GUILayout.FlexibleSpace();
                }
            }
        }

        public static void ImportLatestSDK()
        {
            AddRequest request = Client.
                Add(Strings.GitUrls.Build(ApiCategory.sdk));
            Client.Add(Strings.GitUrls.Build(ApiCategory.shared));

            EditorApplication.update += Progress;

            void Progress()
            {
                if (request.IsCompleted)
                {
                    Debug.Log("PlayFab SDK Install: Complete");
                    if (request.Status == StatusCode.Success)
                    {
                        Debug.Log("Installed: " + request.Result.packageId);
                        CheckIfInstalledAndVersion();
                    }
                    else if (request.Status >= StatusCode.Failure)
                        Debug.Log(request.Error.message);

                    EditorApplication.update -= Progress;
                }
            }
        }

        public static void CheckIfInstalledAndVersion()
        {
            installedSdkVersion = PlayFabEditorHelper.GetApiVersion(ApiCategory.sdk);
            //Debug.Log($"installedSdkVersion: {installedSdkVersion}");
            isInstalled = installedSdkVersion != null;
        }

        private static UnityEngine.Object FindSdkAsset()
        {
            return AssetDatabase.LoadAssetAtPath(
                Strings.Package.BuildPath(ApiCategory.sdk), typeof(UnityEngine.Object));
        }

        private static bool ShowSDKUpgrade()
        {
            if (string.IsNullOrEmpty(latestSdkVersion) || latestSdkVersion == "Unknown")
            {
                return false;
            }

            if (string.IsNullOrEmpty(installedSdkVersion) || installedSdkVersion == "Unknown")
            {
                return true;
            }

            string[] currrent = installedSdkVersion.Split('.');
            string[] latest = latestSdkVersion.Split('.');

            if (int.Parse(currrent[0]) < 2)
            {
                return false;
            }

            return int.Parse(latest[0]) > int.Parse(currrent[0])
                || int.Parse(latest[1]) > int.Parse(currrent[1])
                || int.Parse(latest[2]) > int.Parse(currrent[2]);
        }

        private static void UpgradeSdk()
        {
            if (EditorUtility.DisplayDialog("Confirm SDK Upgrade", "This action will remove the current PlayFab SDK and install the lastet version. Related plug-ins will need to be manually upgraded.", "Confirm", "Cancel"))
            {
                ImportLatestSDK();
            }
        }

        private static void RemoveSdk(bool prompt = true)
        {
            if (prompt && !EditorUtility.DisplayDialog("Confirm SDK Removal", "This action will remove the current PlayFab SDK. Related plug-ins will need to be manually removed.", "Confirm", "Cancel"))
                return;

            //try to clean-up the plugin dirs
            if (Directory.Exists(Application.dataPath + "/Plugins"))
            {
                var folders = Directory.GetDirectories(Application.dataPath + "/Plugins", "PlayFabShared", SearchOption.AllDirectories);
                foreach (var folder in folders)
                    FileUtil.DeleteFileOrDirectory(folder);

                //try to clean-up the plugin files (if anything is left)
                var files = Directory.GetFiles(Application.dataPath + "/Plugins", "PlayFabErrors.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                    FileUtil.DeleteFileOrDirectory(file);
            }

            var request = Client.Remove(Strings.Package.BuildName(ApiCategory.sdk));
            EditorApplication.update += Progress;

            void Progress()
            {
                if (request.IsCompleted)
                {
                    if (request.Status == StatusCode.Success)
                    {
                        PlayFabEditor.RaiseStateUpdate(PlayFabEditor.EdExStates.OnSuccess, "PlayFab SDK Removed!");
                        installedSdkVersion = null;
                        isInstalled = false;
                    }
                    else if (request.Status >= StatusCode.Failure)
                        PlayFabEditor.RaiseStateUpdate(PlayFabEditor.EdExStates.OnError,
                            "An unknown error occured and the PlayFab SDK could not be removed.");

                    EditorApplication.update -= Progress;
                }
            }
        }

        private async static void GetLatestSdkVersion()
        {
            var threshold = PlayFabEditorPrefsSO.Instance.EdSet_lastSdkVersionCheck != DateTime.MinValue ? PlayFabEditorPrefsSO.Instance.EdSet_lastSdkVersionCheck.AddHours(1) : DateTime.MinValue;

            if (DateTime.Today > threshold)
            {
                var www = await UnityWebRequest.Get(Strings.GitUrls.SDK_VERSION).SendWebRequest();

                if (!www.isHttpError || !www.isNetworkError)
                {
                    var dict = Json.PlayFabSimpleJson.DeserializeObject<Dictionary<string, string>>(www.downloadHandler.text);
                    dict.TryGetValue("version", out string version);
                    //Debug.Log($"Remote PlayFab version: {version}");
                    latestSdkVersion = version ?? "Unknown";
                    PlayFabEditorPrefsSO.Instance.EdSet_latestSdkVersion = latestSdkVersion;
                }
                else
                    Debug.Log(www.error);
            }
            else
                latestSdkVersion = PlayFabEditorPrefsSO.Instance.EdSet_latestSdkVersion;
        }
    }
}
