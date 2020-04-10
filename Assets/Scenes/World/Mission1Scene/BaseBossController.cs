using CommonCore.SideScrollerGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug.Mission1
{

    /// <summary>
    /// Controller for the end boss/arena fight
    /// </summary>
    public class BaseBossController : MonoBehaviour
    {
        [SerializeField] //normally wouldn't do things this way but I'm in a hurry
        private SideScrollerSceneController SceneController = null;
        [SerializeField]
        private CameraFollowScript CameraFollowScript = null;

        [SerializeField]
        private Transform CameraTargetTransform = null;
        [SerializeField]
        private GameObject AfterBattleHintObject = null;

        [SerializeField]
        private HelicopterController Helicopter = null;
        [SerializeField]
        private GameObject BlockObject = null;

        private bool FightStarted = false;
        private bool FightEnded = false;

        private void Update()
        {
            //we listen here because it's stupid but it works so it's not stupid

            if(FightStarted && !FightEnded)
            {
                if(Helicopter.IsDead)
                {
                    FightEnded = true;
                    HandleBossFightEnded();
                }
            }
        }

        public void HandleArenaReached()
        {
            StartCoroutine(CoStartBossFight());
        }

        private IEnumerator CoStartBossFight()
        {
            BlockObject.SetActive(true);

            //move camera
            SceneController.UseCameraMovementBounds = true;
            CameraFollowScript.FollowPlayer = false;
            CameraFollowScript.MoveTo(CameraTargetTransform.position, 5f);

            //wait for camera move
            yield return new WaitForSeconds(2f);
            Helicopter.gameObject.SetActive(true);

            FightStarted = true;
        }

        public void HandleBossFightEnded()
        {

            //unlock camera, display hint
            SceneController.UseCameraMovementBounds = false;
            CameraFollowScript.CancelMove();
            CameraFollowScript.FollowPlayer = true;
            if (AfterBattleHintObject != null)
                AfterBattleHintObject.SetActive(true);
            BlockObject.SetActive(false); //so I guess you could go back if you wanted but why?
        }
    }
}