using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Input;
using UnityEngine.UI;
using CommonCore.State;
using CommonCore.Audio;

namespace Slug
{

    /// <summary>
    /// Controller for the character select screen
    /// </summary>
    public class CharacterSelectSceneController : BaseSceneController
    {
        private const float NavigateDeadzone = 0.1f;
        private const float DebounceTime = 0.5f;

        //like most things in this game, it was originally going to handle both players, but that was never implemented

        [SerializeField, Header("Character Select Scene")]
        private Text Player1CreditsText = null;
        [SerializeField]
        private Text Player2CreditsText = null;
        [SerializeField]
        private RectTransform[] CharacterPanels = null;
        [SerializeField]
        private RectTransform[] CloseupPanels = null;
        [SerializeField]
        private RectTransform P1Indicator = null;
        [SerializeField]
        private RectTransform P2Indicator = null;
        [SerializeField]
        private GameObject StartHintObject = null;

        private bool InputBlocked = false;
        private int Player1HighlightedCharacter = 0;
        private int Player2HighlightedCharacter = 0;
        private int Player1SelectedCharacter = -1;
        private int Player2SelectedCharacter = -1;

        private float TimeToNextP1Input = 0;
        private float TimeToNextP2Input = 0;

        public override void Update()
        {
            base.Update();

            HandleDebounceCooldown();
            UpdateCreditsText();
            HandleCharacterSelection();
            UpdateCharacterPanels();
        }

        private void HandleDebounceCooldown()
        {
            if (TimeToNextP1Input >= 0)
                TimeToNextP1Input -= Time.deltaTime;

            if (TimeToNextP2Input >= 0)
                TimeToNextP2Input -= Time.deltaTime;
        }

        private void UpdateCreditsText()
        {
            if (MetaState.Instance.Player1Credits > 0)
            {
                Player1CreditsText.text = $"CREDITS: {MetaState.Instance.Player1Credits}";
            }
            else
            {
                Player1CreditsText.text = $"INSERT COIN";
            }

            if (MetaState.Instance.Player2Credits > 0)
            {
                Player2CreditsText.text = $"CREDITS: {MetaState.Instance.Player2Credits}";
            }
            else
            {
                Player2CreditsText.text = $"INSERT COIN";
            }
        }

        private void HandleCharacterSelection()
        {
            if (InputBlocked)
                return;

            //this logic only really works for 1P; we'd have to check more conditions for two players

            //listen for movement, selection
            if(Player1SelectedCharacter >= 0)
            {
                //P1 character selected, listen for and allow deselection
                if (MappedInput.GetButtonDown(CommonCore.Input.DefaultControls.Fire))
                {
                    Player1SelectedCharacter = -1;
                    AudioPlayer.Instance.PlayUISound("char_deselect");
                }
            }
            else if(MetaState.Instance.Player1Credits > 0)
            {
                //P1 character not selected, listen for and allow movement and selection
                if (MappedInput.GetButtonDown(CommonCore.Input.DefaultControls.Fire) || MappedInput.GetButtonDown("Start"))
                {
                    Player1SelectedCharacter = Player1HighlightedCharacter;
                    AudioPlayer.Instance.PlayUISound($"char_select{Player1HighlightedCharacter}");
                }
                else
                {
                    float h = MappedInput.GetAxis(CommonCore.Input.DefaultControls.MoveX);
                    if (Mathf.Abs(h) > NavigateDeadzone && TimeToNextP1Input <= 0)
                    {
                        if (Mathf.Sign(h) > 0)
                            Player1HighlightedCharacter++;
                        else
                            Player1HighlightedCharacter--;

                        if (Player1HighlightedCharacter >= CharacterPanels.Length)
                            Player1HighlightedCharacter = 0;
                        else if (Player1HighlightedCharacter < 0)
                            Player1HighlightedCharacter = CharacterPanels.Length - 1;

                        TimeToNextP1Input += DebounceTime;
                    }
                }
            }

            //allow continuing if at least P1 has selected a character
            if(Player1SelectedCharacter >= 0 && MappedInput.GetButtonDown("Start"))
            {
                //goto next scene
                InputBlocked = true;
                StartCoroutine(CoExitScene());
            }
        }

