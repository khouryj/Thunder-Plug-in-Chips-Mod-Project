using BepInEx.Configuration;
using RoR2;
using R2API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlugInChipsMod.PlugInChips;
using System;

namespace PlugInChipsMod.Scripts
{
    public class OffensiveHeal : CustomItem<OffensiveHeal>
    {
        public override string Name => "Offensive Heal";
        public override string Pickup => "Dealing damage heals you instantly.";
        public override string Desc => $"Whenever you damage an enemy, heal for <style=cIsHealing>{BaseHealingAmount.Value + "%"}</style> of damage dealt. <style=cStack>{"+" + BaseHealingAmountIncrements.Value + "% per stack"}</style>";
        public override string Lore => "This plug-in chip has assisted many units in staying alive against the largest of foes.";

        public ConfigEntry<float> BaseHealingAmount;
        public ConfigEntry<float> BaseHealingAmountIncrements;

        public override void Init(ConfigFile config)
        {
            base.itemDef = serializeableContentPack.itemDefs[2];

            SetupConfig(config);
            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupConfig(ConfigFile config)
        {
            BaseHealingAmount = config.Bind<float>("Item: " + Name, "Base Healing Amount", 10f, "What percent should the base healing amount be?");
            BaseHealingAmountIncrements = config.Bind<float>("Item: " + Name, "Base Healing Amount Increments", 5f, "How much should the percentage rise per stack?");
        }

        protected override void SetupHooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += HealPlayer;
        }

        private void HealPlayer(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, RoR2.GlobalEventManager self, RoR2.DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (victim && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>() ? null : damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody && attackerBody.inventory)
                {
                    var inventoryCount = attackerBody.inventory.GetItemCount(itemDef);
                    float stackIncrease = BaseHealingAmountIncrements.Value / 100;
                    float healingPercent = BaseHealingAmount.Value / 100;
                    if (inventoryCount == 1)
                    {
                        attackerBody.healthComponent.Heal((damageInfo.damage * healingPercent), default(ProcChainMask));
                    }
                    else if (inventoryCount >= 2)
                    {
                        attackerBody.healthComponent.Heal((damageInfo.damage * (healingPercent + (stackIncrease * inventoryCount) - stackIncrease)), default(ProcChainMask));
                    }
                }
            }
        }
    }
}
