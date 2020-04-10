using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;
using CommonCore;
using CommonCore.Config;

namespace Slug
{

    /// <summary>
    /// Skips the main menu
    /// </summary>
    public static class MainMenuSkipScript
    {
        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.AfterMainMenuCreate)]
        public static void SkipMainMenu()
        {
            if (!ConfigState.Instance.HasCustomFlag("ShowMainMenu"))
                SharedUtils.StartGame();
        }
        
    }
}