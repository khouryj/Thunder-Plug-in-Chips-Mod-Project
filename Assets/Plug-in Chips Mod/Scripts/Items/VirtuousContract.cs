using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace PlugInChipsMod.Scripts
{
    public class VirtuousContract : CustomItem<VirtuousContract>
    {
        public override string Name => "Virtuous Contract";
        public override string Pickup => "Deal more damage while at full health";
        public override string Desc => $"Deal {VirtuousBaseDamage.Value}% more damage when you are at full health.";
        public override string Lore => "";

        public ConfigEntry<float> VirtuousBaseDamage, VirtuousDamageIncrease;
        private float damage, increments;

        public override void Init(ConfigFile config)
        {
            itemDef = Utilities.VirtuousContract;

            SetupConfig(config);
            //Set values after the config initializes in case of NRE or user change
            damage = VirtuousBaseDamage.Value / 100;
            increments = VirtuousDamageIncrease.Value / 100;

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupConfig(ConfigFile config)
        {
            VirtuousBaseDamage = config.Bind<float>("Item: " + Name, "Base Damage Increase", 10f, "By what percent should Virtuous Contract increase damage at base?");
            VirtuousDamageIncrease = config.Bind<float>("Item: " + Name, "Damage Increments", 5f, "How much should the percentage increase per stack?");
        }

        protected override void SetupHooks()
        {
            RoR2.GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
        //Event subscription that adds damage if health is full
        private void GlobalEventManager_onServerDamageDealt(RoR2.DamageReport obj)
        {
            if (obj.attackerBody && obj.attackerBody.inventory && !obj.isFallDamage)
            {
                if (obj.attackerBody.inventory.GetItemCount(itemDef) > 0 && obj.attackerBody.healthComponent.health >= obj.attackerBody.healthComponent.fullHealth)
                {
                    obj.damageDealt *= (1 + (damage + (increments * (obj.attackerBody.inventory.GetItemCount(itemDef) - 1))));
                }
            }
        }
    }
}