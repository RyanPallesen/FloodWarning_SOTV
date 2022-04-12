using System.Collections.Generic;
using RoR2;
using UnityEngine;
using Resources = FloodWarning.Artifacts.Properties.Resources;
using Stage = On.RoR2.Stage;

namespace FloodWarning.Artifacts.Artifacts
{
    internal class Dissidence
    {
        private static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();
        public static CombatDirector.EliteTierDef[] eliteTiers;

        public static List<SpawnCardCachedValues> allSpawnCards = new List<SpawnCardCachedValues>();

        public static List<ArtifactDef> GetDefs()
        {
            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Dissidence",
                    "Artifact of Dissidence",
                    "All enemies may be boss enemies and elite enemies.",
                    Resources.DissidenceSelected,
                    Resources.DissidenceDeselected
                )
            );

            return tempDefs;
        }

        public static void DoConfig()
        {
        }

        public static void DoHooks()
        {
            On.RoR2.CharacterSpawnCard.Awake +=
                delegate(On.RoR2.CharacterSpawnCard.orig_Awake orig, CharacterSpawnCard self)
                {
                    orig.Invoke(self);

                    allSpawnCards.Add(new SpawnCardCachedValues
                    {
                        spawnCard = self,
                        noElites = self.noElites,
                        forbiddenAsBoss = self.forbiddenAsBoss
                    });
                };

            Stage.Start += delegate(Stage.orig_Start orig, RoR2.Stage self)
            {
                foreach (SpawnCardCachedValues spawnCardCachedValues in allSpawnCards)
                    if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                    {
                        spawnCardCachedValues.spawnCard.forbiddenAsBoss = false;
                        spawnCardCachedValues.spawnCard.noElites = false;
                    }
                    else
                    {
                        spawnCardCachedValues.spawnCard.forbiddenAsBoss = spawnCardCachedValues.forbiddenAsBoss;
                        spawnCardCachedValues.spawnCard.noElites = spawnCardCachedValues.noElites;
                    }

                orig.Invoke(self);
            };
        }

        public struct SpawnCardCachedValues
        {
            // Token: 0x04000026 RID: 38
            public CharacterSpawnCard spawnCard;

            // Token: 0x04000027 RID: 39
            public bool noElites;

            // Token: 0x04000028 RID: 40
            public bool forbiddenAsBoss;
        }
    }
}