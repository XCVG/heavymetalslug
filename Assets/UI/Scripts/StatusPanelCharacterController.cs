using CommonCore;
using CommonCore.State;
using CommonCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slug
{

    /// <summary>
    /// Inserts the portrait into the status panel
    /// </summary>
    public class StatusPanelCharacterController : PanelController
    {
        [SerializeField]
        private Image CharacterImage = null;

        public override void SignalPaint()
        {
            base.SignalPaint();

            //slightly stupid
            CharacterImage.sprite = CoreUtils.LoadResource<Sprite>($"UI/Portraits/{GameState.Instance.Player1Character}Cameo");
        }

    }
}