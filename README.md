# PlayFab Unity Editor Extensions

## Fork with Package Manager, Assembly Definitions, async/await support

Read: [Original README](https://github.com/PlayFab/UnityEditorExtensions/blob/master/README.md)

⚠️ Warning - this is my custom version of official sdk, some functions may not work correctly.

## Setup:
  
  1. Open `<project_root>/Packages/manifest.json` and add
  ```
  "com.playfab.editorextensions": "https://github.com/kamyker/PlayFabUnityEditorExtensions.git",
  "com.playfab.shared": "https://github.com/kamyker/PlayFabUnityShared.git"
  ```
  to `dependencies`
  
  2. Follow [Original README](https://github.com/PlayFab/UnityEditorExtensions/blob/master/README.md) from step 2.

This is how project looks like with this fork:

![project](_repoAssets/img/EdEx_Project.png?raw=true "Title")

# Features
## Async/Await and simpler syntax
Example from  [playfab-unity-getting-started](https://api.playfab.com/docs/getting-started/unity-getting-started) instead of writing this code:
```
var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true};
PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

private void OnLoginSuccess(LoginResult result)
{
}

private void OnLoginFailure(PlayFabError error)
{
    Debug.LogError(error.GenerateErrorReport());
}
```
You get:
```
try
{
    var result = await ClientAPI.LoginWithCustomID("144", true, "GettingStartedGuide");
    //success
}
catch(PlayFabError e)
{
    Debug.LogError(e.Message);
}
```
## Assembly Definitions
No recompilation of PlayFabSDK

## Package Manager
Simplifies how sdks are installed, upgraded and managed. Makes it really easy to make plugins for playfab.

# Tips
## Fire Forget
To run any api call without awaiting it (for example logging events) use included FireForgetLog extension method:
```
EventsAPI.WriteEvents(some_events).FireForgetLog();
```
This is similar to:
```
Task.Run(async () => 
try
{
    await EventsAPI.WriteEvents(some_events);
}
catch(Exception e)
{
    Debug.LogException(e);
});
```
## Other
Avoid using `async void` methods. Use `async Task` instead and run them with FireForgetLog.
