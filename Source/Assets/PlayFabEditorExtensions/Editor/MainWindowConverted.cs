using KS.UxmlToCsharp;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MainWindowConverted : UxmlConvertedBase
{
    protected override string uxmlGuid => "4a9d09e0a8eda534187166cdeae7a0b0";
    public IMGUIContainer ProgressBar;
    public VisualElement Menu;
    public IMGUIContainer MainIMGUI;
    protected override void AssignFields()
    {
        ProgressBar = (IMGUIContainer)elementsToAssign["ProgressBar"];
        Menu = (VisualElement)elementsToAssign["Menu"];
        MainIMGUI = (IMGUIContainer)elementsToAssign["MainIMGUI"];
    }
}
