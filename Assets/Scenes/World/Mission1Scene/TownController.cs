using CommonCore;
using CommonCore.SideScrollerGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug.Mission1
{

    /// <summary>
    /// Controls the town square battle... yeah pretty much just that
    /// </summary>
    public class TownController : MonoBehaviour
    {
        [SerializeField] //normally wouldn't do things this way but I'm in a hurry
        private SideScrollerSceneController SceneController = null;
        [SerializeField]
        private CameraFollowScript CameraFollowScript = null;

        [SerializeField]
        private Transform CameraTargetTransform = null;
        [SerializeField]
        private GameObject TownCenterHintObject = null;

        [SerializeField]
        private int EnemiesToSpawn = 10;
        [SerializeField]
        private float EnemySpawnRate = 4f;
        [SerializeField]
        private GameObject EnemySpawnPrefab = null;
        [SerializeField]
        private Transform[] EnemySpawnPoints = null;

        private bool SpawnInTownCenter = false;
        private bool TownCenterCleared = false;
        private float TimeSinceLastSpawn = 0;
        private int EnemiesSpawned = 0;
        private List<GameObject> SpawnedEnemies = new List<GameObject>();

        private void Update()
        {
            if(SpawnInTownCenter)
            {
                TimeSinceLastSpawn += Time.deltaTime;

                if (TimeSinceLastSpawn >= EnemySpawnRate && EnemiesSpawned < EnemiesToSpawn)
                {
                    var spawnPoint = EnemySpawnPoints[UnityEngine.Random.Range(0, EnemySpawnPoints.Length)];
                    var go = Instantiate(EnemySpawnPrefab, spawnPoint.position, Quaternion.identity, CoreUtils.GetWorldRoot());
                    SpawnedEnemies.Add(go);
                    TimeSinceLastSpawn = 0;
                    EnemiesSpawned++;

                    if (EnemiesSpawned == EnemiesToSpawn)
                        SpawnInTownCenter = false;
                }
            }
            else if(!TownCenterCleared)
            {
                //check if it's clear
                int totalEnemies = 0;
                int deadEnemies = 0;
                foreach (var enemy in SpawnedEnemies)
                {
                    if (enemy != null)
                    {
                        var ec = enemy.GetComponent<EnemyController>();
                        if (ec != null)
                        {
                            totalEnemies++;
                            if (ec.IsDead)
                                deadEnemies++;
                        }
                    }
                }

                if(totalEnemies > 0 && deadEnemies > 0 && totalEnemies == deadEnemies)
                {
                    TownCenterCleared = true;
                    HandleTownCenterCleared();
                }

            }

        }


        public void HandleTownCenterReached()
        {
            Debug.Log("Town started!");

            //start spawning!
            SpawnInTownCenter = true;
            //TimeSinceLastSpawn = 5f;

            //move camera
            SceneController.UseCameraMovementBounds = true;
            CameraFollowScript.FollowPlayer = false;
            CameraFollowScript.MoveTo(CameraTargetTransform.position, 5f);
        }

        public void HandleTownCenterCleared()
        {
            Debug.Log("Town cleared!");

            //unlock camera, display hint
            SceneController.UseCameraMovementBounds = false;            
            CameraFollowScript.CancelMove();
            CameraFollowScript.FollowPlayer = true;
            if (TownCenterHintObject != null)
                TownCenterHintObject.SetActive(true);
        }

        
        
    }
}