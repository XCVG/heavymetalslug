using CommonCore.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.SideScrollerGame
{
    /// <summary>
    /// Controller for the "mission start" splash text
    /// </summary>
    public class MissionStartSplashController : MonoBehaviour
    {
        [SerializeField]
        private Text MissionText = null;
        private float TimeLeft;

        public void SetupSplash(float time, string missionText, string sound)
        {
            TimeLeft = time;
            MissionText.text = missionText;
            AudioPlayer.Instance.PlayUISound(sound);
        }

        private void Update()
        {
            TimeLeft -= Time.deltaTime;

            if (TimeLeft <= 0)
                Destroy(gameObject);
        }
    }
}