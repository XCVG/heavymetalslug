using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Slug
{

    /// <summary>
    /// Displays the score in the endgame
    /// </summary>
    public class GameOverScoreScript : MonoBehaviour
    {
        //abandon all pretense of P2 support ye who enter here
        [SerializeField]
        private Text ScoreText = null;

        private void Update()
        {
            ScoreText.text = $"SCORE: {GameState.Instance.Player1Score}";
        }
    }
}