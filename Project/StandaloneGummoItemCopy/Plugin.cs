using System;
using System.Collections.Generic;
using FloodWarning.StandaloneGummoItemCopy;
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
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "RyanPallesen";
        public const string PluginName = "FW_StandaloneGummoItemCopy";
        public const string PluginVersion = "1.0.0";


        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void ContentManager_collectContentPackProviders(
            ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider.Invoke(new StandaloneGummoItemCopy.FW_StandaloneGummoItemCopy());
        }
    }
}