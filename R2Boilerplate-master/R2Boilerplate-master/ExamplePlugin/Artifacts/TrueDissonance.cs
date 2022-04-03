using System;
using System.Collections.Generic;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Console = System.Console;
using MasterCatalog = On.RoR2.MasterCatalog;
using Object = UnityEngine.Object;
using Resources = FloodWarning.Properties.Resources;
using Run = On.RoR2.Run;

namespace FloodWarning.Artifacts
{
    [R2APISubmoduleDependency]
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


        public static List<object> GetAllOfType<T>()
        {
            List<object> output = new List<object>();

            foreach (var item in Addressables.ResourceLocators)
            foreach (var key in item.Keys)
            {
                var asset = Addressables.LoadAssetAsync<Object>(key).WaitForCompletion();
                if (asset is T) output.Add(asset);
            }

            return output;
        }

        public static List<object> GetAllWithComponent<T>()
        {
            List<object> output = new List<object>();

            foreach (var item in Addressables.ResourceLocators)
            foreach (var key in item.Keys)
            {
                var asset = Addressables.LoadAssetAsync<Object>(key).WaitForCompletion();
                if (asset is T) output.Add(asset);
            }

            return output;
        }

        public static void DoHooks()
        {
            DoConfig();

            MasterCatalog.Init += delegate(MasterCatalog.orig_Init orig)
            {
                Log.LogInfo("Init begun");
                orig();

                foreach (GameObject master in ContentManager.masterPrefabs)
                    try
                    {
                        CharacterMaster innerMaster = master.GetComponent<CharacterMaster>();

                        GameObject bodyPrefab = innerMaster.bodyPrefab;
                        if (!bodyPrefab) break;

                        CharacterBody characterBody = bodyPrefab.GetComponentInChildren<CharacterBody>();

                        if (!characterBody) break;

                        //Check to see if this is a survivor, as only survivors have preferred pods.
                        bool hasPreferredPodPrefab = characterBody.preferredPodPrefab != null;

                        //Health Calculation
                        float baseHealth = characterBody.baseMaxHealth + characterBody.baseMaxShield +
                                           characterBody.baseArmor;
                        float levelHealth = characterBody.levelMaxHealth + characterBody.levelMaxShield +
                                            characterBody.levelArmor;
                        float totalHealth = baseHealth + levelHealth;

                        //DPS calculation
                        float baseDPS = characterBody.baseDamage * characterBody.baseAttackSpeed;
                        float levelDPS = characterBody.levelDamage + characterBody.levelAttackSpeed;
                        float totalDPS = baseDPS + levelDPS;

                        //The total cost of the character to spawn. Completely arbitrary.
                        float bodyCredit = totalDPS / 8f + (totalHealth * 2);

                        //survivors cost 25x, due to disproportionate health and damage values. Completely arbitrary.
                        bodyCredit *= hasPreferredPodPrefab ? 25 : 1;
                        bodyCredit *= characterBody.name.Contains("Brother") ? 8 : 1;

                        //Invert weights, makes game not wait for ages to spawn larger enemies, and favor hordes/beetles. Completely arbitrary.
                        int weight = 2000 - Mathf.Clamp((int)bodyCredit, 0, 2000);
                        //set weights 0-50
                        weight /= 40;
                        //Make it such that nothing has a weight of 0.
                        weight += 1;

                        //Bring costs more in line with base game.
                        bodyCredit /= 12f;


                        CharacterSpawnCard characterSpawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
                        characterSpawnCard.name = "cscDISSONANCE" + master.name;
                        characterSpawnCard.prefab = master;
                        characterSpawnCard.sendOverNetwork = true;
                        characterSpawnCard.hullSize = 0;
                        characterSpawnCard.nodeGraphType = 0;
                        characterSpawnCard.requiredFlags = 0;
                        characterSpawnCard.forbiddenFlags =
                            NodeFlags.NoCharacterSpawn;
                        characterSpawnCard.directorCreditCost = Mathf.FloorToInt(bodyCredit);
                        characterSpawnCard.occupyPosition = false;
                        characterSpawnCard.loadout = new SerializableLoadout();
                        characterSpawnCard.noElites = false;
                        characterSpawnCard.forbiddenAsBoss = false;

                        DirectorCard directorCard = new DirectorCard
                        {
                            spawnCard = characterSpawnCard,
                            selectionWeight = weight,
                            preventOverhead = false,
                            minimumStageCompletions = 0,
                            spawnDistance = 0,
                        };

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
                            InteractableCategory = DirectorAPI.InteractableCategory.Invalid
                        };
                        Log.LogInfo("\nNew Card Created: " + characterBody.name + "\n Category: " + monsterCategory +
                                    "\nCredits: " + bodyCredit + "\nWeight: " + weight);

                        Run.Start += delegate(Run.orig_Start start, RoR2.Run self)
                        {
                            //Seems to be an issue with adding the same monster multiple times, so we need to guarantee a fresh list every run.
                            DirectorAPI.Helpers.RemoveExistingMonster(monsterCard.Card.spawnCard.name);

                            if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                            {
                                DirectorAPI.Helpers.AddNewMonster(monsterCard, true);
                            }

                            start(self);
                        };

                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
            };

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
                            deathRewards.goldReward =
                                (uint) (cardresult.spawnRequest.spawnCard.directorCreditCost * 0.25f);

                        if (deathRewards.expReward < 1U)
                            deathRewards.expReward =
                                (uint) (cardresult.spawnRequest.spawnCard.directorCreditCost * 0.25f);

                        bodyStartBody.master.money = (uint) cardresult.spawnRequest.spawnCard.directorCreditCost;
                    };
            };
        }
    }
}