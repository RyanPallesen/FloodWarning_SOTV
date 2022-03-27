using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using FloodWarning;
using FloodWarning.Artifacts;
using RoR2;
using RoR2.ContentManagement;

namespace Artifacts
{
    // Token: 0x02000002 RID: 2
    public class FW_Artifacts : IContentPackProvider
    {
        public static List<ArtifactDef> artifactDefs = new List<ArtifactDef>();

        public ContentPack contentPack = new ContentPack();

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public string identifier => "FW_ArtifactsContent";

        // Token: 0x06000002 RID: 2 RVA: 0x00002067 File Offset: 0x00000267
        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            Artifacts.Create();
            contentPack.artifactDefs.Add(artifactDefs.ToArray());
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
            public static List<ArtifactStruct> structList = new List<ArtifactStruct>();

            public static void Create()
            {
                GenerateArtifacts();
                ConfigArtifacts();
            }

            private static void GenerateArtifacts()
            {
                //Greed
                structList.Add(new ArtifactStruct
                {
                    artifactName = "Greed",
                    defs = Greed.GetDefs(),
                    hookCallbacks = new List<Action> {Greed.DoHooks}
                });

                //True Dissonance
                structList.Add(new ArtifactStruct
                {
                    artifactName = "True Dissonance",
                    defs = TrueDissonance.GetDefs(),
                    hookCallbacks = new List<Action> { TrueDissonance.DoHooks }
                });

                //Atlas
                structList.Add(new ArtifactStruct
                {
                    artifactName = "Atlas",
                    defs = Atlas.GetDefs(),
                    hookCallbacks = new List<Action> { Atlas.DoHooks }
                });
            }

            private static void ConfigArtifacts()
            {
                foreach (ArtifactStruct def in structList)
                {
                    //Create a config entry for this artifact
                    ConfigEntry<bool> entry = Plugin.mainConfigFile.Bind("Artifacts",
                        def.artifactName + "_Enabled", true,
                        "Whether or not the artifact, " + def.artifactName + ", should be enabled");

                    //If we are enabling this to be chosen.
                    if (entry.Value)
                    {
                        //add any related artifact definitions
                        foreach (ArtifactDef artifactDef in def.defs) artifactDefs.Add(artifactDef);

                        //set up all hooks
                        foreach (Action action in def.hookCallbacks) action.Invoke();
                    }
                }
            }

            public struct ArtifactStruct
            {
                public string artifactName;
                public List<ArtifactDef> defs;
                public List<Action> hookCallbacks;
            }
        }
    }
}