using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug
{

    /// <summary>
    /// Spins the helicopter blades. If I wasn't so lazy I'd just bring in GenericRotateScript
    /// </summary>
    public class HelicopterBladeScript : MonoBehaviour
    {
        [SerializeField]
        private float RotateSpeed = 180f;

        private void Update()
        {
            transform.Rotate(Vector3.up, RotateSpeed * Time.deltaTime);
        }
    }
}