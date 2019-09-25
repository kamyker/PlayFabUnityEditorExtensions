using KS.UxmlToCsharp;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class HeaderConverted : UxmlConvertedBase
{
    protected override string uxmlGuid => "ad1e82c3e09e6394395efd4596523c12";
    public Box Header;
    public Button GMText;
    protected override void AssignFields()
    {
        Header = (Box)elementsToAssign["Header"];
        GMText = (Button)elementsToAssign["GMText"];
    }
}
