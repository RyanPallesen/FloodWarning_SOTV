using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2.ContentManagement;

namespace FloodWarning.Tweaks
{
    // Token: 0x02000002 RID: 2
    public class FW_Tweaks : IContentPackProvider
    {
        public ContentPack contentPack = new ContentPack();

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public string identifier => "FW_TweaksContent";

        // Token: 0x06000002 RID: 2 RVA: 0x00002067 File Offset: 0x00000267
        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            Artifacts.Create();
            args.ReportProgress(1f);
            yield break;
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000207D File Offset: 0x0000027D
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002093 File Offset: 0x00000293
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public static class Artifacts
        {
            public static List<TweakStruct> structList = new List<TweakStruct>();

            public static void Create()
            {
                GenerateTweaks();
                ConfigTweaks();
            }

            private static void GenerateTweaks()
            {
            }

            private static void ConfigTweaks()
            {
                foreach (TweakStruct def in structList)
                {
                    //Create a config entry for this artifact
                    ConfigEntry<bool> entry = Plugin.mainConfigFile.Bind("Tweaks",
                        def.tweakName + "_Enabled", true,
                        "Whether or not the artifact, " + def.tweakName + ", should be enabled");

                    //If we are enabling this to be chosen.
                    if (entry.Value)
                        //set up all hooks
                        foreach (Action action in def.hookCallbacks)
                            action.Invoke();
                }
            }

            public struct TweakStruct
            {
                public string tweakName;
                public List<Action> hookCallbacks;
            }
        }
    }
}