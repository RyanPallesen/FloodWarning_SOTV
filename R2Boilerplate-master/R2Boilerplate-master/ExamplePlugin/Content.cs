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
            private static readonly List<ArtifactDef> tempDefs = null;

            public static void Create()
            {
                GenerateArtifacts();
                ConfigArtifacts();
            }

            private static void GenerateArtifacts()
            {
                tempDefs.AddRange(Greed.GetDefs());
            }

            private static void ConfigArtifacts()
            {
                foreach (ArtifactDef def in tempDefs)
                {
                    ConfigEntry<bool> entry = Plugin.mainConfigFile.Bind("Artifacts",
                        Language.GetString(def.nameToken) + "_Enabled", true,
                        "Whether or not the artifact, " + Language.GetString(def.nameToken) + ", should be enabled");

                    if (entry.Value) artifactDefs.Add(def);
                }
            }
        }
    }
}