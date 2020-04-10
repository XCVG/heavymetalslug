using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug
{

    /// <summary>
    /// Spins a sprite or whatever. Yes, I really have two scripts that do almost the same thing.
    /// </summary>
    public class SpriteRotateScript : MonoBehaviour
    {
        [SerializeField]
        private float RotateRate = 180f;

        private void Update()
        {
            transform.Rotate(Vector3.forward, RotateRate * Time.deltaTime);
        }
    }
}