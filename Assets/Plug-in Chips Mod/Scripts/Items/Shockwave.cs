using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using static PlugInChipsMod.PlugInChips;
using System;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace PlugInChipsMod.Scripts
{
    public class Shockwave : CustomItem<Shockwave>
    {
        public override string Name => "Shockwave";
        public override string Pickup => "Fires forward a shockwave attack when using your <style=cIsUtility>primary skill.</style>";
        public override string Desc => $"Fires forward a shockwave attack that deals <style=cIsDamage>{BaseDamage.Value}% damage</style>, and pierces enemies. <style=cStack>+{BaseDamageIncrements.Value}% per stack</style>";
        public override string Lore => "This chip has assisted units specialized in close quarters combat in taking down enemies at range.";

        public ConfigEntry<float> BaseDamage;
        public ConfigEntry<float> BaseDamageIncrements;

        private static GameObject shockwaveProjectile;

        public override void Init(ConfigFile config)
        {
            base.itemDef = serializeableContentPack.itemDefs[3];
            shockwaveProjectile = Projectiles.shockwaveProjectile;

            SetupConfig(config);
            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupConfig(ConfigFile config)
        {
            BaseDamage = config.Bind<float>("Item: " + Name, "Base Damage", 100f, "What percent of the character's base damage should the base shockwave damage be?");
            BaseDamageIncrements = config.Bind<float>("Item: " + Name, "Base Damage Increments", 50f, "How much should the percentage increase per stack?");
        }

        protected override void SetupHooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += FireShockwave;
        }

        private void FireShockwave(On.RoR2.CharacterBody.orig_OnSkillActivated orig, RoR2.CharacterBody self, RoR2.GenericSkill skill)
        {
            var inventoryCount = self.inventory.GetItemCount(itemDef);
            if (inventoryCount == 1)
            {
                if (skill.skillFamily == self.GetComponent<SkillLocator>().primary.skillFamily)
                {
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
                    {
                        owner = self.gameObject,
                        damage = BaseDamage.Value / 100 * self.baseDamage,
                        position = self.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(self.inputBank.aimDirection),
                        crit = false,
                        projectilePrefab = shockwaveProjectile
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
            else if (inventoryCount >= 2)
            {
                if (skill.skillFamily == self.GetComponent<SkillLocator>().primary.skillFamily)
                {
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
                    {
                        owner = self.gameObject,
                        damage = self.baseDamage * (BaseDamage.Value / 100 + BaseDamageIncrements.Value / 100 * inventoryCount - BaseDamageIncrements.Value / 100),
                        position = self.corePosition,
                        rotation = Util.QuaternionSafeLookRotation(self.inputBank.aimDirection),
                        procChainMask = default(ProcChainMask),
                        crit = false,
                        projectilePrefab = shockwaveProjectile
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
            orig(self, skill);
        }
    }
}