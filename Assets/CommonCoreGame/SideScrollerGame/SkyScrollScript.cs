using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.SideScrollerGame
{

    /// <summary>
    /// Sky follower/fake parallax scrolling script
    /// </summary>
    public class SkyScrollScript : MonoBehaviour
    {
        [SerializeField]
        private Transform CameraTransform = null;
        [SerializeField]
        private float YShiftFactor = 0.1f;
        [SerializeField]
        private float XShiftFactor = 0.01f;

        private Vector3 OriginalPosition;

        private void Start()
        {
            //this is the only component with proper CommonCore-style fallback/safety
            if (CameraTransform == null)
                CameraTransform = Camera.main.Ref()?.transform;

            if(CameraTransform == null)
            {
                Debug.LogError($"{nameof(SkyScrollScript)} on {name} can't find a camera to track!");
                enabled = false;
            }

            OriginalPosition = transform.position;
        }

        private void LateUpdate()
        {
            //transform.position = new Vector3(CameraTransform.position.x, CameraTransform.position.y, transform.position.z);

            //I'm pretty sure this makes no physical sense but it doesn't explode and that's good enough for now
            transform.position = new Vector3(CameraTransform.position.x - OriginalPosition.x * XShiftFactor, CameraTransform.position.y - OriginalPosition.y * YShiftFactor, transform.position.z);
        }
    }
}