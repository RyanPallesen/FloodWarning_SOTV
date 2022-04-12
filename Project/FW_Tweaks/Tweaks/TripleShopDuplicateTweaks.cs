using System;
using System.Collections.Generic;
using On.RoR2;
using UnityEngine;
using ShopTerminalBehavior = RoR2.ShopTerminalBehavior;

namespace FW_Tweaks.Tweaks
{
    internal class TripleShopDuplicateTweaks
    {
        public static void DoHooks()
        {
            MultiShopController.CreateTerminals += delegate(MultiShopController.orig_CreateTerminals orig,
                RoR2.MultiShopController self)
            {
                orig(self);

                List<ShopTerminalBehavior> shopTerminals = new List<ShopTerminalBehavior>();

                foreach (GameObject terminal in self.terminalGameObjects)
                    shopTerminals.Add(terminal.GetComponent<ShopTerminalBehavior>());

                for (int i = 0; i < shopTerminals.Count; i++)
                for (int o = 0; i < shopTerminals.Count; o++)
                    //Don't compare to self
                    if (i != o)
                        if (shopTerminals[i].CurrentPickupIndex() == shopTerminals[o].CurrentPickupIndex())
                        {
                            shopTerminals[i].GenerateNewPickupServer(shopTerminals[i].pickupIndexIsHidden);
                            shopTerminals[o].GenerateNewPickupServer(shopTerminals[o].pickupIndexIsHidden);
                        }
            };
        }
    }
}