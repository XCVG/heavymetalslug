using CommonCore.LockPause;
using CommonCore.State;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for enemies
    /// </summary>
    public class HelicopterController : MonoBehaviour, ITakeDamage
    {
        private const float MoveThreshold = 0.25f;

        public bool IsCrashing = false;
        public bool IsDead = false;
        public bool CanMove = true;
        public bool CanAttack = true;

        [Header("Components"), SerializeField]
        private Rigidbody2D Rigidbody = null;
        [SerializeField]
        private Collider2D Collider = null;
        [SerializeField]
        private Transform ShootPoint = null;
        [SerializeField]
        private GameObject ModelObject = null;
        [SerializeField]
        private GameObject DamagedEffectObject = null;

        [Header("Sounds"), SerializeField]
        private AudioSource AttackSound = null;
        [SerializeField]
        private AudioSource DieSound = null;
        [SerializeField]
        private AudioSource IdleSound = null;

        [Header("Health")]
        public float Health = 25;
        public float MaxHealth = 25;
        [SerializeField]
        private float DamageDisplayRatio = 0.33f;

        [Header("Scoring"), SerializeField]
        private int GrantScore = 2000;

        [Header("Movement"), SerializeField]
        private float MoveSpeed = 1.0f;
        [SerializeField]
        private float MoveJitter = 1.0f;
        [SerializeField]
        private Vector2 TargetCenter = default;
        [SerializeField]
        private Vector2 TargetRadius = new Vector2(5f, 3f);
        [SerializeField]
        private float TargetHoldTime = 5f;

        [Header("Attack"), SerializeField]
        private float AttackDamage = 1;
        [SerializeField]
        private float AttackInterval = 1;
        [SerializeField]
        private float AttackWarmup = 0.5f;
        [SerializeField]
        private int BulletBurstCount = 5;
        [SerializeField]
        private float BulletVelocity = 5f;
        [SerializeField]
        private GameObject BulletPrefab = null;

        [Header("Misc"), SerializeField]
        private GameObject DeathEffectPrefab = null;
        [SerializeField]
        private GameObject DamageEffectPrefab = null;

        private Vector2? TargetPoint;

        private bool ApproachDone;
        private float TimeAtTarget;

        private EnemyAttackState AttackState;
        private float AttackTimeInState;
        private uint BulletsFiredInBurst;

        private void Update()
        {
            if (LockPauseModule.IsPaused())
                return;

            HandleMovement();
            HandleAttack();
            HandleDeathFall();
        }


        private void HandleMovement()
        {
            if (!CanMove || IsDead || IsCrashing)
                return;

            //new move logic: move to random point, hold until attack is done, repeat
            if(ApproachDone)
            {
                TimeAtTarget += Time.deltaTime;

                if(TimeAtTarget >= TargetHoldTime)
                {
                    ApproachDone = false;
                    TimeAtTarget = 0;
                    TargetPoint = null;
                }
                else
                {
                    Vector2 jitterMove = new Vector2(UnityEngine.Random.Range(-MoveJitter, MoveJitter), UnityEngine.Random.Range(-MoveJitter, MoveJitter));

                    Rigidbody.MovePosition(TargetPoint.Value + (jitterMove * Time.deltaTime));
                }
            }
            else
            {
                if(!TargetPoint.HasValue)
                {
                    //determine next target
                    TargetPoint = TargetCenter + new Vector2(UnityEngine.Random.Range(-TargetRadius.x, TargetRadius.x), UnityEngine.Random.Range(-TargetRadius.y, TargetRadius.y));
                }

                Vector2 vecToTarget = (Vector2)TargetPoint - (Vector2)transform.position;
                float distToTarget = vecToTarget.magnitude;

                Vector2 jitterMove = new Vector2(UnityEngine.Random.Range(-MoveJitter, MoveJitter), UnityEngine.Random.Range(-MoveJitter, MoveJitter));

                Rigidbody.MovePosition(Rigidbody.position + vecToTarget.normalized * Mathf.Min(Time.deltaTime * MoveSpeed, distToTarget) + jitterMove * Time.deltaTime);

                if(distToTarget < MoveThreshold)
                {
                    ApproachDone = true;                    
                }
            }

        }

        private void HandleAttack()
        {
            if (!CanAttack || IsDead || IsCrashing)
                return;

            if (!ApproachDone && AttackState != EnemyAttackState.Approaching)
            {
                AttackState = EnemyAttackState.Approaching;
                AttackTimeInState = 0;
                BulletsFiredInBurst = 0;
            }

            if (ApproachDone && AttackState == EnemyAttackState.Approaching)
                AttackState = EnemyAttackState.Waiting;

            if(AttackState != EnemyAttackState.Approaching)
            {
                switch (AttackState)
                {
                    case EnemyAttackState.WarmingUp:
                        if(AttackTimeInState > AttackWarmup)
                        {
                            //enter fire state
                            AttackState = EnemyAttackState.Firing;
                            AttackTimeInState = 0;
                        }
                        break;
                    case EnemyAttackState.Firing:
                        {
                            AttackSound.Ref()?.Play();

                            DoAttack();
                            unchecked
                            {
                                BulletsFiredInBurst++;
                            }

                            //immediately exit, at least for now
                            AttackState = EnemyAttackState.Waiting;
                            AttackTimeInState = 0;
                        }
                        break;
                    case EnemyAttackState.Waiting:
                        if(((BulletBurstCount <= 1 || BulletsFiredInBurst < BulletBurstCount) && AttackTimeInState > AttackInterval))
                        {
                            //enter fire state
                            AttackState = EnemyAttackState.Firing;
                            AttackTimeInState = 0;
                            if(BulletsFiredInBurst >= BulletBurstCount)
                                BulletsFiredInBurst = 0;
                        }
                        break;
                }

                AttackTimeInState += Time.deltaTime;
            }

        }

        private void HandleDeathFall()
        {
            if (!IsDead || !IsCrashing)
                return;

            //nop because we decided to use gravity instead
        }

        private void DoAttack()
        {
            if(BulletPrefab != null)
            {
                //shoot point
                Transform shootPoint = ShootPoint.Ref() ?? transform;

                //velocity vector
                Vector2 velocityVector = Vector2.down * BulletVelocity;

                //do bullet attack
                var go = Instantiate(BulletPrefab, shootPoint.position, Quaternion.identity, CoreUtils.GetWorldRoot());
                var rb = go.GetComponent<Rigidbody2D>();
                rb.velocity = velocityVector;
                var bs = go.GetComponent<BulletScript>();
                bs.Damage = AttackDamage;
            }
        }

        public void TakeDamage(float damage)
        {
            if (DamageEffectPrefab != null)
                Instantiate(DamageEffectPrefab, transform.position, Quaternion.identity, CoreUtils.GetWorldRoot());

            if (Health / MaxHealth <= DamageDisplayRatio && DamagedEffectObject != null && !DamagedEffectObject.activeSelf)
                DamagedEffectObject.SetActive(true);

            if (MaxHealth > 0)
            {
                Health -= damage;
                if (Health <= 0 && !IsDead)
                    StartDeathSequence();
            }
        }

        public void StartDeathSequence()
        {
            IsCrashing = true;
            Rigidbody.gravityScale = 1;
        }

        public void Kill()
        {            
            IsDead = true;
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.isKinematic = true;
            Collider.enabled = false;
            AttackState = EnemyAttackState.Approaching;
            AttackTimeInState = 0;
            DamagedEffectObject.Ref()?.SetActive(false);
            IdleSound.Ref()?.Stop();
            DieSound.Ref()?.Play();

            if (DeathEffectPrefab != null)
                Instantiate(DeathEffectPrefab, transform.position, transform.rotation, CoreUtils.GetWorldRoot());

            StartCoroutine(CoHideModel());

            if(GrantScore > 0)
            {
                //TODO 1P/2P handling, we would need to pass the bullet or HitInfo in ITakeDamage like RpgGame and add a property to BulletScript for which player fired or something
                GameState.Instance.Player1Score += GrantScore;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //we only use this to detect hitting the ground/object after crashing

            if (!IsCrashing)
                return;

            var collider = collision.collider;

            if(collider.gameObject.layer == LayerMask.NameToLayer("Ground") || collider.gameObject.layer == LayerMask.NameToLayer("Default") || collider.gameObject.layer == LayerMask.NameToLayer("Actor"))
            {
                IsCrashing = false;
                Kill();
            }
        }

        private IEnumerator CoHideModel()
        {
            yield return new WaitForSeconds(0.1f);
            ModelObject.SetActive(false);
        }

        private enum EnemyAttackState
        {
            Approaching, WarmingUp, Firing, Waiting
        }
    }
}