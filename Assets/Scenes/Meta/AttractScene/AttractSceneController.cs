using CommonCore;
using CommonCore.Input;
using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slug
{

    /// <summary>
    /// Controller for attract screen (does very little)
    /// </summary>
    public class AttractSceneController : BaseSceneController
    {
        [SerializeField, Header("Attract Scene")]
        private Text Player1CreditsText = null;
        [SerializeField]
        private Text Player2CreditsText = null;
        [SerializeField]
        private GameObject CopyrightText = null;
        [SerializeField]
        private GameObject AltCopyrightText = null;

        public override void Update()
        {
            base.Update();

            //handle text
            if(MetaState.Instance.Player1Credits > 0 || MetaState.Instance.Player2Credits > 0)
            {
                CopyrightText.SetActive(false);
                AltCopyrightText.SetActive(true);
                Player1CreditsText.text = $"CREDITS: {MetaState.Instance.Player1Credits}";
                Player2CreditsText.text = $"CREDITS: {MetaState.Instance.Player2Credits}";
            }
            else
            {
                CopyrightText.SetActive(true);
                AltCopyrightText.SetActive(false);
                Player1CreditsText.text = string.Empty;
                Player2CreditsText.text = string.Empty;
            }

            //listen for start button
            if(MappedInput.GetButtonDown("Start"))
            {
                SharedUtils.ChangeScene("CharacterSelectScene");
            }


            //TODO STRETCH attract mode
            //as in, a proper attract sequence as a stretch goal, not literally stretch the attract screen

        }
    }
}