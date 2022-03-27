using System;
using System.Collections.Generic;
using Artifacts;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace FloodWarning
{
    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(R2API.R2API.PluginGUID)]

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //We will be using 2 modules from R2API: ItemAPI to add our item and LanguageAPI to add our language tokens.
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI))]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class Plugin : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName!
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "RyanPallesen";
        public const string PluginName = "FW_Artifacts";
        public const string PluginVersion = "2.0.0";

        private static List<ArtifactDef> artifactDefs;
        private static List<ArtifactDef> activeArtifacts;

        public static ConfigFile mainConfigFile;
        public static List<ConfigEntry<bool>> MainConfigEntries { get; set; }

        public static Dictionary<ArtifactDef, ConfigFile> perArtifactConfigs;
        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            ContentManager.collectContentPackProviders += new ContentManager.CollectContentPackProvidersDelegate(ContentManager_collectContentPackProviders);
            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider.Invoke(new FW_Artifacts());
        }

        public static void RegisterLanguageToken(string token, string text)
        {
            LanguageAPI.Add(token, text);
        }

        public static ArtifactDef GenerateArtifactDef(string artifactNameInternal, string artifactName, string artifactDescription, byte[] selectImage, byte[] deselectImage)
        {
            ArtifactDef artifactDef = ScriptableObject.CreateInstance<ArtifactDef>();
            artifactDef.descriptionToken = "FW_ARTIFACTS_" + artifactName.ToUpper() + "_DESCRIPTION";
            artifactDef.nameToken = "FW_ARTIFACTS_" + artifactName.ToUpper() + "_NAME";
            Plugin.RegisterLanguageToken(artifactDef.nameToken, artifactName);
            Plugin.RegisterLanguageToken(artifactDef.descriptionToken, artifactDescription);

            artifactDef.smallIconSelectedSprite =
                CreateSprite(selectImage, Color.magenta);

            artifactDef.smallIconDeselectedSprite =
                CreateSprite(deselectImage, Color.grey);

            return artifactDef;
        }

        public static Sprite CreateSprite(byte[] resourceBytes, Color fallbackColor)
        {
            Texture2D texture2D = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            try
            {
                bool isNull = resourceBytes == null;
                if (isNull)
                {
                    Color[] pixels = texture2D.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = fallbackColor;
                    }
                    texture2D.SetPixels(pixels);
                    texture2D.Apply();
                }
                else
                {
                    texture2D.LoadImage(resourceBytes, false);
                    texture2D.Apply();

                    Color[] pixels = texture2D.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        bool pixelAlphaBelowThreshold = pixels[i].a < 0.05f;
                        if (pixelAlphaBelowThreshold)
                        {
                            pixels[i] = Color.clear;
                        }
                    }
                    texture2D.SetPixels(pixels);
                    texture2D.Apply();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
                Color[] pixels = texture2D.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = fallbackColor;
                }
                texture2D.SetPixels(pixels);
                texture2D.Apply();
            }
            return Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(31f, 31f));
        }
    }

}
