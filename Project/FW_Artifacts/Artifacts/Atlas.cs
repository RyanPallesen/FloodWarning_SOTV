using System.Collections.Generic;
using R2API.Utils;
using RoR2;
using UnityEngine;
using Resources = FloodWarning.Artifacts.Properties.Resources;

namespace FloodWarning.Artifacts.Artifacts
{
    [R2APISubmoduleDependency]
    internal class Atlas
    {
        private static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();

        public static List<ArtifactDef> GetDefs()
        {
            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Atlas",
                    "Artifact of Atlas",
                    "All shrines activate an additional mountain shrine",
                    Resources.AtlasSelected,
                    Resources.AtlasDeselected
                )
            );


            return tempDefs;
        }

        public static void DoConfig()
        {
        }

        public static void DoHooks()
        {
            DoConfig();

            On.RoR2.GlobalEventManager.OnInteractionBegin += GlobalEventManager_OnInteractionBegin;
        }

        private static void GlobalEventManager_OnInteractionBegin(
            On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor,
            IInteractable interactable, GameObject interactableObject)
        {
            orig.Invoke(self, interactor, interactable, interactableObject);

            if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]) &&
                interactableObject && interactableObject.GetComponent<PurchaseInteraction>() &&
                interactableObject.GetComponent<PurchaseInteraction>().isShrine &&
                TeleporterInteraction.instance) TeleporterInteraction.instance.AddShrineStack();
        }
    }
}