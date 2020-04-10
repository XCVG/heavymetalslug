using CommonCore.LockPause;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Controller for a player follow camera
    /// </summary>
    public class CameraFollowScript : MonoBehaviour
    {
        private const float TargetDistanceThreshold = 0.5f;

        /// <summary>
        /// If set, will follow the player(s)
        /// </summary>
        public bool FollowPlayer = true;

        public float MaxYDist = 0;
        public float YOffset = 1;

        public List<Transform> PlayerTransforms;

        [SerializeField]
        private Camera AttachedCamera;

        private float MoveSpeed;
        private Vector3? MoveTarget;

        private void Start()
        {
            if (AttachedCamera == null)
                AttachedCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (LockPauseModule.IsPaused())
                return;

            if (FollowPlayer)
            {

                //multiple player handling for hmslug
                Vector2 centroid;
                if (PlayerTransforms.Count == 1)
                {
                    centroid = PlayerTransforms[0].position;
                }
                else
                {
                    centroid = Vector2.zero;
                    int validPlayers = 0;
                    foreach (var t in PlayerTransforms)
                    {
                        if (t == null) //skip invalid transforms
                            continue;
                        centroid += (Vector2)t.position;
                        validPlayers++;
                    }

                    centroid /= validPlayers;
                }

                //max y dist handling
                float yPos = transform.position.y - YOffset;
                float yPosToCentroid = centroid.y - yPos;
                float distYPosToCentroid = Mathf.Abs(yPosToCentroid);
                if (distYPosToCentroid > MaxYDist)
                {
                    yPos += Mathf.Sign(yPosToCentroid) * (distYPosToCentroid - MaxYDist);
                }

                transform.position = new Vector3(centroid.x, yPos + YOffset, transform.position.z);
            }
            else if(MoveTarget.HasValue)
            {
                //move to target
                Vector3 vecToTarget = MoveTarget.Value - transform.position;
                float distToTarget = vecToTarget.magnitude;
                if(distToTarget > TargetDistanceThreshold)
                {
                    //move!
                    float moveDistance = Mathf.Min(distToTarget, MoveSpeed * Time.fixedDeltaTime);
                    transform.Translate(moveDistance * vecToTarget.normalized);
                }
            }
        }

        public Rect GetCameraBounds()
        {
            float distZ = Mathf.Abs(transform.position.z); //we always use z=0 as our plane

            Vector3 bottomLeft = AttachedCamera.ScreenToWorldPoint(new Vector3(0, 0, distZ));
            Vector3 topRight = AttachedCamera.ScreenToWorldPoint(new Vector3(AttachedCamera.pixelWidth, AttachedCamera.pixelHeight, distZ));

            float minX = bottomLeft.x;
            float maxX = topRight.x;
            float width = Mathf.Abs(maxX - minX);

            float minY = bottomLeft.y;
            float maxY = topRight.y;
            float height = Mathf.Abs(maxY - minY);

            //Debug.Log($"Camera bounds: minX={minX:F2} maxX={maxX:F2} width={width:F2}");

            return new Rect(minX, minY, width, height);
        }

        public void MoveTo(Vector3 target, float speed)
        {
            MoveSpeed = speed;
            MoveTarget = target;
        }

        public void CancelMove()
        {
            MoveSpeed = 0;
            MoveTarget = null;
        }

    }
}