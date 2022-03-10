﻿using RoR2;
using R2API;
using BepInEx.Configuration;
using static PlugInChipsMod.PlugInChips;
using System;

namespace PlugInChipsMod.Scripts
{
    public class AntiChainDamage : CustomItem<AntiChainDamage>
    {
        public override string Name => "Anti-Chain Damage";
        public override string Pickup => "Taking damage from an enemy makes you invulnerable for a short time.";
        public override string Desc => $"Getting hit by any enemy makes you invulnerable for <style=cIsUtility>{BaseInvulnerabilityTime.Value + " second(s). "}</style>"
                               + $"<style=cStack>{"+" + InvulnerabilityTimeIncrements.Value + " second(s) per stack"}</style>";
        public override string Lore => "This plug-in chip assisted many units in surviving against barrages of attacks from enemies.";

        private ItemDef antiChain;
        private BuffDef hiddenCooldown;

        private ConfigEntry<float> BaseInvulnerabilityTime;
        private ConfigEntry<float> InvulnerabilityTimeIncrements;

        public override void Init(ConfigFile config)
        {
            antiChain = serializeableContentPack.itemDefs[0];
            hiddenCooldown = serializeableContentPack.buffDefs[4];

            PlugInChips.instance.Logger.LogMessage("Initializing Anti-Chain Damage");

            SetupConfig(config);
            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupConfig(ConfigFile config)
        {
            BaseInvulnerabilityTime = config.Bind<float>("Item: " + Name, "Base Invulnerability Time", 1.5f, "How long should the base invulnerability time be?");
            InvulnerabilityTimeIncrements = config.Bind<float>("Item: " + Name, "Invulnerability Time Increments", .5f, "How much time should invulnerability increase per stack?");
        }

        protected override void SetupLanguage()
        {
            PlugInChips.instance.Logger.LogMessage("Making tokens");
            LanguageAPI.Add(antiChain.nameToken, Name);
            LanguageAPI.Add(antiChain.pickupToken, Pickup);
            LanguageAPI.Add(antiChain.descriptionToken, Desc);
            LanguageAPI.Add(antiChain.loreToken, Lore);
        }

        protected override void SetupHooks()
        {
            On.RoR2.CharacterBody.OnTakeDamageServer += Invulnerability;
        }

        private void Invulnerability(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            var inventoryCount = self.inventory.GetItemCount(antiChain);
            if (inventoryCount == 1 && !self.HasBuff(RoR2Content.Buffs.Immune) && !self.HasBuff(hiddenCooldown))
            {
                if (!damageReport.isFallDamage)
                {
                    self.AddTimedBuffAuthority(RoR2Content.Buffs.Immune.buffIndex, BaseInvulnerabilityTime.Value);
                    if (self?.teamComponent.teamIndex != TeamIndex.Player)
                    {
                        self.AddTimedBuff(hiddenCooldown, 5f);
                    }
                }
            }
            else if (inventoryCount >= 2 && !damageReport.isFallDamage && !self.HasBuff(RoR2Content.Buffs.Immune) && !self.HasBuff(hiddenCooldown))
            {
                self.AddTimedBuffAuthority(RoR2Content.Buffs.Immune.buffIndex, BaseInvulnerabilityTime.Value + (InvulnerabilityTimeIncrements.Value * inventoryCount) - InvulnerabilityTimeIncrements.Value);
                if (self?.teamComponent.teamIndex != TeamIndex.Player)
                {
                    self.AddTimedBuff(hiddenCooldown, 5f);
                }
            }
            orig(self, damageReport);
        }
    }
}