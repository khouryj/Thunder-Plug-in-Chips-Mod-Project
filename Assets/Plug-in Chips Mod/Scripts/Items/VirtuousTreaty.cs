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
        public override string Desc => "On hit, enemies will gain 1 stack of logic virus. Building up 30 stacks of logic virus will cause the enemy to short-circuit, taking 20% of their max HP as damage. The enemy will lose 10 stacks of logic virus every 5 seconds.";
        public override string Lore => "";

        private BuffDef logicVirus;

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
        }

        private void LoseStacks(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (self)
            {
                if (buffDef == logicVirus)
                {
                    var count = self.GetBuffCount(buffDef) >= 11 ? self.GetBuffCount(buffDef) : -1;
                    orig(self, buffDef);
                    if (count != -1)
                    {
                        self.AddTimedBuff(logicVirus, 5, 30);
                        self.SetBuffCount(logicVirus.buffIndex, count - 10);
                    }
                    return;
                }
            }
            orig(self, buffDef);
        }

        private void ApplyVirus(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            CharacterBody cb = damageInfo.attacker.GetComponent<CharacterBody>();
            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
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
                        victimBody.RemoveBuff(logicVirus);
                        victimBody.healthComponent.TakeDamage(new DamageInfo { 
                            attacker = cb.gameObject, 
                            crit = false, 
                            damage = .2f * victimBody.healthComponent.fullHealth, 
                            damageColorIndex = DamageColorIndex.Bleed,
                            force = Vector3.zero,
                            canRejectForce = false,
                            damageType = DamageType.Generic,
                            dotIndex = 0,
                            inflictor = victimBody.gameObject,
                            rejected = false 
                        });
                    }
                }
            }
        }
    }
}
