using System.Collections.Generic;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
using Resources = FloodWarning.Properties.Resources;

namespace FloodWarning.Artifacts
{
    internal class TrueHonor
    {
        private static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();
        public static CombatDirector.EliteTierDef[] eliteTiers;

        public static List<ArtifactDef> GetDefs()
        {
            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "True Honor",
                    "Artifact of True Honor",
                    "Enemies may now have multiple elite types.",
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
            On.RoR2.CombatDirector.Awake += delegate(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
            {
                orig(self);

                if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                    self.onSpawnedServer.AddListener(delegate(GameObject obj)
                    {
                        if (!NetworkServer.active) return;
                        DirectorCard lastAttemptedMonsterCard = self.lastAttemptedMonsterCard;
                        AddEliteTypesToBody(obj.GetComponentInChildren<CharacterMaster>().GetBody(),
                            ref self.monsterCredit, lastAttemptedMonsterCard.cost);
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

            //todo: Config this.
            float TrueHonorChance = 20;

            //todo: Config this.
            //How much the base cost should increase, in percent, for each elite modifier.
            //100 is a 100% increase, meaning 2 elites cost 200% base cost, 3 for 300%.
            float TrueHonorPerBuffMultiplier = 100f;

            float perBuffMultiplier = TrueHonorPerBuffMultiplier / 100f;

            if (Run.instance.spawnRng.nextNormalizedFloat * 100 > TrueHonorChance)
                return;

            int eliteBuffs = GetTotalEliteBuffs(characterBody);

            foreach (CombatDirector.EliteTierDef tierDef in eliteTiers)
            foreach (EliteDef eliteDef in tierDef.availableDefs)

                if (eliteDef && eliteDef.IsAvailable() && eliteDef.eliteEquipmentDef &&
                    eliteDef.eliteEquipmentDef.passiveBuffDef)
                {
                    //Treat card as x% more expensive per buff, cost as normal otherwise.
                    //Add +1 to get the cost for an additional buff.
                    float eliteBuffCost = cardCost * (eliteBuffs * perBuffMultiplier + 1) * tierDef.costMultiplier;

                    if (eliteBuffCost > monsterCredit)
                    {
                        monsterCredit -= eliteBuffCost;
                        ApplyEliteToBody(eliteDef, characterBody);
                        eliteBuffs++;
                    }
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