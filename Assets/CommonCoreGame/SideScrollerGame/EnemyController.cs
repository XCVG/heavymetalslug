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
    public class EnemyController : MonoBehaviour, ITakeDamage
    {
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
        private Animator Animator = null;

        [Header("Sounds"), SerializeField]
        private AudioSource AttackSound = null;
        [SerializeField]
        private AudioSource DieSound = null;

        [Header("Health")]
        public float Health = 1;
        public float MaxHealth = 1;

        [Header("Scoring")]
        public int GrantScore = 100;

        [Header("Targeting"), SerializeField]
        private bool TargetPlayer = true;
        [SerializeField]
        private GameObject Target;
        [SerializeField]
        private bool AlwaysFaceTarget = true;
        [SerializeField]
        private float ApproachDistance = 5;
        [SerializeField, Tooltip("<0 means approach to approach distance, then stay there")]
        private float MaxDistance = -1; //

        [Header("Movement"), SerializeField]
        private float MoveSpeed = 1.0f;

        [Header("Attack"), SerializeField]
        private float AttackDamage = 1;
        [SerializeField]
        private float AttackInterval = 1;
        [SerializeField]
        private float AttackWarmup = 0.5f;
        [SerializeField]
        private int BulletBurstCount = 1;
        [SerializeField]
        private float BulletBurstInterval = 0;
        [SerializeField]
        private float BulletVelocity = 5f;
        [SerializeField, Tooltip("If set to null, enemy will use melee attack")]
        private GameObject BulletPrefab = null;
        [SerializeField, Tooltip("If set, will aim toward the target instead of firing forward")]
        private bool FireBulletAtTarget = false;
        [SerializeField]
        private float MeleeRange = 1.5f;        

        //TODO boss and helicopter handling

        private bool ApproachDone;
        private EnemyAttackState AttackState;
        private float AttackTimeInState;
        private uint BulletsFiredInBurst;

        private void Update()
        {
            if (LockPauseModule.IsPaused())
                return;

            HandleTargeting();
            HandleMovement();
            HandleAttack();
            HandleDeathGravity();
        }

        private void HandleTargeting()
        {
            if(IsDead)
            {
                //Target = null;
                return;
            }

            if(Target != null)
            {
                var pc = Target.GetComponent<PlayerController>();
                if (pc)
                {
                    if (pc.PlayerIsDead)
                        Target = null;
                }
            }

            if(Target == null)
            {
                if(TargetPlayer)
                {
                    var playerObjects = GameObject.FindGameObjectsWithTag("Player");
                    foreach(var playerObject in playerObjects)
                    {
                        var pc = playerObject.GetComponent<PlayerController>();
                        if(pc != null && !pc.PlayerIsDead)
                        {
                            Target = pc.gameObject;
                            break;
                        }
                    }
                }
            }
        }

        private void HandleMovement()
        {
            if (!CanMove || IsDead)
                return;

            float maxDisplacement = MoveSpeed * Time.deltaTime;
            float displacement = 0;

            //check if we are within range, move if we are not
            if (Target != null)
            {
                float vecToTarget = Target.transform.position.x - transform.position.x;
                float distanceToTarget = Mathf.Abs(vecToTarget);
                bool doApproach = false;
                if (ApproachDone)
                {
                    //approach if MaxDistance >= 0 and distance to target > max distance
                    doApproach = MaxDistance >= 0 && distanceToTarget > MaxDistance;
                    if(doApproach)
                        ApproachDone = false;
                }
                else
                {
                    doApproach = distanceToTarget > ApproachDistance;
                    if (!doApproach)
                        ApproachDone = true;
                }

                if(doApproach || AlwaysFaceTarget)
                {
                    //point the correct direction
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Math.Sign(vecToTarget), transform.localScale.y, transform.localScale.z);
                }

                if (doApproach)
                {
                    displacement = Mathf.Min(maxDisplacement, distanceToTarget) * Math.Sign(vecToTarget);
                    if (Animator != null && Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                        Animator.Play("Move");
                }
            }
            //TODO wander behavior?

            //Rigidbody.velocity = new Vector2(displacement, Rigidbody.velocity.y);
            Rigidbody.MovePosition(Rigidbody.position + (Vector2.right * displacement));
            //Rigidbody.position = Rigidbody.position + (Vector2.right * displacement);


        }

        private void HandleAttack()
        {
            if (!CanAttack || IsDead)
                return;

            if (!ApproachDone && AttackState != EnemyAttackState.Approaching)
            {
                AttackState = EnemyAttackState.Approaching;
                AttackTimeInState = 0;
                BulletsFiredInBurst = 0;
                //TODO cancel animation
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
                            Animator.Ref()?.Play("Attack");
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
                        if(((BulletBurstCount <= 1 || BulletsFiredInBurst < BulletBurstCount) && AttackTimeInState > AttackInterval) ||
                            AttackTimeInState > (AttackInterval + BulletBurstInterval))
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

        private void HandleDeathGravity()
        {
            //hacked in based on player code
            if (IsDead)
            {
                bool touchesGround = Physics2D.OverlapPoint(transform.position - (Vector3.down * 0.1f), LayerMask.GetMask("Ground"));
                if (!touchesGround)
                {
                    Rigidbody.MovePosition(Rigidbody.position + Vector2.down * 5f * Time.deltaTime);
                }
            }
        }

        private void DoAttack()
        {
            if(BulletPrefab != null)
            {
                //shoot point
                Transform shootPoint = ShootPoint.Ref() ?? transform;

                //velocity vector
                Vector2 velocityVector = Vector2.zero;
                if (!FireBulletAtTarget || Target == null)
                    velocityVector = ((Vector2)transform.right * Mathf.Sign(transform.localScale.x) * BulletVelocity);// + Rigidbody.velocity;
                else
                    velocityVector = ((Vector2)Target.transform.position - (Vector2)shootPoint.position).normalized * BulletVelocity;

                //do bullet attack
                var go = Instantiate(BulletPrefab, shootPoint.position, Quaternion.identity, CoreUtils.GetWorldRoot());
                var rb = go.GetComponent<Rigidbody2D>();
                rb.velocity = velocityVector;
                var bs = go.GetComponent<BulletScript>();
                bs.Damage = AttackDamage;
            }
            else
            {
                //do melee attack

                if (Target != null)
                {
                    float distToTarget = ((Vector2)Target.transform.position - (Vector2)transform.position).magnitude;
                    if(distToTarget < MeleeRange)
                    {
                        var itd = Target.GetComponent<ITakeDamage>();
                        if (itd != null)
                            itd.TakeDamage(AttackDamage);
                    }
                }
                
                //if we don't have a target we can't melee attack, nor can we attack something that isn't our target. I'm sure that will never cause any problems whatsoever
            }
        }

        public void TakeDamage(float damage)
        {
            if (MaxHealth > 0)
            {
                Health -= damage;
                if (Health <= 0 && !IsDead)
                    Kill();
            }
        }

        public void Kill()
        {
            IsDead = true;
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.isKinematic = true;
            Collider.enabled = false;
            AttackState = EnemyAttackState.Approaching;
            AttackTimeInState = 0;
            Animator.Ref()?.Play("Die");
            DieSound.Ref()?.Play();

            if(GrantScore > 0)
            {
                //TODO 1P/2P handling, we would need to pass the bullet or HitInfo in ITakeDamage like RpgGame and add a property to BulletScript for which player fired or something
                GameState.Instance.Player1Score += GrantScore;
            }
        }

        private enum EnemyAttackState
        {
            Approaching, WarmingUp, Firing, Waiting
        }
    }
}