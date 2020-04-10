﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.UI;

namespace CommonCore.Input
{

    /// <summary>
    /// Abstract base InputMapper. Functionality can be implemented by mappers with different backends
    /// </summary>
    public abstract class InputMapper
    {
        public virtual void Configure()
        {
            Modal.PushMessageModal("No additional configuration is available for this input mapper.", "Not Available", null, null);
        }

        public abstract float GetAxis(string axis);
        public abstract float GetAxisRaw(string axis);
        public abstract bool GetButton(string button);
        public abstract bool GetButtonDown(string button);
        public abstract bool GetButtonUp(string button);
    }

    /// <summary>
    /// Null InputMapper. Useless but there anyway.
    /// </summary>
    internal class NullInputMapper : InputMapper
    {
        public override float GetAxis(string axis)
        {
            return 0;
        }

        public override float GetAxisRaw(string axis)
        {
            return 0;
        }

        public override bool GetButton(string button)
        {
            return false;
        }

        public override bool GetButtonDown(string button)
        {
            return false;
        }

        public override bool GetButtonUp(string button)
        {
            return false;
        }
    }

    /// <summary>
    /// Unity InputMapper. Simple passthrough to built-in input system
    /// </summary>
    internal class UnityInputMapper : InputMapper
    {
        public override void Configure()
        {
            Modal.PushMessageModal("Please configure the controls using the Unity launcher", "Not Available", null, null);
        }

        public override float GetAxis(string axis)
        {
            return UnityEngine.Input.GetAxis(axis);
        }

        public override float GetAxisRaw(string axis)
        {
            return UnityEngine.Input.GetAxisRaw(axis);
        }

        public override bool GetButton(string button)
        {
            return UnityEngine.Input.GetButton(button);
        }

        public override bool GetButtonDown(string button)
        {
            return UnityEngine.Input.GetButtonDown(button);
        }

        public override bool GetButtonUp(string button)
        {
            return UnityEngine.Input.GetButtonUp(button);
        }
    }

}
