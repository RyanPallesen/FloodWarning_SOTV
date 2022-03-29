using System.Collections.Generic;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using Resources = FloodWarning.Properties.Resources;

namespace FloodWarning.Artifacts
{
    [R2APISubmoduleDependency()]

    internal class TrueDissonance
    {
        private static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();

        public static List<ArtifactDef> GetDefs()
        {
            tempDefs.Add(Plugin.GenerateArtifactDef
                (
                    "True Dissonance",
                    "Artifact of True Dissonance",
                    "All entities can spawn on any stage.",
                    Resources.TrueDissonanceSelected,
                    Resources.TrueDissonanceDeselected
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

            foreach (GameObject gameObject in UnityEngine.Resources.LoadAll<GameObject>("Prefabs/charactermasters"))
            {
                CharacterBody characterBody = gameObject.GetComponentInChildren<CharacterMaster>().bodyPrefab
                    .GetComponentInChildren<CharacterBody>();

                if (!characterBody) break;

                //Check to see if this is a survivor, as only survivors have preferred pods.
                bool hasPreferredPodPrefab = characterBody.preferredPodPrefab != null;

                float bodyCredit = 0;
                //Health Calculation
                float baseHealth = characterBody.baseMaxHealth + characterBody.baseMaxShield + characterBody.baseArmor;
                float levelHealth = characterBody.levelMaxHealth + characterBody.levelMaxShield +
                                    characterBody.levelArmor;
                float totalHealth = baseHealth + levelHealth;

                //DPS calculation
                float baseDPS = characterBody.baseDamage * characterBody.baseAttackSpeed;
                float levelDPS = characterBody.levelDamage + characterBody.levelAttackSpeed;
                float totalDPS = baseDPS + levelDPS;

                bodyCredit += totalDPS / 16f + totalHealth;

                //survivors cost 25x, due to disproportionate health and damage values.
                bodyCredit *= hasPreferredPodPrefab ? 25 : 1;

                CharacterSpawnCard characterSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
                characterSpawnCard.name = "csc_DISSONANCE_" + gameObject.name;
                characterSpawnCard.prefab = gameObject;
                characterSpawnCard.sendOverNetwork = true;
                characterSpawnCard.hullSize = 0;
                characterSpawnCard.nodeGraphType = 0;
                characterSpawnCard.requiredFlags = 0;
                characterSpawnCard.forbiddenFlags =
                    NodeFlags.NoCeiling | NodeFlags.NoCharacterSpawn | NodeFlags.TeleporterOK;
                characterSpawnCard.directorCreditCost = Mathf.FloorToInt(bodyCredit);
                characterSpawnCard.occupyPosition = false;
                characterSpawnCard.loadout = new SerializableLoadout();
                characterSpawnCard.noElites = false;
                characterSpawnCard.forbiddenAsBoss = false;

                DirectorCard directorCard = new DirectorCard
                {
                    spawnCard = characterSpawnCard,
                    selectionWeight = (int) Mathf.Sqrt(bodyCredit),
                    preventOverhead = false,
                    minimumStageCompletions = 0,
                    spawnDistance = 0,
                };

                //This is to make the value more in-line with default weights.

                bodyCredit /= 64;
                DirectorAPI.MonsterCategory monsterCategory;

                //Completely arbitrary numbers that determine what category monsters are part of.
                if (bodyCredit < 150f)
                    monsterCategory = DirectorAPI.MonsterCategory.BasicMonsters;
                else if (bodyCredit < 1000f)
                    monsterCategory = DirectorAPI.MonsterCategory.Minibosses;
                else
                    monsterCategory = DirectorAPI.MonsterCategory.Champions;

                //Survivor characters are always champions.
                if (hasPreferredPodPrefab) monsterCategory = DirectorAPI.MonsterCategory.Champions;

                //Create the 'Card'
                DirectorAPI.DirectorCardHolder monsterCard = new DirectorAPI.DirectorCardHolder
                {
                    Card = directorCard,
                    MonsterCategory = monsterCategory,
                    InteractableCategory = DirectorAPI.InteractableCategory.None
                };

                //Tell the DirectorAPI to add this card to the existing cards.
                DirectorAPI.MonsterActions +=
                    delegate(List<DirectorAPI.DirectorCardHolder> list, DirectorAPI.StageInfo stage)
                    {
                        if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]) &&
                            !list.Contains(monsterCard))
                        {
                            Debug.Log("Card added for " + monsterCard.Card.spawnCard.name);

                            list.Add(monsterCard);
                        }
                        
                    };
            }

            SpawnCard.onSpawnedServerGlobal += delegate(SpawnCard.SpawnResult cardresult)
            {
                Debug.Log("Server spawned " + cardresult.spawnRequest.spawnCard.name);

                if (cardresult.spawnRequest.spawnCard == null) return;
                if (!cardresult.spawnRequest.spawnCard.name.Contains("DISSONANCE")) return;

                cardresult.spawnedInstance.gameObject.GetComponent<CharacterMaster>().onBodyStart +=
                    delegate(CharacterBody bodyStartBody)
                    {
                        bodyStartBody.bodyFlags = 0;

                        DeathRewards deathRewards = bodyStartBody.gameObject.GetComponent<DeathRewards>();

                        if (!deathRewards)
                        {
                            deathRewards = bodyStartBody.gameObject.AddComponent<DeathRewards>();
                            deathRewards.SetFieldValue("characterBody", bodyStartBody);
                        }

                        if (deathRewards.goldReward < 1U)
                            deathRewards.goldReward = (uint) (cardresult.spawnRequest.spawnCard.directorCreditCost * 0.25f);

                        if (deathRewards.expReward < 1U)
                            deathRewards.expReward = (uint) (cardresult.spawnRequest.spawnCard.directorCreditCost * 0.25f);

                        bodyStartBody.master.money = (uint)cardresult.spawnRequest.spawnCard.directorCreditCost;
                    };
            };
        }
    }
}