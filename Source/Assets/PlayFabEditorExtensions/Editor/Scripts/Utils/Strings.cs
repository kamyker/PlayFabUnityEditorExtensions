using System.IO;
using UnityEngine;

namespace PlayFab.PfEditor
{
    public static class Strings
    {
        public static class GitUrls
        {
            private const string RAW = "https://raw.githubusercontent.com/kamyker/";
            public static readonly string EDEX_VERSION = RAW + "PlayFabUnityEditorExtensions/master/package.json";
            public static readonly string SDK_VERSION = RAW + "PlayFabUnitySDK/master/package.json";

            private const string MAIN = "https://github.com/kamyker/";
            public static string Build(ApiCategory apiCategory)
            {
                string name = apiCategory.ToString();
                if (apiCategory != ApiCategory.editorextensions)
                    return MAIN + "PlayFabUnity" + char.ToUpper(name[0]) + name.Substring(1) + ".git";
                else
                    return MAIN + "PlayFabUnityEditorExtensions.git";

            }
        }

        public static readonly string PATH_MAIN = Path.Combine("Packages", "com.playfab.editorextensions", "Source", "Assets", "PlayFabEditorExtensions", "Editor");
        public static readonly string PATH_UI = Path.Combine(PATH_MAIN, "UI");
        public static string PATH_UI_IMG(string img) => Path.Combine(PATH_UI, "Images", img);

        public static class Package
        {
            public const string PREFIX = "com.playfab.";
            public static string BuildName(ApiCategory api) => PREFIX + api.ToString();
            public static string BuildPath(ApiCategory api) => Path.Combine("Packages", BuildName(api));
        }
        public static string PATH_EDEX_RESOURCES_RELATIVE
         => "Assets" + PATH_EDEX_RESOURCES.Replace(Application.dataPath, "");

        public static string PATH_EDEX_RESOURCES
        {
            get
            {
                if (!Directory.Exists(pathEdexResources))
                    Directory.CreateDirectory(pathEdexResources);
                return pathEdexResources;
            }
        }
        private static readonly string pathEdexResources = Path.Combine(Application.dataPath, "Resources", "PlayFabEditorExtensions");

        public const string CLOUDSCRIPT_FILENAME = ".CloudScript.js";  //prefixed with a '.' to exclude this code from Unity's compiler
        public static readonly string CLOUDSCRIPT_PATH = Path.Combine(PATH_EDEX_RESOURCES, CLOUDSCRIPT_FILENAME);
    }

}
