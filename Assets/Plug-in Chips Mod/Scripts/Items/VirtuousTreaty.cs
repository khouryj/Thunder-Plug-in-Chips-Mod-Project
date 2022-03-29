using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System;

namespace PlugInChipsMod.Scripts
{
    public class VirtuousTreaty : CustomItem<VirtuousTreaty>
    {
        public override string Name => "Virtuous Treaty";
        public override string Pickup => "Hit enemies to apply quickly-decaying stacks of logic virus. Full stacks damages enemies instantly.";
        public override string Desc => "On hit, enemies will gain 1 stack of logic virus. Building up 30 stacks of logic virus will cause the enemy to short-circuit, taking 20% of their current COMBINED health as damage. The enemy will lose 10 stacks of logic virus every 5 seconds.";
        public override string Lore => "";

        private BuffDef logicVirus;
        private int count;

        public override void Init(ConfigFile config)
        {
            itemDef = Utilities.VirtuousTreaty;
            logicVirus = Utilities.LogicVirus;

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupHooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += ApplyVirus;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += LoseStacks;
            On.RoR2.CharacterBody.SetBuffCount += SaveStacks;
        }

        private void SaveStacks(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            if (buffType == logicVirus.buffIndex && newCount == 0)
            {
                if (self)
                {
                    count = self.GetBuffCount(logicVirus);
                    PlugInChips.instance.Logger.LogMessage("count saved");
                }
            }
            orig(self, buffType, newCount);
        }

        private void LoseStacks(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (buffDef == logicVirus)
            {
                if (self)
                {
                    orig(self, buffDef);
                    if (count >= 11 && count != 30)
                    {
                        self.AddTimedBuff(buffDef, 5, 30);
                        self.SetBuffCount(buffDef.buffIndex, count - 10);
                        count = 0;
                        PlugInChips.instance.Logger.LogMessage("buff reset");
                    }
                    return;
                }
            }
            orig(self, buffDef);
        }

        private void ApplyVirus(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            CharacterBody cb = damageInfo?.attacker?.GetComponent<CharacterBody>();
            CharacterBody victimBody = victim?.GetComponent<CharacterBody>();
            if (cb && victimBody && cb.inventory)
            {
                if (cb.inventory.GetItemCount(itemDef) > 0)
                {
                    victimBody.AddTimedBuff(logicVirus, 5, 30);
                }
                if (victimBody.HasBuff(logicVirus))
                {
                    if (victimBody.GetBuffCount(logicVirus) == 30)
                    {
                        victimBody.healthComponent.TakeDamage(new DamageInfo { 
                            attacker = cb.gameObject, 
                            crit = false, 
                            damage = .2f * victimBody.healthComponent.combinedHealth, 
                            damageColorIndex = DamageColorIndex.Bleed,
                            force = Vector3.zero,
                            canRejectForce = false,
                            damageType = DamageType.Generic,
                            dotIndex = 0,
                            inflictor = victim,
                            rejected = false 
                        });
                        victimBody.SetBuffCount(logicVirus.buffIndex, 0);
                    }
                }
            }
        }
    }
}
