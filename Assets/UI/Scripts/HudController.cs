using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.UI;
using CommonCore.State;
using CommonCore.SideScrollerGame;
using UnityEngine.UI;

namespace Slug
{
    /// <summary>
    /// Slug hud controller. Note that it's not a real HudController so subtitles etc won't work
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [SerializeField, Header("Player 1")]
        private PlayerController Player1 = null;
        [SerializeField]
        private Text P1ScoreText = null;
        [SerializeField]
        private Text P1AmmoText = null;
        [SerializeField]
        private Text P1BombText = null;
        [SerializeField]
        private Text P1CreditText = null;

        [SerializeField, Header("Player 2")]
        private PlayerController Player2 = null;
        [SerializeField]
        private GameObject Player2Hud = null;
        [SerializeField]
        private Text P2ScoreText = null;
        [SerializeField]
        private Text P2AmmoText = null;
        [SerializeField]
        private Text P2BombText = null;
        [SerializeField]
        private Text P2CreditText = null;

        private void Update()
        {
            //update ALL the things
            {
                P1ScoreText.text = GameState.Instance.Player1Score.ToString("##00000000");
                P1AmmoText.text = Player1.Ammo > 0 ? Player1.Ammo.ToString() : "INF";
                P1BombText.text = Player1.Bombs.ToString();
                P1CreditText.text = $"CREDIT: {MetaState.Instance.Player1Credits}";
            }

            if(Player2 != null && GameState.Instance.Player2Active)
            {
                Player2Hud.SetActive(true);
                P2ScoreText.text = GameState.Instance.Player2Score.ToString("##00000000");
                P2AmmoText.text = Player2.Ammo > 0 ? Player2.Ammo.ToString() : "INF";
                P2BombText.text = Player2.Bombs.ToString();
                P2CreditText.text = $"CREDIT: {MetaState.Instance.Player2Credits}";
            }
            else
            {
                Player2Hud.SetActive(false);
            }
        }
    }
}