﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// CommonCore Parameters- core config, versioning, paths, etc
    /// </summary>
    public static class CoreParams
    {

        //*****system version info
        public static Version VersionCode { get; private set; } = new Version(2, 0, 0); //2.0.0
        public static string VersionName { get; private set; } = "Balmora"; //start with A, locations from RPGs
        public static Version UnityVersion { get; private set; } //auto-set
        public static string UnityVersionName { get; private set; } //auto-set
        public static RuntimePlatform Platform { get; private set; } //auto-set
        public static ScriptingImplementation ScriptingBackend { get; private set; } //auto-set

        //*****game version info
        public static string GameName { get; private set; } //auto-set from Unity settings
        public static Version GameVersion { get; private set; } //auto-set from Unity settings
        public static string GameVersionName { get; private set; } = "";

        //*****basic config settings
        public static bool AutoInit { get; private set; } = true;
        public static ImmutableArray<string> ExplicitModules { get; private set; } = new string[] { "DebugModule", "QdmsMessageBus", "ConfigModule", "AsyncModule", "ScriptingModule", "ConsoleModule" }.ToImmutableArray();
        private static DataLoadPolicy LoadData = DataLoadPolicy.OnStart;
        public static string PreferredCommandConsole { get; private set; } = "SickDevConsoleImplementation";
        private static WindowsPersistentDataPath PersistentDataPathWindows = WindowsPersistentDataPath.Roaming;
        private static bool CorrectWindowsLocalDataPath = false; //if set, use AppData/Local/* instead of AppData/LocalLow/* for LocalDataPath
        private static bool UseGlobalScreenshotFolder = true; //ignored on UWP and probably other platforms
        public static bool SetSafeResolutionOnExit = true;
        public static Vector2Int SafeResolution = new Vector2Int(1280, 720);

        //*****additional config settings
        public static float DelayedEventPollInterval { get; private set; } = 1.0f;
        //public static bool UseAggressiveLookups { get; private set; } = true; //may bring this back someday if performance is an issue

        //*****game config settings
        public static string InitialScene { get; private set; } = "AttractScene";
        public static bool UseCampaignIdentifier { get; private set; } = true;
        public static bool AllowSaveLoad { get; private set; } = false;
        public static bool AllowManualSave { get; private set; } = false;
        public static IReadOnlyList<string> AdditionalAxes { get; private set; } = ImmutableArray.Create<string>(); //specify additional axes your game will use; it's up to individual input mappers to handle these
        public static IReadOnlyList<string> AdditionalButtons { get; private set; } = ImmutableArray.Create<string>("InsertCoin", "Start", "InsertCoinP2", "StartP2"); //same, but for buttons
        public static IReadOnlyList<string> HideControlMappings { get; private set; } = ImmutableArray.Create<string>("OpenMenu", "OpenFastMenu", "ChangeView", "Fire3", "Reload", "Use", "Use2", "Offhand1", "Offhand2", "LookX", "LookY", "Run", "Crouch"); //add things to this to hide DefaultControls you're not using, note that it's not guaranteed to stop the control from responding to input

        //*****path variables (some hackery to provide thread-safeish versions)
        public static string DataPath { get; private set; }
        public static string GameFolderPath { get; private set; }
        public static string PersistentDataPath { get; private set; }
        public static string LocalDataPath { get; private set; }
        public static string StreamingAssetsPath { get; private set; }
        public static string ScreenshotsPath { get; private set; }

        //*****automatic environment params
        public static bool IsDebug
        {
            get
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static string SavePath
        {
            get
            {
                return PersistentDataPath + "/saves";
            }
        }

        public static DataLoadPolicy LoadPolicy
        {
            get
            {
                if (LoadData == DataLoadPolicy.Auto)
                {
#if UNITY_EDITOR
                    return DataLoadPolicy.OnDemand;
#else
                    return DataLoadPolicy.OnStart;
#endif
                }
                else
                    return LoadData;
            }
        }

        /// <summary>
        /// A hack necessary to preset variables so they can be safely accessed across threads
        /// </summary>
        internal static void SetInitial()
        {
            //VERSION/NAME HANDLING
            UnityVersion = TypeUtils.ParseVersion(Application.unityVersion);
            UnityVersionName = Application.unityVersion;
            GameName = Application.productName;

            try
            {
                GameVersion = TypeUtils.ParseVersion(Application.version);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to decode version string \"{Application.version}\" (please use something resembling semantic versioning)");
                Debug.LogException(e);
            }

            //PLATFORM HANDLING
            Platform = Application.platform;

            //afaict no way to check these at runtime
#if !UNITY_EDITOR && UNITY_WSA && !ENABLE_IL2CPP
            ScriptingBackend = ScriptingImplementation.WinRTDotNET;
#elif ENABLE_IL2CPP
            ScriptingBackend = ScriptingImplementation.IL2CPP;
#else
            ScriptingBackend = ScriptingImplementation.Mono2x; //default
#endif

            //PATH HANDLING

            //normal handling for DataPath and StreamingAssetsPath
            DataPath = Application.dataPath;
            StreamingAssetsPath = Application.streamingAssetsPath;

            //GameFolderPath (ported from Sandstorm)
            GameFolderPath = Directory.GetParent(Application.dataPath).ToString();

            //special handling for PersistentDataPath and LocalDataPath
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            switch (PersistentDataPathWindows)
            {
                case WindowsPersistentDataPath.Corrected:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.Roaming:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.Documents:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.MyGames:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", Application.companyName, Application.productName);
                    break;
                default:
                    PersistentDataPath = Application.persistentDataPath;
                    break;
            }
            if (CorrectWindowsLocalDataPath)
                LocalDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName, Application.productName, "local");
            else
                LocalDataPath = Path.Combine(Application.persistentDataPath, "local");
#else
            PersistentDataPath = Application.persistentDataPath;
            LocalDataPath = Path.Combine(Application.persistentDataPath, "local");
#endif

            //create data folder if it doesn't exist
            if (!Directory.Exists(PersistentDataPath))
                Directory.CreateDirectory(PersistentDataPath); //failing this is considered fatal

            //special handling for ScreenshotPath
#if UNITY_WSA
            ScreenshotsPath = Path.Combine(PersistentDataPath, "screenshot");
#else
            if (UseGlobalScreenshotFolder)
            {
                ScreenshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
            }
            else
            {
                ScreenshotsPath = Path.Combine(PersistentDataPath, "screenshot");
            }
#endif

            //create screenshot folder if it doesn't exist (this is a survivable error)
            try
            {
                if (!Directory.Exists(ScreenshotsPath))
                    Directory.CreateDirectory(ScreenshotsPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create screenshots directory ({ScreenshotsPath})");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Returns a "short" description of the application name, version, Unity environment (shown on main menu)
        /// </summary>
        public static string GetShortSystemText()
        {
            return string.Format("{0}\n{1} {2}\nCommonCore {3} {4}\nUnity {5}",
                GameName,
                GameVersion, GameVersionName,
                VersionCode.ToString(), VersionName,
                UnityVersionName);
        }
    }


}