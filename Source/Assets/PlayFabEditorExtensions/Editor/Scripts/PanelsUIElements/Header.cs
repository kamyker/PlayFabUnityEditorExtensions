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
    public class Header : HeaderConverted
    {
        public Header()
        {
            Header.Query<Button>(null, "gameManagerBtn").ForEach(btn
                => btn.clickable.clicked += OnDashbaordClicked);

            Header.RegisterCallback<GeometryChangedEvent>(OnWindowResized);

            void OnWindowResized(GeometryChangedEvent evt)
            {
                if (evt.newRect.width < 375)
                {
                    GMText.visible = false;
                    if (Header.style.height != 40)
                        Header.style.height = 40;
                }
                else
                {
                    GMText.visible = true;
                    if (Header.style.height != 50)
                        Header.style.height = 50;
                }
            }
        }

        public static void OnDashbaordClicked()
        {
            Help.BrowseURL(PlayFabEditorDataService.ActiveTitle != null ? PlayFabEditorDataService.ActiveTitle.GameManagerUrl : PlayFabEditorHelper.GAMEMANAGER_URL);
        }
    }
}
