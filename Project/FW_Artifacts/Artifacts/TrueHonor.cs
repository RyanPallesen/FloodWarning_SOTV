using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
using Resources = FloodWarning.Artifacts.Properties.Resources;

namespace FloodWarning.Artifacts.Artifacts
{
    internal class TrueHonor
    {
        public static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();
        public static CombatDirector.EliteTierDef[] eliteTiers;

        private static readonly ConfigFile configFile =
            new ConfigFile(Paths.ConfigPath + "\\FloodWarning\\Artifacts\\TrueHonor.cfg", true);

        public static List<ArtifactDef> GetDefs()
        {
            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "True Honor",
                    "Artifact of True Honor",
                    "Enemies may now have multiple elite types.",
                    Resources.TrueHonorSelected,
                    Resources.TrueHonorDeselected
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

            On.RoR2.CombatDirector.Awake += delegate(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
            {
                orig(self);

                if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                    self.onSpawnedServer.AddListener(delegate(GameObject obj)
                    {
                        if (!NetworkServer.active) return;

                        CharacterSpawnCard lastAttemptedMonsterCard =
                            self.lastAttemptedMonsterCard.spawnCard as CharacterSpawnCard;

                        if (lastAttemptedMonsterCard == null || lastAttemptedMonsterCard.noElites) return;

                        AddEliteTypesToBody(obj.GetComponentInChildren<CharacterMaster>().GetBody(),
                            ref self.monsterCredit, lastAttemptedMonsterCard.directorCreditCost);
                    });
            };

            On.RoR2.CombatDirector.Init += delegate(On.RoR2.CombatDirector.orig_Init orig)
            {
                orig();
                eliteTiers = typeof(CombatDirector).GetFieldValue<CombatDirector.EliteTierDef[]>("eliteTiers");
            };
        }

        public static void AddEliteTypesToBody(CharacterBody characterBody, ref float monsterCredit, float cardCost)
        {
            if (characterBody == null) return;

            float TrueHonorChance = configFile.Bind("", "TrueHonorChance", 20,
                "The chance to attempt TrueHonor an any given spawn").Value;

            float TrueHonorLevelChance = configFile.Bind("", "TrueHonorLevelChance", 25,
                "The chance, for each elite buff we can afford, to apply it. I.E. 25% Chance for Blazing").Value;


            float TrueHonorLevelCumulative = configFile.Bind("", "TrueHonorLevelCumulative", 5f,
                "The amount TrueHonorLevelChance should change for each elite buff applied.").Value;


            float TrueHonorPerBuffMultiplier = configFile.Bind("", "TrueHonorPerBuffMultiplier", 100,
                "How much the base cost of an entity should be increased, in percent, for each elite buff").Value;

            //Turning this into 0-1f.
            float perBuffCostMultiplier = TrueHonorPerBuffMultiplier / 100f;

            //So that we don't spend the entire spawning budget, the elites can only cost this percent of the budget.
            float percentCostOfCredit = configFile.Bind("", "TrueHonorPerBuffMultiplier", 80,
                "The percentage of the total spawning budget we can spend on a single enemy with TrueHonor").Value;

            if (Run.instance.spawnRng.nextNormalizedFloat * 100 > TrueHonorChance)
                return;

            int eliteBuffs = GetTotalEliteBuffs(characterBody);

            foreach (CombatDirector.EliteTierDef tierDef in eliteTiers)
            foreach (EliteDef eliteDef in tierDef.eliteTypes)
            {
                if (!eliteDef)
                    continue;
                if (!eliteDef.IsAvailable())
                    continue;
                if (!eliteDef.eliteEquipmentDef)
                    continue;
                if (!eliteDef.eliteEquipmentDef.passiveBuffDef)
                    continue;

                //Treat card as x% more expensive per buff, cost as normal otherwise.
                //Add +1 to get the cost for the additional buff.
                float eliteBuffCost = cardCost * (eliteBuffs * perBuffCostMultiplier + 1) * tierDef.costMultiplier;

                //If we can afford this elite buff
                if (eliteBuffCost > monsterCredit * (100 / percentCostOfCredit))
                    continue;

                //Test if we are willing to give it this buff via randomness
                if (Run.instance.spawnRng.nextNormalizedFloat * 100 >
                    TrueHonorLevelChance + TrueHonorLevelCumulative * eliteBuffs)
                    continue;

                monsterCredit -= eliteBuffCost;
                ApplyEliteToBody(eliteDef, characterBody);
                eliteBuffs++;
            }
        }

        private static void ApplyEliteToBody(EliteDef eliteDef, CharacterBody characterBody)
        {
            characterBody.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
            float healthBoostCoefficient = eliteDef.healthBoostCoefficient;
            float damageBoostCoefficient = eliteDef.damageBoostCoefficient;
            int livingPlayerCount = Run.instance.livingPlayerCount;

            //Duplicate logic from RoR2 code.
            healthBoostCoefficient *= Mathf.Pow(livingPlayerCount, 2f);
            characterBody.inventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt(healthBoostCoefficient * 10f));
            characterBody.inventory.GiveItem(RoR2Content.Items.BoostDamage,
                Mathf.RoundToInt(damageBoostCoefficient * 10f));
        }

        public static int GetTotalEliteBuffs(CharacterBody characterBody)
        {
            int buffCount = 0;

            foreach (EliteDef eliteDef in ContentManager.eliteDefs)
                if (characterBody.HasBuff(eliteDef.eliteEquipmentDef.passiveBuffDef))
                    buffCount++;

            return buffCount;
        }
    }
}