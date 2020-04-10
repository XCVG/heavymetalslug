using CommonCore.Input;
using CommonCore.LockPause;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for the player's gun/attacks
    /// </summary>
    public class PlayerGunController : MonoBehaviour
    {
        private const float AimDeadzone = 0.25f;
        private const float AimDownDeadzone = 0.33f; //TODO may revise this

        [SerializeField]
        private PlayerController PlayerController = null;
        [SerializeField]
        private Transform GunPivot = null;
        [SerializeField]
        private Transform GunPivotCrouched = null;
        [SerializeField]
        private Transform GunObject = null;
        [SerializeField]
        private SpriteRenderer GunSprite = null;
        [SerializeField]
        private Animator GunAnimator = null;

        [SerializeField]
        private AudioSource AttackSound = null;

        [SerializeField, Header("Gun Parameters")]
        private float FireInterval = 0.25f;

        [SerializeField, Header("Bullet Parameters")]
        private float BulletDamage = 1;
        [SerializeField]
        private float BulletVelocity = 10f;
        [SerializeField]
        private GameObject BulletPrefab = null;

        private float TimeUntilNextShot;
        private bool DidJustFire;
        private Vector2 LastFireVector;

        //TODO STRETCH GOAL: temporary upgrades

        private void Update()
        {
            if (LockPauseModule.IsPaused())
                return;

            if (TimeUntilNextShot > 0)
                TimeUntilNextShot -= Time.deltaTime;

            HandleShooting();
            HandlePivot();
        }

        private void HandleShooting()
        {
            DidJustFire = false;

            if (LockPauseModule.IsInputLocked() || !PlayerController.PlayerInControl || PlayerController.PlayerIsDead || !PlayerController.PlayerCanShoot)
                return;

            if(PlayerController.GetButtonDown(DefaultControls.Fire) && TimeUntilNextShot <= 0)
            {
                var gunPivot = PlayerController.IsCrouched ? GunPivotCrouched : GunPivot;

                //aiming!
                Vector2 fireVector = (Vector2)transform.right * Mathf.Sign(PlayerController.transform.localScale.x);
                float spawnDistance = ((Vector2)transform.position - (Vector2)GunPivot.position).magnitude;                
                float aimY = PlayerController.GetAxis("Vertical");
                if (aimY > AimDeadzone || (!PlayerController.IsTouchingGround && aimY < -AimDownDeadzone))
                {
                    float aimX = PlayerController.GetAxis("Horizontal");
                    fireVector = new Vector2(aimX, aimY).normalized;                    
                }
                Vector2 spawnPosition = (Vector2)gunPivot.position + fireVector * spawnDistance; //this technique misaligns the bullet if the muzzle isn't aligned with the pivot. Can you fix it?
                LastFireVector = fireVector;

                Quaternion bulletRotation = Quaternion.FromToRotation(Vector3.right, fireVector);

                var go = Instantiate(BulletPrefab, spawnPosition, bulletRotation, CoreUtils.GetWorldRoot());
                var rb = go.GetComponent<Rigidbody2D>();
                rb.velocity = (fireVector * BulletVelocity);// + (Vector2.right * PlayerController.Rbody.velocity.x); 
                var bs = go.GetComponent<BulletScript>();
                bs.Damage = BulletDamage;
                TimeUntilNextShot += FireInterval;
                DidJustFire = true;

                //play effect, anim
                AttackSound.Ref()?.Play();
                GunAnimator.Play("Fire"); //we rely on the Animator to return to idle state
            }
        }

        private void HandlePivot()
        {
            //pivot handling
            if (DidJustFire)
            {
                /*
                var rotation = Quaternion.FromToRotation(Vector3.right, LastFireVector);
                if (PlayerController.transform.localScale.x < 0)
                {
                    rotation *= Quaternion.AngleAxis(180, Vector3.forward);
                }
                GunObject.rotation = rotation;
                */

                //local space works better and avoids weird issues
                var correctedFireVector = LastFireVector.x > 0 ? LastFireVector : new Vector2(-LastFireVector.x, LastFireVector.y);
                var rotation = Quaternion.FromToRotation(Vector3.right, correctedFireVector);
                GunObject.localRotation = rotation;
            }
            else if(TimeUntilNextShot <= 0) //TODO use a separate timer for this?
            {
                GunObject.rotation = Quaternion.identity;
                //GunSprite.flipY = false;
            }

            //crouch handling
            if(PlayerController.IsCrouched)
            {
                GunObject.localPosition = GunPivotCrouched.localPosition;
            }
            else
            {
                GunObject.localPosition = GunPivot.localPosition;
            }
        }
    }

    
}