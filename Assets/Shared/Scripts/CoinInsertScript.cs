using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using CommonCore.State;
using CommonCore.Input;
using CommonCore.Messaging;
using CommonCore.Audio;

namespace Slug
{
    /// <summary>
    /// Handles coin insertions
    /// </summary>
    /// <remarks>Actually one of my favourite pieces of code here</remarks>
    public class CoinInsertScript : MonoBehaviour
    {
        private static CoinInsertScript Instance;


        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnGameStart)]
        private static void InjectCoinScript()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Can't inject {nameof(CoinInsertScript)} because one already exists!");
                return;
            }

            var go = new GameObject(nameof(CoinInsertScript));
            var cis = go.AddComponent<CoinInsertScript>();
            Instance = cis;
            Debug.Log($"Injected {nameof(CoinInsertScript)}!");
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnGameEnd)]
        private static void RemoveCoinScript()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Debug.Log($"Destroyed {nameof(CoinInsertScript)}!");
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            //listen for coin insertion
            if(MappedInput.GetButtonDown("InsertCoin"))
            {
                MetaState.Instance.Player1Credits += 1;
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("CreditAdded"));
                AudioPlayer.Instance.PlayUISound("coin_insert");
            }

            if (MappedInput.GetButtonDown("InsertCoinP2"))
            {
                MetaState.Instance.Player2Credits += 1;
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("CreditAdded"));
                AudioPlayer.Instance.PlayUISound("coin_insert");
            }


        }
    }
}