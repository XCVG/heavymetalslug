﻿using CommonCore;
using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.State;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Basic console commands
/// </summary>
public static class SharedConsoleCommands
{
    //***** UTILITIES 
    [Command]
    static void PrintDataPath() //TODO move elsewhere
    {
        ConsoleModule.WriteLine(CoreParams.PersistentDataPath);
    }

    [Command]
    static void PrintScenePathList()
    {
        try
        {
            var sceneNames = CoreUtils.GetSceneList();
            StringBuilder sb = new StringBuilder(sceneNames.Length * 16);
            foreach (var s in sceneNames)
            {
                sb.AppendLine(s);
            }
            ConsoleModule.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    [Command]
    static void PrintSceneList()
    {
        try
        {
            var sceneNames = CoreUtils.GetSceneList();
            StringBuilder sb = new StringBuilder(sceneNames.Length * 16);
            foreach (var s in sceneNames)
            {
                sb.AppendLine(Path.GetFileNameWithoutExtension(s));
            }
            ConsoleModule.WriteLine(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    [Command]
    static void PrintCoreParams()
    {
        Dictionary<string, object> coreParams = new Dictionary<string, object>();
        var props = typeof(CoreParams).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach (var prop in props)
            coreParams.Add(prop.Name, prop.GetValue(null));

        ConsoleModule.WriteLine(DebugUtils.JsonStringify(coreParams));
    }

    //***** LOAD/SAVE

    /// <summary>
    /// Forces a full load from file, with scene transition
    /// </summary>
    [Command]
    static void LoadClean(string name)
    {
        MetaState.Instance.TransitionType = SceneTransitionType.LoadGame;
        MetaState.Instance.LoadSave = name;
        MetaState.Instance.Intents.Clear();

        SceneManager.LoadScene("LoadingScene"); //TODO put loading scene name somewhere
        
    }

    /// <summary>
    /// Loads game state from a file, then restores it to this scene
    /// </summary>
    [Command]
    static void Load(string name)
    {
        GameState.DeserializeFromFile(CoreParams.SavePath + @"\" + name);

        Restore();
    }

    /// <summary>
    /// Restores the saved game scene state to this scene
    /// </summary>
    [Command]
    static void Restore()
    {
        MetaState.Instance.TransitionType = SceneTransitionType.LoadGame;
        BaseSceneController bsc = SharedUtils.GetSceneController();
        bsc.Restore();
    }

    /// <summary>
    /// Commits this scene's state to game state
    /// </summary>
    [Command]
    static void Commit()
    {
        BaseSceneController bsc = SharedUtils.GetSceneController();
        bsc.Commit();
    }

    /// <summary>
    /// Commits this scene's state, then saves game state to file
    /// </summary>
    [Command]
    static void Save(string name)
    {
        Commit();

        GameState.SerializeToFile(CoreParams.SavePath + @"\" + name);
    }

    //***** WARP

    /// <summary>
    /// Changes scenes
    /// </summary>
    [Command]
    static void Warp(string scene)
    {
        SharedUtils.ChangeScene(scene);
    }

    /// <summary>
    /// Reloads the current scene
    /// </summary>
    [Command]
    static void Reload()
    {
        SharedUtils.ChangeScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Ends the current game
    /// </summary>
    [Command]
    static void EndGame()
    {
        SharedUtils.EndGame();
    }

    //***** SCREENFADE

    /// <summary>
    /// Clears any screen fade
    /// </summary>
    [Command(alias = "Clear", className = "ScreenFader", useClassName = true)]
    static void ClearScreenFade()
    {
        ScreenFader.ClearFade();
    }
}
