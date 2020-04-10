using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore.SideScrollerGame
{
    /// <summary>
    /// Triggers a UnityEvent when the player or an actor crosses
    /// </summary>
    /// <remarks>Ersatz-ActionSpecialTrigger. Really need to port World to 2D along with ObjectActions.</remarks>
    public class ColliderTrigger : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent Event = null;
        [SerializeField]
        private bool Repeatable = true;
        [SerializeField]
        private bool PlayerCanTrigger = true;
        [SerializeField]
        private bool ActorCanTrigger = false;

        private bool Triggered = false;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Triggered && !Repeatable)
                return;

            var pc = collision.gameObject.GetComponent<PlayerController>();
            bool otherIsPlayer = collision.gameObject.tag == "Player" || pc != null;
            if((otherIsPlayer && PlayerCanTrigger) || (!otherIsPlayer && ActorCanTrigger))
            {
                if (Event != null)
                    Event.Invoke();

                Triggered = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (Triggered && !Repeatable)
                return;

            var pc = collision.gameObject.GetComponent<PlayerController>();
            bool otherIsPlayer = collision.gameObject.tag == "Player" || pc != null;
            if((otherIsPlayer && PlayerCanTrigger) || (!otherIsPlayer && ActorCanTrigger))
            {
                if (Event != null)
                    Event.Invoke();

                Triggered = true;
            }

        }
    }
}