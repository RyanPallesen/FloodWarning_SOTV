using System;
using System.Collections.Generic;
using FloodWarning.Tweaks;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace FloodWarning
{
    //This attribute specifies that we have a dependency on R2API
    [BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(DirectorAPI))]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "RyanPallesen";
        public const string PluginName = "FW_Tweaks";
        public const string PluginVersion = "2.0.0";

        public static ConfigFile mainConfigFile;

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            mainConfigFile = new ConfigFile(Paths.ConfigPath + "\\FloodWarning\\Tweaks.cfg", true);

            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void ContentManager_collectContentPackProviders(
            ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider.Invoke(new FW_Tweaks());
        }

        public static void RegisterLanguageToken(string token, string text)
        {
            LanguageAPI.Add(token, text);
        }
    }
}