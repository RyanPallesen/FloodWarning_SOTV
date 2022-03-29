using System;
using System.Collections.Generic;
using System.Text;
using RoR2;

namespace FloodWarning.Artifacts
{
    class Greed
    {
        private static List<ArtifactDef> tempDefs = new List<ArtifactDef>();

        public static List<ArtifactDef> GetDefs()
        {

            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Greed1",
                    "Artifact of Greed",
                    "Increase enemy and interactable spawns by 2x",
                    Properties.Resources.GreedSelected,
                    Properties.Resources.GreedDeselected
                )
            );

            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Greed2",
                    "Artifact of Greed",
                    "Increase enemy and interactable spawns by 2x",
                    Properties.Resources.GreedSelected,
                    Properties.Resources.GreedDeselected
                )
            );

            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Greed3",
                    "Artifact of Greed",
                    "Increase enemy and interactable spawns by 2x",
                    Properties.Resources.GreedSelected,
                    Properties.Resources.GreedDeselected
                )
            );

            return tempDefs;
        }

        public static void DoConfig()
        {

        }

        public static void DoHooks()
        {
            On.RoR2.CombatDirector.Awake += ((orig, self) =>
            {
                for (int i = 0; i < tempDefs.Count; i++)
                {
                    if (RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                    {
                        self.creditMultiplier *= 2;
                    }
                }

                orig(self);
            });

            On.RoR2.SceneDirector.PopulateScene += ((orig, self) =>
            {
                for (int i = 0; i < tempDefs.Count; i++)
                {
                    if (RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                    {
                        self.monsterCredit *= 2;
                        self.interactableCredit *= 2;
                    }
                }

                orig(self);
            });
        }

    }
}
