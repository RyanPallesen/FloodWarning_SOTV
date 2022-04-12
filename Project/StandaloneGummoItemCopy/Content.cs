using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using On.RoR2;
using RoR2;
using RoR2.ContentManagement;
using CharacterMaster = On.RoR2.CharacterMaster;
using DirectorSpawnRequest = RoR2.DirectorSpawnRequest;
using ItemCatalog = On.RoR2.ItemCatalog;

namespace FloodWarning.StandaloneGummoItemCopy
{
    // Token: 0x02000002 RID: 2
    public class FW_StandaloneGummoItemCopy : IContentPackProvider
    {
        public ContentPack contentPack = new ContentPack();

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public string identifier => "FW_StandaloneGummoItemCopyContent";

        // Token: 0x06000002 RID: 2 RVA: 0x00002067 File Offset: 0x00000267
        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            StandaloneGummoItemCopy.Create();
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

        public static class StandaloneGummoItemCopy
        {

            public static void Create()
            {
                On.RoR2.CharacterMaster.AddDeployable += (orig, self, deployable, slot) =>
                {
                    orig(self, deployable, slot);

                    if (slot == DeployableSlot.GummyClone)
                    {
                        deployable.gameObject.GetComponent<RoR2.CharacterMaster>().inventory
                            .CopyItemsFrom(self.inventory);
                        deployable.gameObject.GetComponent<RoR2.CharacterMaster>().inventory
                            .CopyEquipmentFrom(self.inventory);
                    }

                };
            }


        }
    }
}