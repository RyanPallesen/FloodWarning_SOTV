using System.Collections.Generic;
using FloodWarning.Artifacts.Properties;
using RoR2;
using CombatDirector = On.RoR2.CombatDirector;
using SceneDirector = On.RoR2.SceneDirector;

namespace FloodWarning.Artifacts.Artifacts
{
    internal class Greed
    {
        private static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();

        public static List<ArtifactDef> GetDefs()
        {
            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Greed1",
                    "Artifact of Greed",
                    "Increase enemy and interactable spawns by 2x",
                    Resources.GreedSelected,
                    Resources.GreedDeselected
                )
            );

            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Greed2",
                    "Artifact of Greed",
                    "Increase enemy and interactable spawns by 2x",
                    Resources.GreedSelected,
                    Resources.GreedDeselected
                )
            );

            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "Greed3",
                    "Artifact of Greed",
                    "Increase enemy and interactable spawns by 2x",
                    Resources.GreedSelected,
                    Resources.GreedDeselected
                )
            );

            return tempDefs;
        }

        public static void DoConfig()
        {
        }

        public static void DoHooks()
        {
            CombatDirector.Awake += (orig, self) =>
            {
                for (int i = 0; i < tempDefs.Count; i++)
                    if (RunArtifactManager.instance.IsArtifactEnabled(tempDefs[i]))
                        self.creditMultiplier *= 2;

                orig(self);
            };

            SceneDirector.PopulateScene += (orig, self) =>
            {
                for (int i = 0; i < tempDefs.Count; i++)
                    if (RunArtifactManager.instance.IsArtifactEnabled(tempDefs[i]))
                    {
                        self.interactableCredit *= 2;
                    }

                orig(self);
            };
        }
    }
}