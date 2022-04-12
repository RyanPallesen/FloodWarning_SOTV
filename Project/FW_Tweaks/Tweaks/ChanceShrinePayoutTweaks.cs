using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;
using BuffCatalog = On.RoR2.BuffCatalog;
using CharacterBody = RoR2.CharacterBody;
using CharacterMaster = On.RoR2.CharacterMaster;
using ShrineChanceBehavior = On.RoR2.ShrineChanceBehavior;

namespace FW_Tweaks.Tweaks
{
    class ChanceShrinePayoutTweaks
    {
        public static void DoHooks()
        {
            On.RoR2.ShrineChanceBehavior.AddShrineStack += delegate(ShrineChanceBehavior.orig_AddShrineStack orig,
                RoR2.ShrineChanceBehavior self, Interactor activator)
            {
                orig(self, activator);

                self.tier1Weight *= 0.8f;
                self.tier2Weight *= 1.1f;
                self.tier3Weight *= 1.6f;
            };
        }
    }
}
