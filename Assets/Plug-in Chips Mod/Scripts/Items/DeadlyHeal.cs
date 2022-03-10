﻿using System;
using RoR2;
using BepInEx.Configuration;
using R2API;
using static PlugInChipsMod.PlugInChips;

namespace PlugInChipsMod.Scripts
{
    public class DeadlyHeal : CustomItem<DeadlyHeal>
    {
        public override string Name => "Deadly Heal";
        public override string Pickup => "Heals you on kills.";
        public override string Desc => $"Each kill heals you for <style=cIsHealing>{BaseHealingAmount.Value + "%"}</style> of your max health. <style=cStack>{"+" + BaseHealingAmountIncrements.Value + "% per stack"}</style>";
        public override string Lore => "This plug-in chip has assisted many units in staying alive in the face of many enemies.";

        public ConfigEntry<float> BaseHealingAmount;
        public ConfigEntry<float> BaseHealingAmountIncrements;

        private ItemDef deadlyHeal;

        public override void Init(ConfigFile config)
        {
            deadlyHeal = serializeableContentPack.itemDefs[1];

            PlugInChips.instance.Logger.LogMessage("Initializing Deadly Heal");
            SetupConfig(config);
            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupConfig(ConfigFile config)
        {
            BaseHealingAmount = config.Bind<float>("Item: " + Name, "Base Healing Amount", 5f, "What percent should the base healing amount be?");
            BaseHealingAmountIncrements = config.Bind<float>("Item: " + Name, "Base Healing Amount Increments", 5f, "How much should the percentage rise per stack?");
        }

        protected override void SetupLanguage()
        {
            PlugInChips.instance.Logger.LogMessage("Making tokens");
            LanguageAPI.Add(deadlyHeal.nameToken, Name);
            LanguageAPI.Add(deadlyHeal.pickupToken, Pickup);
            LanguageAPI.Add(deadlyHeal.descriptionToken, Desc);
            LanguageAPI.Add(deadlyHeal.loreToken, Lore);
        }

        protected override void SetupHooks()
        {
            PlugInChips.instance.Logger.LogMessage("Hooking");
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport?.attackerBody)
            {
                var inventoryCount = damageReport.attackerBody.inventory.GetItemCount(deadlyHeal);
                float stackIncrease = BaseHealingAmountIncrements.Value / 100;
                float healingPercent = BaseHealingAmount.Value / 100;
                if (inventoryCount == 1)
                {
                    damageReport.attackerBody.healthComponent.HealFraction(healingPercent, default(ProcChainMask));
                }
                else if (inventoryCount >= 2)
                {
                    damageReport.attackerBody.healthComponent.HealFraction((healingPercent + (stackIncrease * inventoryCount) - stackIncrease), default(ProcChainMask));
                }
            }
        }
    }
}