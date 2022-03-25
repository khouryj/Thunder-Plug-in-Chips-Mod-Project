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
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }
        //hook that actually works this time
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            CharacterBody cb;
            try
            {
                cb = damageInfo.attacker.GetComponent<CharacterBody>();
            }
            catch
            {
                orig(self, damageInfo, victim);
                return;
            }

            if (cb && cb.inventory)
            {
                if (cb.inventory.GetItemCount(itemDef) > 0 && cb.healthComponent.health >= cb.healthComponent.fullHealth)
                {
                    damageInfo.damage *= (1 + (damage + (increments * (cb.inventory.GetItemCount(itemDef) - 1))));
                }
            }
            orig(self, damageInfo, victim);
        }
    }
}