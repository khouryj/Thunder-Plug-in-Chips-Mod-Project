using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API.Utils;

namespace PlugInChipsMod.Scripts
{
    public class VirtuousContract : CustomItem<VirtuousContract>
    {
        public override string Name => "Virtuous Contract";
        public override string Pickup => "Deal more damage while at full health";
        public override string Desc => $"Deal <style=cIsDamage>{VirtuousBaseDamage.Value}%</style> more damage when you are at <style=cIsHealing>full health</style>.";
        public override string Lore => "\"These are my memories. Take care of everyone...take care of the future.\" -2B";

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
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }
        //No joke, this hook works for real this time
        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!damageInfo?.attacker || !self)
            {
                orig(self, damageInfo);
                return;
            }
            CharacterBody cb = damageInfo?.attacker?.GetComponent<CharacterBody>();
            if (cb && cb.inventory)
            {
                if (cb.inventory.GetItemCount(itemDef) > 0 && cb.healthComponent?.health >= cb.healthComponent?.fullHealth)
                {
                    damageInfo.damage *= (1 + (damage + (increments * (cb.inventory.GetItemCount(itemDef) - 1))));
                }
            }
            orig(self, damageInfo);
        }
    }
}