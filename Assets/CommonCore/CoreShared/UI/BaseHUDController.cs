﻿using CommonCore.Messaging;
using CommonCore.StringSub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Basic HUD controller, providing message box and subtitle support
    /// </summary>
    public class BaseHUDController : MonoBehaviour
    {
        public static BaseHUDController Current { get; protected set; }
        
        public Text MessageText;
        public ScrollRect MessageScrollRect;

        public Text SubtitleText;
        private float SubtitleTimer;
        private int SubtitlePriority = int.MinValue;

        protected QdmsMessageInterface MessageInterface;


        private void Awake()
        {
            MessageInterface = new QdmsMessageInterface();
            Current = this;
        }

        protected virtual void Start()
        {
            MessageText.text = string.Empty;
            UpdateSubtitles();
        }

        protected virtual void Update()
        {
            while (MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }

            UpdateSubtitles();
        }

        /// <summary>
        /// Handles a received message
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <returns>If the message was handled</returns>
        protected virtual bool HandleMessage(QdmsMessage message)
        {
            if (message is SubtitleMessage)
            {
                SubtitleMessage subMessage = (SubtitleMessage)message;
                if (subMessage.Priority >= SubtitlePriority)
                {
                    SubtitlePriority = subMessage.Priority;
                    SubtitleTimer = subMessage.HoldTime;
                    SubtitleText.text = subMessage.UseSubstitution ? Sub.Macro(subMessage.Contents) : subMessage.Contents;
                }
                return true;
            }

            return false;
        }

        private void UpdateSubtitles()
        {

            if (SubtitleTimer <= 0)
            {
                SubtitleText.text = string.Empty;
                SubtitlePriority = int.MinValue;
            }
            else
            {
                SubtitleTimer -= Time.deltaTime;
            }
        }

        protected void AppendHudMessage(string newMessage)
        {
            MessageText.text = MessageText.text + "\n" + newMessage;
            Canvas.ForceUpdateCanvases();
            MessageScrollRect.verticalNormalizedPosition = 0;
        }
    }
}