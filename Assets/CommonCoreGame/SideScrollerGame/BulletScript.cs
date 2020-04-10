using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for a bullet object
    /// </summary>
    public class BulletScript : MonoBehaviour
    {
        public bool CollideWithPlayer = false;
        public bool CollideWithMonster = true;

        public float Damage = 1f;
        public float DwellTime = -1;
        public float DamageRadius = 0;

        [SerializeField, Tooltip("If set, destroys this when terrain is hit")]
        private bool AutoDestroy = false;
        [SerializeField]
        private GameObject DestroyEffect = null;

        private void Start()
        {
            if (DwellTime > 0)
                Invoke(nameof(HandleDwellEnd), DwellTime); //*pukes a little*
        }

        private void HandleDwellEnd()
        {
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision);
        }

        private void HandleCollision(Collider2D collider)
        {
            var pc = collider.GetComponent<PlayerController>();
            bool otherIsPlayer = collider.tag == "Player" || pc != null;
            if (otherIsPlayer && CollideWithPlayer)
            {
                if (pc == null)
                    pc = collider.GetComponentInParent<PlayerController>();

                if (pc != null)
                {
                    pc.TakeDamage(Damage);
                }
                else
                {
                    Debug.LogError($"Bullet \"{name}\" hit a player, but can't find PlayerController!");
                }

                DestroyBullet();
            }
            else if (!otherIsPlayer && CollideWithMonster)
            {
                var itd = collider.GetComponent<ITakeDamage>();
                if (itd == null)
                    itd = collider.GetComponentInParent<ITakeDamage>();

                if (itd != null)
                    itd.TakeDamage(Damage);

                DestroyBullet();
            }
            else if (AutoDestroy && (collider.gameObject.layer == LayerMask.NameToLayer("Ground") || collider.gameObject.layer == LayerMask.NameToLayer("Default")))
            {
                DestroyBullet();
            }
        }

        private void DestroyBullet()
        {
            if (DestroyEffect != null)
                Instantiate(DestroyEffect, transform.position, transform.rotation, CoreUtils.GetWorldRoot());

            if(DamageRadius > 0)
            {
                //no damage falloff, just a simple radius
                var overlapped = Physics2D.OverlapCircleAll(transform.position, DamageRadius, LayerMask.GetMask("Default", "Actor"));
                foreach(var obj in overlapped)
                {
                    bool otherIsPlayer = (obj.tag == "Player") || (obj.GetComponent<PlayerController>() != null);

                    var itd = obj.GetComponent<ITakeDamage>();
                    if(itd != null && ((otherIsPlayer && CollideWithPlayer) || (!otherIsPlayer && CollideWithMonster)))
                    {
                        itd.TakeDamage(Damage);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}