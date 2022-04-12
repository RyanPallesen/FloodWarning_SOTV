using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using BuffCatalog = On.RoR2.BuffCatalog;
using CharacterBody = RoR2.CharacterBody;
using CharacterMaster = On.RoR2.CharacterMaster;

namespace FW_Tweaks.Tweaks
{
    class SizeTweaks
    {
        public static void DoHooks()
        {
            On.RoR2.CharacterMaster.Awake += delegate(CharacterMaster.orig_Awake orig, RoR2.CharacterMaster self)
            {
                orig(self);

                self.onBodyStart += delegate(CharacterBody body)
                {
                    foreach (EliteIndex eliteIndex in EliteCatalog.eliteList)
                    {
                        if (body.HasBuff(EliteCatalog.GetEliteDef(eliteIndex).eliteEquipmentDef.passiveBuffDef))
                        {
                            body.modelLocator.modelBaseTransform.localScale *= 1.2f;
                        }
                    }

                    if (body.isChampion)
                    {
                        body.modelLocator.modelBaseTransform.localScale *= 1.4f;
                    }

                    if (body.isBoss)
                    {
                        body.modelLocator.modelBaseTransform.localScale *= 1.4f;
                    }
                };
            };
        }
    }
}
