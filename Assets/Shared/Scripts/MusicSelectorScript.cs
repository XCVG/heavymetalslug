using CommonCore.Audio;
using CommonCore.State;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug
{

    public class MusicSelectorScript : MonoBehaviour
    {
        [SerializeField]
        private List<MusicChoice> Music = null;

        private void Start()
        {
            string character = GameState.Instance.Player1Character;
            foreach(var mc in Music)
            {
                if(string.Equals(mc.Character, character, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"Selected music \"{mc.Track}\"");
                    AudioPlayer.Instance.SetMusic(mc.Track, MusicSlot.Ambient, mc.Volume, true, false);
                    AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);
                    break;
                }
            }
        }

        [Serializable]
        public struct MusicChoice
        {
            public string Character;
            public string Track;
            public float Volume;
        }
    }
}