using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Navigation;
using UnityEngine;
using Console = System.Console;
using MasterCatalog = On.RoR2.MasterCatalog;
using Resources = FloodWarning.Artifacts.Properties.Resources;
using Run = On.RoR2.Run;
using TeleporterInteraction = On.RoR2.TeleporterInteraction;

namespace FloodWarning.Artifacts.Artifacts
{
    [R2APISubmoduleDependency]
    internal class TrueDissonance
    {
        private static readonly List<ArtifactDef> tempDefs = new List<ArtifactDef>();

        private static readonly List<DirectorAPI.DirectorCardHolder> trueDissonanceMonsters =
            new List<DirectorAPI.DirectorCardHolder>();

        private static ConfigFile mainConfigFile;

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
            mainConfigFile =
                new ConfigFile(
                    Paths.ConfigPath + "\\FloodWarning\\Artifacts\\TrueDissonance.cfg",
                    true);
        }

        public static float GetArbitraryCreditValue(CharacterBody characterBody)
        {
            //Check to see if this is a survivor, as only survivors have preferred pods.
            bool isCharacter = characterBody.preferredPodPrefab != null;
            isCharacter = characterBody.name.Contains("MonsterMaster") || isCharacter;

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
            float bodyCredit = totalDPS / 8f + totalHealth * 2;

            //survivors cost 25x, due to disproportionate health and damage values. Completely arbitrary.
            bodyCredit *= isCharacter ? 25 : 1;

            //Mithrix costs 8x more, due to skills making up for other things.
            bodyCredit *= characterBody.name.Contains("Brother") ? 8 : 1;

            //Bring costs more in line with base game.
            bodyCredit /= 12f;

            return bodyCredit;
        }

        public static int GetArbitraryWeightValue(float bodyCredit)
        {
            //Invert weights, makes game not wait for ages to spawn larger enemies, and favor hordes/beetles. Completely arbitrary.
            int weight = 2000 - Mathf.Clamp((int) bodyCredit, 0, 2000);
            //set weights 0-50
            weight /= 40;
            //Make it such that nothing has a weight of 0.
            weight += 1;

            return weight;
        }

        public static void DoHooks()
        {
            DoConfig();

            TeleporterInteraction.OnBossDirectorSpawnedMonsterServer += delegate(
                TeleporterInteraction.orig_OnBossDirectorSpawnedMonsterServer orig, RoR2.TeleporterInteraction self,
                GameObject masterObject)
            {
                orig(self, masterObject);
                if (!RunArtifactManager.instance ||
                    !RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                    return;

                if (!mainConfigFile.Bind("",
                    "shouldSuicide", true,
                    "Whether or not to kill bosses after a set amount of time").Value)
                    return;

                masterObject.AddComponent<MasterSuicideOnTimer>().lifeTimer = mainConfigFile.Bind("",
                    "suicideTimer", 180f,
                    "The time, in seconds, after which to kill a boss").Value;
            };

            MasterCatalog.Init += delegate(MasterCatalog.orig_Init orig)
            {
                orig();

                foreach (GameObject master in ContentManager.masterPrefabs) Debug.Log(master.name);

                foreach (GameObject master in ContentManager.masterPrefabs)
                {
                    if (master.name == "PlayerMaster")
                        continue;

                    ConfigFile perBodyConfigFile =
                        new ConfigFile(
                            Paths.ConfigPath + "\\FloodWarning\\Artifacts\\TrueDissonance\\" + master.name + ".cfg",
                            true);

                    //Create a config entry for this characterMaster
                    ConfigEntry<bool> shouldSpawn = perBodyConfigFile.Bind("",
                        "shouldSpawn", true,
                        "Whether or not the creature, " + master.name + ", should spawn with true dissonance");

                    //If we are enabling this to be chosen.
                    if (!shouldSpawn.Value) continue;

                    try
                    {
                        CharacterMaster innerMaster = master.GetComponent<CharacterMaster>();

                        GameObject bodyPrefab = innerMaster.bodyPrefab;
                        if (!bodyPrefab) continue;

                        CharacterBody characterBody = bodyPrefab.GetComponentInChildren<CharacterBody>();
                        if (!characterBody) continue;

                        float bodyCredit = GetArbitraryCreditValue(characterBody);

                        //Create a config entry for the credit cost
                        ConfigEntry<float> bodyCreditConfig = perBodyConfigFile.Bind("",
                            "bodyCredit", bodyCredit,
                            "The cost for the director to spawn this creature");
                        bodyCredit = bodyCreditConfig.Value;

                        int weight = GetArbitraryWeightValue(bodyCredit);
                        //Create a config entry for the weight
                        ConfigEntry<int> weightConfig = perBodyConfigFile.Bind("",
                            "weight", weight,
                            "The weighting to spawn this creature");
                        weight = weightConfig.Value;

                        //Create a config entry for the forbiddenAsBoss
                        ConfigEntry<bool> forbiddenAsBoss = perBodyConfigFile.Bind("",
                            "forbiddenAsBoss", false,
                            "Whether or not this creature can be a teleporter boss");

                        //Create a config entry for the noElites
                        ConfigEntry<bool> noElites = perBodyConfigFile.Bind("",
                            "noElites", false,
                            "Whether or not this creature cannot be an elite");

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
                        characterSpawnCard.noElites = noElites.Value;
                        characterSpawnCard.forbiddenAsBoss = forbiddenAsBoss.Value;

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

                        //Create the 'Card'
                        DirectorAPI.DirectorCardHolder monsterCard = new DirectorAPI.DirectorCardHolder
                        {
                            Card = directorCard,
                            MonsterCategory = monsterCategory,
                            InteractableCategory = DirectorAPI.InteractableCategory.Invalid
                        };
                        trueDissonanceMonsters.Add(monsterCard);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("True Dissonance unable to add character, " + master.name +
                                          ", Error thrown:" + e);
                        throw;
                    }
                }
            };

            Run.Start += delegate(Run.orig_Start start, RoR2.Run self)
            {
                foreach (DirectorAPI.DirectorCardHolder monsterCard in trueDissonanceMonsters)
                {
                    //Seems to be an issue with adding the same monster multiple times, so we need to guarantee a fresh list every run.
                    DirectorAPI.Helpers.RemoveExistingMonster(monsterCard.Card.spawnCard.name);

                    if (RunArtifactManager.instance &&
                        RunArtifactManager.instance.IsArtifactEnabled(tempDefs[0]))
                        DirectorAPI.Helpers.AddNewMonster(monsterCard, true);
                }

                start(self);
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