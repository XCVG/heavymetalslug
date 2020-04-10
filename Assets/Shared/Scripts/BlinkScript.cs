using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slug
{

    /// <summary>
    /// Blinks an object
    /// </summary>
    public class BlinkScript : MonoBehaviour
    {
        [SerializeField]
        private GameObject Target = null;
        [SerializeField]
        private float BlinkOffTime = 1;
        [SerializeField]
        private float BlinkOnTime = 1;

        private float Elapsed = 0;

        void Update()
        {
            Elapsed += Time.deltaTime;

            if(Target.activeSelf && Elapsed >= BlinkOnTime)
            {
                Target.SetActive(false);
                Elapsed = 0;
            }
            else if(!Target.activeSelf && Elapsed >= BlinkOffTime)
            {
                Target.SetActive(true);
                Elapsed = 0;
            }
        }
    }
}