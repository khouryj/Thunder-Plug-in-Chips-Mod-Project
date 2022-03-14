using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using static PlugInChipsMod.PlugInChips;
using System;

namespace PlugInChipsMod.Scripts
{
    public class TauntUp : CustomItem<TauntUp>
    {
        public override string Name => "Taunt-Up";
        public override string Pickup => "Striking an enemy causes them to permanently take more damage, however they also deal more damage and move faster.";
        public override string Desc => $"Striking an enemy sends them into a rage, permanently taking <style=cIsDamage>{DamageTakenIncrease.Value}% more damage.</style> " +
            $"However, they will also deal <style=cIsDamage>{DamageIncrease.Value}% more damage</style> and <style=cIsUtility>move {MovementSpeedIncrease.Value}% faster.</style>" +
            $"<style=cStack> +{DamageIncrease.Value}% damage & {MovementSpeedIncrease.Value}% movement speed per stack</style>";
        public override string Lore => "This chip was lost by a unit who got too greedy, and fought an enemy too powerful for them. Hopefully you do not end up in the same situation.";

        private BuffDef Taunted;
        private CharacterBody victimbody;
        public ConfigEntry<float> DamageIncrease, DamageTakenIncrease, MovementSpeedIncrease;

        public override void Init(ConfigFile config)
        {
            itemDef = serializeableContentPack.itemDefs[4];
            Taunted = serializeableContentPack.buffDefs[2];

            SetupConfig(config);
            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupConfig(ConfigFile config)
        {
            DamageIncrease = config.Bind<float>("Item: " + Name, "Damage Increase", 100f, "What percent more damage should an enemy do when taunted?");
            DamageTakenIncrease = config.Bind<float>("Item: " + Name, "Damage Taken Increase", 100f, "What percent more damage should an enemy receive when taunted?");
            MovementSpeedIncrease = config.Bind<float>("Item: " + Name, "Movement Speed Increase", 50f, "What percent should enemy movement speed increase when taunted?");
        }

        protected override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += ApplyBuff;
        }

        private void ApplyBuff(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo?.attacker)
            {
                int inventoryCount = 0;
                try
                {
                    victimbody = self.GetComponent<CharacterBody>();
                    inventoryCount = damageInfo.attacker.GetComponent<CharacterBody>().inventory.GetItemCount(itemDef);
                }
                catch (NullReferenceException NRE)
                {
                    PlugInChips.instance.Logger.LogWarning("GameObject does not have a valid characterbody");
                    orig(self, damageInfo);
                    return;
                }
                if (inventoryCount > 0 && !victimbody.HasBuff(Taunted) && victimbody != damageInfo.attacker.GetComponent<CharacterBody>())
                {
                    victimbody.baseDamage += (DamageIncrease.Value * inventoryCount / 100 * victimbody.baseDamage);
                    victimbody.baseMoveSpeed += (MovementSpeedIncrease.Value * inventoryCount / 100 * victimbody.baseMoveSpeed);
                    victimbody.AddBuff(Taunted);
                }
                else if (inventoryCount > 0 && victimbody.HasBuff(Taunted))
                {
                    damageInfo.damage += (DamageTakenIncrease.Value * inventoryCount / 100 * damageInfo.damage);
                }
            }
            orig(self, damageInfo);
        }
    }
}
