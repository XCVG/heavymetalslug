using CommonCore;
using CommonCore.SideScrollerGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug.Mission1
{

    public class BeachController : MonoBehaviour
    {
        [SerializeField, Header("Beach")]
        private Transform[] BeachSpawnPoints = null;
        [SerializeField]
        private float BeachSpawnRate = 5f;
        [SerializeField]
        private GameObject BeachSpawnPrefab = null;

        [SerializeField, Header("Gate")]
        private Transform[] GateSpawnPoints = null;
        [SerializeField]
        private float GateSpawnRate = 5f;
        [SerializeField]
        private GameObject GateSpawnPrefab = null;
        [SerializeField]
        private GameObject[] GateBeginActivateObjects = null; //activate when gate battle starts
        [SerializeField]
        private GameObject[] GateEndActivateObjects = null; //activate when gate battle ends
        [SerializeField]
        private GameObject[] GateEndDeactivateObjects = null; //deactivate when gate battle ends
        [SerializeField]
        private GameObject[] GateEndThrowObjects = null;
        [SerializeField]
        private float GateThrowForce = 100f;

        private bool SpawnOnBeach = false;
        private bool SpawnNearGate = false;
        private float TimeSinceLastSpawn = 0;
        private List<GameObject> SpawnedEnemies = new List<GameObject>();
        

        private void Update()
        {
            if(SpawnOnBeach)
            {
                TimeSinceLastSpawn += Time.deltaTime;

                if(TimeSinceLastSpawn >= BeachSpawnRate)
                {
                    var spawnPoint = BeachSpawnPoints[UnityEngine.Random.Range(0, BeachSpawnPoints.Length)];
                    var go = Instantiate(BeachSpawnPrefab, spawnPoint.position, Quaternion.identity, CoreUtils.GetWorldRoot());
                    SpawnedEnemies.Add(go);
                    TimeSinceLastSpawn = 0;
                }
            }
            else if (SpawnNearGate)
            {
                TimeSinceLastSpawn += Time.deltaTime;

                if (TimeSinceLastSpawn >= GateSpawnRate)
                {
                    var spawnPoint = GateSpawnPoints[UnityEngine.Random.Range(0, GateSpawnPoints.Length)];
                    var go = Instantiate(GateSpawnPrefab, spawnPoint.position, Quaternion.identity, CoreUtils.GetWorldRoot());
                    SpawnedEnemies.Add(go);
                    TimeSinceLastSpawn = 0;
                }
            }
        }

        public void HandleBeachStartReached()
        {
            SpawnOnBeach = true;

            //initial spawn
            foreach(var spawnPoint in BeachSpawnPoints)
            {
                var go = Instantiate(BeachSpawnPrefab, spawnPoint.position, Quaternion.identity, CoreUtils.GetWorldRoot());
                SpawnedEnemies.Add(go);
            }
        }

        public void HandleBeachEndReached()
        {
            SpawnOnBeach = false;
            SpawnNearGate = true;

            if(GateBeginActivateObjects != null && GateBeginActivateObjects.Length > 0)
            {
                foreach (var obj in GateBeginActivateObjects)
                    obj.SetActive(true);
            }

            //kill all beach enemies
            foreach(var enemy in SpawnedEnemies)
            {
                if(enemy != null)
                {
                    var ec = enemy.GetComponent<EnemyController>();
                    if(ec != null && !ec.IsDead)
                    {
                        ec.GrantScore = 0;
                        //ec.Kill();
                        ec.gameObject.SetActive(false);
                    }
                }
            }

            //TODO move/lock camera
        }

        public void HandleGateBattleDone()
        {
            //will be triggered by ammo explosion

            SpawnOnBeach = false;
            SpawnNearGate = false;

            //destroy the gate by selectively deactivating, activating, and throwing
            if (GateEndActivateObjects != null && GateEndActivateObjects.Length > 0)
            {
                foreach (var obj in GateEndActivateObjects)
                    obj.SetActive(true);
            }
            if (GateEndDeactivateObjects != null && GateEndDeactivateObjects.Length > 0)
            {
                foreach (var obj in GateEndDeactivateObjects)
                    obj.SetActive(false);
            }
            if (GateEndThrowObjects != null && GateEndThrowObjects.Length > 0)
            {
                foreach(var obj in GateEndThrowObjects)
                {
                    //throw objects
                    var rb = obj.GetComponent<Rigidbody>();                    
                    if(rb != null)
                    {
                        var collider = obj.GetComponent<Collider>();
                        collider.enabled = true;
                        rb.isKinematic = false;
                        rb.AddForce(Vector3.up * GateThrowForce, ForceMode.Impulse);
                        rb.AddTorque(new Vector3(UnityEngine.Random.Range(1, GateThrowForce), UnityEngine.Random.Range(1, GateThrowForce), UnityEngine.Random.Range(1, GateThrowForce)) * 0.5f, ForceMode.Impulse);
                    }

                }

                StartCoroutine(CoCleanupGateObjects());
            }

            //TODO unlock camera

            
        }

        private IEnumerator CoCleanupGateObjects()
        {
            //wait and cleanup thrown objects
            yield return new WaitForSeconds(10f);

            if (GateEndThrowObjects != null && GateEndThrowObjects.Length > 0)
            {
                foreach (var obj in GateEndThrowObjects)
                {
                    if (obj != null)
                        Destroy(obj); //okay because it doesn't actually affect the iterator
                }
            }
        }

        

    }
}