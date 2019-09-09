using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayFab.PfEditor
{
    public class Header : Box
    {
        public VisualElement GMText;

        public Header()
        {
            this.Set(name: "header").AddRange(
                new Image().Set(background_image: Strings.PATH_UI_IMG("playfablogo.png"), maxWidth: 230, flexGrow: 0.65f),
                new Box().Set(flexDirection: FlexDirection.Row,
                                             alignItems: Align.Center).AddRange(
                    new Button().Set(name: "gMText", _class: "gameManagerBtn").Set(text: "GAME MANAGER").AssignTo(out GMText),
                    new Button().Set(name: "gMIcon", _class: "gameManagerBtn")
                )
            );

            this.Query<Button>(null, "gameManagerBtn").ForEach(btn
                => btn.clickable.clicked += OnDashbaordClicked);

            RegisterCallback<GeometryChangedEvent>(OnWindowResized);

            void OnWindowResized(GeometryChangedEvent evt)
            {
                if (evt.newRect.width < 375)
                {
                    GMText.visible = false;
                    if (style.height != 40)
                        style.height = 40;
                }
                else
                {
                    GMText.visible = true;
                    if (style.height != 50)
                        style.height = 50;
                }
            }
        }

        public static void OnDashbaordClicked()
        {
            Help.BrowseURL(PlayFabEditorDataService.ActiveTitle != null ? PlayFabEditorDataService.ActiveTitle.GameManagerUrl : PlayFabEditorHelper.GAMEMANAGER_URL);
        }
    }
}
