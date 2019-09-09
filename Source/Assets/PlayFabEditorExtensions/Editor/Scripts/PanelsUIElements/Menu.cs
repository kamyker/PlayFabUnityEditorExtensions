using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace PlayFab.PfEditor
{
    public class Menu : Box
    {
        internal enum MenuStates
        {
            Sdk = 0,
            Settings = 1,
            Data = 2,
            Help = 3,
            Tools = 4,
            Packages = 5,
            Logout = 6
        }

        Dictionary<MenuStates, Button> stateWithButton = new Dictionary<MenuStates, Button>();

        public Menu()
        {
            this.Set(flexDirection: FlexDirection.Row, justifyContent: Justify.SpaceAround, height: 25f, _class: "darkBackground");

            stateWithButton.Add(MenuStates.Sdk, new Button());
            stateWithButton.Add(MenuStates.Settings, new Button());
            stateWithButton.Add(MenuStates.Data, new Button());
            stateWithButton.Add(MenuStates.Tools, new Button());
            stateWithButton.Add(MenuStates.Packages, new Button());
            stateWithButton.Add(MenuStates.Help, new Button());
            stateWithButton.Add(MenuStates.Logout, new Button());

            foreach (var sWB in stateWithButton)
            {
                var btn = sWB.Value;
                var state = sWB.Key;

                btn.clickable.clicked += () => OnMenuButton(state);
                btn.text = state.ToString().ToUpperInvariant();

                Add(btn);
            }
            var savedState = (MenuStates)PlayFabEditorPrefsSO.Instance.curMainMenuIdx;
            OnMenuButton(savedState);

            PlayFabEditor.EdExStateUpdate += PlayFabEditor_EdExStateUpdate;

            void OnMenuButton(MenuStates state)
            {
                foreach (var sWB in stateWithButton)
                {
                    var curBtn = sWB.Value;
                    curBtn.RemoveFromClassList("blueColor");
                }
                stateWithButton[state].AddToClassList("blueColor");
                PlayFabEditor.RaiseStateUpdate(PlayFabEditor.EdExStates.OnMenuItemClicked, state.ToString());

                PlayFabEditorPrefsSO.Instance.curMainMenuIdx = (int)state;

                if (state == MenuStates.Logout)
                    PlayFabEditorAuthenticate.Logout();
            }

            void PlayFabEditor_EdExStateUpdate(PlayFabEditor.EdExStates state, string status, string misc)
            {
                if (state == PlayFabEditor.EdExStates.OnLogin)
                    OnMenuButton(MenuStates.Sdk);

                if (state == PlayFabEditor.EdExStates.GoToSettings)
                    OnMenuButton(MenuStates.Settings);
            }
        }
    }
}
