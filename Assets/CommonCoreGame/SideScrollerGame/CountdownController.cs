using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for the respawn countdown
    /// </summary>
    public class CountdownController : MonoBehaviour
    {
        [SerializeField]
        private Text CountdownText = null;
        private float TimeLeft;

        //TODO sound effect

        public void SetupCountdown(float time)
        {
            TimeLeft = time;
        }

        private void Update()
        {
            TimeLeft -= Time.deltaTime;

            if (TimeLeft <= 0)
                Destroy(gameObject);

            CountdownText.text = Mathf.RoundToInt(TimeLeft).ToString("00");
        }
    }
}