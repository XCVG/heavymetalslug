using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for a treasure item. Touch to grant score!
    /// </summary>
    public class TreasureController : MonoBehaviour
    {
        [SerializeField]
        private float DwellTime = 10f;
        [SerializeField]
        private Animator Animator = null;
        [SerializeField]
        private AudioSource GrantSound = null;
        [SerializeField]
        private int Score = 256;

        private bool Triggered = false;
        private float TimeSinceTriggered = 0;

        private void Update()
        {
            if(Triggered)
            {
                TimeSinceTriggered += Time.deltaTime;

                if (TimeSinceTriggered > DwellTime)
                    Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (Triggered)
                return;

            //Debug.Log($"Treasure hit by {collision.gameObject}");

            var pc = collision.gameObject.GetComponent<PlayerController>();
            bool otherIsPlayer = collision.gameObject.tag == "Player" || pc != null;
            if (otherIsPlayer)
            {
                if (pc.PlayerNumber == 1)
                    GameState.Instance.Player1Score += Score;
                else if (pc.PlayerNumber == 2)
                    GameState.Instance.Player2Score += Score;

                if (Animator != null)
                    Animator.enabled = true;

                if (GrantSound != null)
                    GrantSound.Play();

                Triggered = true;
            }
        }
    }
}