        private void UpdateCharacterPanels()
        {
            //position indicators
            if (MetaState.Instance.Player1Credits > 0 && Player1HighlightedCharacter >= 0)
            {
                P1Indicator.gameObject.SetActive(true);
                var rt = CharacterPanels[Player1HighlightedCharacter];
                P1Indicator.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - rt.rect.height + 40f);
            }
            else
            {
                P1Indicator.gameObject.SetActive(false);
            }

            if (MetaState.Instance.Player2Credits > 0 && Player2HighlightedCharacter >= 0)
            {
                P2Indicator.gameObject.SetActive(true);
                var rt = CharacterPanels[Player2HighlightedCharacter];
                P2Indicator.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - rt.rect.height + 40f);
            }
            else
            {
                P2Indicator.gameObject.SetActive(false);
            }

            //set all unselected except selected index
            for (int i = 0; i < CharacterPanels.Length; i++)
            {
                if (Player1SelectedCharacter == i || Player2SelectedCharacter == i)
                {
                    CharacterPanels[i].transform.GetChild(0).gameObject.SetActive(false);
                    CharacterPanels[i].transform.GetChild(1).gameObject.SetActive(true);
                }
                else
                {
                    CharacterPanels[i].transform.GetChild(0).gameObject.SetActive(true);
                    CharacterPanels[i].transform.GetChild(1).gameObject.SetActive(false);
                }
            }

            //set the closeup image
            if(Player1SelectedCharacter >= 0 && Player2SelectedCharacter < 0)
            {
                for(int i = 0; i < CloseupPanels.Length; i++)
                {
                    if (i == Player1SelectedCharacter)
                        CloseupPanels[i].gameObject.SetActive(true);
                    else
                        CloseupPanels[i].gameObject.SetActive(false);
                }
            }
            else
            {
                //hide ALL closeup images
                foreach (var panel in CloseupPanels)
                    panel.gameObject.SetActive(false);
            }

            //set the hint panel
            if(Player1SelectedCharacter >= 0) //logic would break if we allow only-P2 start
            {
                StartHintObject.SetActive(true);
            }
            else
            {
                StartHintObject.SetActive(false);
            }
        }

        private IEnumerator CoExitScene()
        {
            SetCharacterSelections();

            AudioPlayer.Instance.PlayUISound("char_startgame");
            ScreenFader.FadeTo(Color.black, 3.0f, false, false, false);
            yield return new WaitForSeconds(3f);

            if (Player1SelectedCharacter >= 0)
                MetaState.Instance.Player1Credits--;
            if (Player2SelectedCharacter >= 0)
                MetaState.Instance.Player2Credits--;
            
            SharedUtils.ChangeScene("Mission1Scene");
        }

        private void SetCharacterSelections()
        {
            //this is a stupid way of doing it, but really we should have never stored strings in GameState
            switch (Player1SelectedCharacter)
            {
                case 0:
                    GameState.Instance.Player1Character = "Marco";
                    break;
                case 1:
                    GameState.Instance.Player1Character = "Sharon";
                    break;
                case 2:
                    GameState.Instance.Player1Character = "Joakim";
                    break;
                case 3:
                    GameState.Instance.Player1Character = "Alissa";
                    break;
                default:
                    GameState.Instance.Player1Character = null;
                    break;
            }

            switch (Player2SelectedCharacter)
            {
                case 0:
                    GameState.Instance.Player2Character = "Marco";
                    break;
                case 1:
                    GameState.Instance.Player2Character = "Sharon";
                    break;
                case 2:
                    GameState.Instance.Player2Character = "Joakim";
                    break;
                case 3:
                    GameState.Instance.Player2Character = "Alissa";
                    break;
                default:
                    GameState.Instance.Player2Character = null;
                    break;
            }
        }
    }
}