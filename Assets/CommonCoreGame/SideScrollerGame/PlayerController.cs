using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.LockPause;
using CommonCore.Input;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for the player
    /// </summary>
    /// <remarks>
    /// <para>Based on https://www.mooict.com/unity-2d-platformer/ against my better judgement</para>
    /// <para>Yeah there's no actual handling for changing player sprites for different characters or weapons here... so have fun with that</para>
    /// </remarks>
    public class PlayerController : MonoBehaviour, ITakeDamage
    {
        private const float MoveAnimateDeadzone = 0.05f;
        private const float CrouchDeadzone = 0.25f;

        //these can be manipulated through scripting
        public int PlayerNumber = 1;
        public bool PlayerInControl = true;
        public bool PlayerIsDead = false;
        public bool PlayerCanShoot = true;
        public bool PlayerIsInvulnerable = false;
        public Rect MovementBounds = Rect.zero;
        public int Ammo = -1;
        public float Bombs = 0;

        [SerializeField, Header("Components")]
        private Transform GroundCheckTransform = null;
        [SerializeField]
        private Rigidbody2D Rigidbody = null;
        public Rigidbody2D Rbody => Rigidbody;
        [SerializeField]
        private CapsuleCollider2D Collider = null;
        [SerializeField]
        private SpriteRenderer PlayerSprite = null;
        [SerializeField]
        private Animator PlayerAnimator = null;
        [SerializeField]
        private SpriteRenderer GunSprite = null;
        [SerializeField]
        private Transform GrenadeThrowPoint = null;

        [Header("Sounds"), SerializeField]       
        private AudioSource DieSound = null;
        [SerializeField]
        private AudioSource RespawnSound = null;

        [SerializeField, Header("Movement Options")]
        private float Speed = 1.0f;
        [SerializeField]
        private float CrouchSpeed = 0.5f;
        [SerializeField]
        private float JumpSpeed = 5.5f;
        [SerializeField]
        private float TimeBetweenJumps = 0.5f;
        [SerializeField]
        private float MinDistToBounds = 1.0f;
        [SerializeField]
        private float BoundsBounceVelocity = 0.1f;
        [SerializeField]
        private float BoundsBounceDisplacement = 0.05f;

        [SerializeField, Header("Health/Damage")]
        private float Health = 1f;
        [SerializeField]
        private float MaxHealth = 1f;

        [SerializeField, Header("Grenade")]
        private GameObject GrenadePrefab = null;
        [SerializeField]
        private float GrenadeVelocity = 5f;
        [SerializeField]
        private float GrenadeAngle = 45f;

        [SerializeField, Header("Other")]
        private float DeadFallSpeed = 5.0f;
        [SerializeField]
        private float CrouchYScale = 0.75f;
        [SerializeField]
        private float CorpseSpawnDelay = 1.0f;
        [SerializeField]
        private GameObject CorpsePrefab = null;        

        public bool IsCrouched { get; private set; }
        public bool IsTouchingGround { get; private set; }

        private LayerMask GroundLayerMask; 
        private float ScaleX;
        private float ScaleY;
        private float TimeUntilNextJump;
        private float TimeUntilNoInvulnerability;

        //crouch handling
        private DefaultSizeData DefaultSizes;

        private bool DidRequestJump;
        private Coroutine CorpseSpawnCoroutine;

        private void Start()
        {
            ScaleX = transform.localScale.x;
            ScaleY = transform.localScale.y;

            GroundLayerMask = LayerMask.GetMask("Ground");
        }

        private void Update()
        {
            if (LockPauseModule.IsPaused())
                return;

            HandleCriticalInput();
            HandleInvulnerability();
            HandleGrenadeThrow();
        }

        private void FixedUpdate()
        {
            if (LockPauseModule.IsPaused())
                return;

            if (TimeUntilNextJump > 0)
                TimeUntilNextJump -= Time.fixedDeltaTime;


            HandleMovement();
            HandleBounds();
            HandleDeathGravity();
        }

        private void HandleCriticalInput()
        {
            //we do this here because it shits the bed in FixedUpdate
            DidRequestJump |= GetButtonDown(DefaultControls.Jump);
        }

        private void HandleInvulnerability()
        {
            //countdown and disable invulnerability
            if(TimeUntilNoInvulnerability > 0)
            {
                TimeUntilNoInvulnerability -= Time.deltaTime;
                if(TimeUntilNoInvulnerability <= 0)
                {
                    PlayerIsInvulnerable = false;
                    TimeUntilNoInvulnerability = -1;
                    DisableInvulnerabilityEffect();
                }
            }
        }

        private void HandleGrenadeThrow()
        {
            if (LockPauseModule.IsInputLocked() || !PlayerInControl || PlayerIsDead || !PlayerCanShoot)
                return;

            if(GetButtonDown("Fire2") && Bombs > 0)
            {
                Vector2 fireVector = (Quaternion.AngleAxis(GrenadeAngle, Vector3.forward) * Vector3.right) * new Vector2(Mathf.Sign(transform.localScale.x), 1);
                Quaternion bulletRotation = Quaternion.FromToRotation(Vector3.right, fireVector);

                var go = Instantiate(GrenadePrefab, GrenadeThrowPoint.position, bulletRotation, CoreUtils.GetWorldRoot());
                var rb = go.GetComponent<Rigidbody2D>();
                rb.velocity = (fireVector * GrenadeVelocity);// + Rigidbody.velocity;
                
                Bombs--;
            }
        }

        private void HandleMovement()
        {
            if (LockPauseModule.IsInputLocked() || !PlayerInControl || PlayerIsDead)
                return;

            float h = GetAxis(DefaultControls.MoveX);

            IsTouchingGround = Physics2D.OverlapPoint(GroundCheckTransform.position, GroundLayerMask); //move outside of PlayerIsDead check?

            if (DidRequestJump && TimeUntilNextJump <= 0)
            {              
                if (IsTouchingGround)
                {
                    Rigidbody.AddForce(new Vector2(0, JumpSpeed * Rigidbody.mass), ForceMode2D.Impulse);
                    IsTouchingGround = false;
                    TimeUntilNextJump = TimeBetweenJumps;
                    PlayerAnimator.Play("Jump");
                }
                DidRequestJump = false;
            }

            if (h > 0)
            {
                transform.localScale = new Vector2(ScaleX, ScaleY);
            }
            else if (h < 0)
            {
                transform.localScale = new Vector2(-ScaleX, ScaleY);
            }
            Rigidbody.velocity = new Vector2(h * (IsCrouched ? CrouchSpeed : Speed), Rigidbody.velocity.y); //this is going to be fucking horrible

            //handle animation
            if(Mathf.Abs(h) > MoveAnimateDeadzone)
            {
                if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    PlayerAnimator.Play("Walk");
                else if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("CrouchIdle"))
                    PlayerAnimator.Play("CrouchWalk");
            }
            else
            {
                if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                    PlayerAnimator.Play("Idle");
                else if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("CrouchWalk"))
                    PlayerAnimator.Play("CrouchIdle");
            }

            //handle crouch
            float moveY = GetAxis(DefaultControls.MoveY);
            if(moveY < -CrouchDeadzone && IsTouchingGround)
            {
                if(!IsCrouched)
                {
                    EnterCrouch();
                    IsCrouched = true;
                }
            }
            else
            {
                if(IsCrouched)
                {
                    ExitCrouch();
                    IsCrouched = false;
                }
            }
        }

        private void HandleBounds()
        {
            //check bounds, keep player on the screen (critical for mslug)
            if(MovementBounds.width > 0)
            {
                //width > 0, need to check bounds

                //we're essentially thinking of these as 1D vectors
                float xPosToMaxX = MovementBounds.xMax - transform.position.x;
                float distXPosToMaxX = Mathf.Abs(xPosToMaxX);

                if (distXPosToMaxX <= MinDistToBounds)
                {
                    Rigidbody.velocity = new Vector3(-BoundsBounceVelocity, Rigidbody.velocity.y);
                    transform.position = new Vector3(MovementBounds.xMax - MinDistToBounds - BoundsBounceDisplacement, transform.position.y, transform.position.z);
                }

                float xPosToMinX = MovementBounds.xMin - transform.position.x;
                float distXPosToMinX = Mathf.Abs(xPosToMinX);

                if(distXPosToMinX <= MinDistToBounds)
                {
                    Rigidbody.velocity = new Vector3(BoundsBounceVelocity, Rigidbody.velocity.y);
                    transform.position = new Vector3(MovementBounds.xMin + MinDistToBounds + BoundsBounceDisplacement, transform.position.y, transform.position.z);
                }
            }
        }

        private void HandleDeathGravity()
        {
            if(PlayerIsDead)
            {
                bool touchesGround = Physics2D.OverlapPoint(GroundCheckTransform.position, GroundLayerMask);
                if(!touchesGround)
                {
                    Rigidbody.MovePosition(Rigidbody.position + Vector2.down * DeadFallSpeed * Time.fixedDeltaTime);
                }
            }
        }

        //we have this extra layer of redirection for handling P2
        public float GetAxis(string axis)
        {
            if (PlayerNumber > 1)
                return MappedInput.GetAxis($"{axis}P{PlayerNumber}");
            return MappedInput.GetAxis(axis);
        }

        public bool GetButton(string button)
        {
            if (PlayerNumber > 1)
                return MappedInput.GetButton($"{button}P{PlayerNumber}");
            return MappedInput.GetButton(button);
        }

        public bool GetButtonDown(string button)
        {
            if (PlayerNumber > 1)
                return MappedInput.GetButtonDown($"{button}P{PlayerNumber}");
            return MappedInput.GetButtonDown(button);
        }

        public void TakeDamage(float damage)
        {
            if (PlayerIsInvulnerable)
                return;

            Health -= damage;

            if (Health <= 0 && !PlayerIsDead)
                Kill();
        }

        public void Kill()
        {
            Debug.Log($"Player {PlayerNumber} died!");

            PlayerIsDead = true;
            if (IsCrouched)
            {
                ExitCrouch();
                IsCrouched = false;
            }
            //TODO death animation, death sound
            Rigidbody.velocity = Vector2.zero;
            Rigidbody.isKinematic = true;
            Collider.enabled = false;
            DieSound.Ref()?.Play();
            PlayerAnimator.Play("Die");
            GunSprite.enabled = false;
            DisableInvulnerabilityEffect();
            Bombs = 0;
            Ammo = 0;
            SpawnCorpse();
        }

        public void Resurrect() //yes I know there's inconsistency between Resurrect and Respawn terminology... I was tired
        {
            Debug.Log($"Player {PlayerNumber} resurrected!");

            //TODO play spawn animation, spawn sound

            Health = MaxHealth;
            PlayerIsDead = false;            
            Rigidbody.isKinematic = false;
            Collider.enabled = true;
            RespawnSound.Ref()?.Play();
            PlayerAnimator.Play("Spawn");
            GunSprite.enabled = true;
            //DisableInvulnerabilityEffect();
            //stop the corpse spawn coroutine?
        }

        public void SetSpawnAnimation() //plays the spawn anim
        {
            PlayerAnimator.Play("Spawn");
        }

        public void SetSpawnInvulnerability(float invulnerableTime)
        {
            //spawn invulnerability
            PlayerIsInvulnerable = true;
            TimeUntilNoInvulnerability = invulnerableTime;

            EnableInvulnerabilityEffect();
        }

        //these only affect visual appearance
        private void EnableInvulnerabilityEffect()
        {
            if(PlayerSprite != null)
            {
                PlayerSprite.color = new Color(1f, 1f, 1f, 0.5f);
            }
            if (GunSprite != null)
            {
                GunSprite.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        private void DisableInvulnerabilityEffect()
        {
            if (PlayerSprite != null)
            {
                PlayerSprite.color = Color.white;
            }
            if (GunSprite != null)
            {
                GunSprite.color = Color.white;
            }
        }

        private void EnterCrouch()
        {
            if (DefaultSizes == null)
                DefaultSizes = new DefaultSizeData() { ColliderHeight = Collider.size.y, ColliderOffset = Collider.offset.y };

            Collider.size = new Vector2(Collider.size.x, DefaultSizes.ColliderHeight * CrouchYScale);
            Collider.offset = new Vector2(Collider.offset.x, DefaultSizes.ColliderOffset * CrouchYScale);

            //handle animation
            if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                PlayerAnimator.Play("CrouchIdle");
            else if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                PlayerAnimator.Play("CrouchWalk");
            else if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
                PlayerAnimator.Play("CrouchIdle");
        }

        private void ExitCrouch()
        {
            Collider.size = new Vector2(Collider.size.x, DefaultSizes.ColliderHeight);
            Collider.offset = new Vector2(Collider.offset.x, DefaultSizes.ColliderOffset);

            if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("CrouchIdle"))
                PlayerAnimator.Play("Idle");
            else if (PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName("CrouchWalk"))
                PlayerAnimator.Play("Walk");

        }

        private void SpawnCorpse()
        {
            if(CorpsePrefab != null && CorpseSpawnDelay >= 0)
            {
                CorpseSpawnCoroutine = StartCoroutine(CoSpawnCorpse());
            }
        }

        private IEnumerator CoSpawnCorpse()
        {
            yield return new WaitForSeconds(CorpseSpawnDelay);
            var go = Instantiate(CorpsePrefab, transform.position, Quaternion.identity, CoreUtils.GetWorldRoot());
            go.transform.localScale = Vector3.Scale(go.transform.localScale, new Vector3(Mathf.Sign(transform.localScale.x), 1, 1)); //flip handling
        }

        private class DefaultSizeData //not all that much data tbh
        {
            public float ColliderHeight;
            public float ColliderOffset;
        }
    }
}