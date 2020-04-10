﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace CommonCore.Input.EventSystems
{

    /// <summary>
    /// Input Module for event systems that uses MappedInput
    /// </summary>
    public class MappedInputModule : StandaloneInputModule
    {
        //basically StandaloneInputModule that auto-creates a different input component

        [SerializeField, Header("Mapped Input Options")]
        private bool UseMappedInput = true;
        [SerializeField, Tooltip("If set, warns if the axes or buttons are not set correctly")]
        private bool WarnAboutBindings = true;
        [SerializeField, Tooltip("If set, will automatically set bindings to")]
        private bool AutosetBindings = true;
        [SerializeField, Tooltip("If set, will use both mapped and builtin input")]
        private bool UseDualInput = true;

        protected override void Awake()
        {
            if(WarnAboutBindings)
            {
                if(!horizontalAxis.Equals("NavigateX", StringComparison.Ordinal) || !verticalAxis.Equals("NavigateY", StringComparison.Ordinal))
                {
                    Debug.LogWarning($"{nameof(MappedInputModule)} on {name} does not have axes set correctly (should be \"NavigateX\" and \"NavigateY\"");                    
                }

                if(!submitButton.Equals("Submit", StringComparison.Ordinal) || !cancelButton.Equals("Cancel", StringComparison.Ordinal))
                {
                    Debug.LogWarning($"{nameof(MappedInputModule)} on {name} does not have buttons set correctly (should be \"Submit\" and \"Cancel\"");
                }
            }
            if (AutosetBindings)
            {
                horizontalAxis = "NavigateX";
                verticalAxis = "NavigateY";
                submitButton = "Submit";
                cancelButton = "Cancel";
            }

            //inject the MappedInputComponent on start
            if (UseMappedInput)
            {
                var mic = gameObject.AddComponent<MappedInputComponent>();
                inputOverride = mic;
                mic.UseDualInput = UseDualInput;
            }
        }
    }
}