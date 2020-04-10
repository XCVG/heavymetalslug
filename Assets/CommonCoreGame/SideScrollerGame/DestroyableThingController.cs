using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore.SideScrollerGame
{
    /// <summary>
    /// Controller for a destroyable thing
    /// </summary>
    public class DestroyableThingController : MonoBehaviour, ITakeDamage
    {
        public float Health = 1;
        public float MaxHealth = 1;

        [SerializeField]
        private GameObject DeathEffect = null;
        [SerializeField]
        private UnityEvent DeathEvent = null;
        [SerializeField]
        private float DwellTime = 10f;
        [SerializeField]
        private Animator Animator = null;
        [SerializeField]
        private AudioSource DieSound = null;
        [SerializeField]
        private int GrantScore = 0;
        [SerializeField]
        private bool DeactivateOnDeath = false;
        

        private bool IsDead = false;
        private float TimeSinceDeath = 0;

        private void Update()
        {
            if (IsDead && DwellTime >= 0)
            {
                TimeSinceDeath += Time.deltaTime;

                if (TimeSinceDeath > DwellTime)
                    Destroy(gameObject);
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead)
                return;

            Health -= damage;

            if (Health <= 0)
                Die();
        }

        private void Die()
        {
            IsDead = true;

            if(Animator != null)
            {
                if (Animator.enabled)
                    Animator.Play("Die");
                else
                    Animator.enabled = true;
            }

            DieSound.Ref()?.Play();

            if(DeathEffect != null)
            {
                Instantiate(DeathEffect, transform.position, Quaternion.identity, CoreUtils.GetWorldRoot());
            }

            if(DeathEvent != null)
            {
                DeathEvent.Invoke();
            }

            if (DeactivateOnDeath)
                gameObject.SetActive(false);

            if (GrantScore > 0)
                GameState.Instance.Player1Score += GrantScore;
        }
    }
}