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
        public override string Desc => "On hit, enemies will have a <style=cIsUtility>75% * Proc Coefficient</style> chance to gain 1 stack of <style=cDeath>logic virus</style>. Building up 25 stacks of logic virus will cause the enemy to <style=cIsUtility>short-circuit</style>, taking <style=cIsDamage>20%</style> <style=cStack>+10% per stack</style> of their current COMBINED health as damage. The enemy will lose all stacks of logic virus every <style=cIsUtility>8 seconds.</style>";
        public override string Lore => "\"How many androids do you think you've killed? You think begging for your lives is going to help? You think that is going to make me forget everything!\" -A2";

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
        }

        private void ApplyVirus(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            CharacterBody cb = damageInfo?.attacker.GetCharacterBody();
            CharacterBody victimBody = victim.GetCharacterBody();
            if (!cb || !victimBody) { return; }
            if (cb.inventory.GetItemCount(itemDef) > 0)
            {
                if (Util.CheckRoll((.75f * damageInfo.procCoefficient) * 100, cb.master))
                {
                    victimBody.AddTimedBuff(logicVirus, 8, 25);
                }
            }
            if (victimBody.HasBuff(logicVirus))
            {
                if (victimBody.GetBuffCount(logicVirus) == 25)
                {
                    victimBody.healthComponent.TakeDamage(new DamageInfo { 
                        attacker = cb.gameObject, 
                        crit = false, 
                        damage = Utilities.CalculateChange(.2f, .1f, cb.inventory.GetItemCount(itemDef)) * victimBody.healthComponent.fullCombinedHealth, 
                        damageColorIndex = DamageColorIndex.Bleed,
                        force = Vector3.zero,
                        canRejectForce = false,
                        damageType = DamageType.Generic,
                        dotIndex = 0,
                        inflictor = victim,
                        position = victimBody.corePosition,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 0,
                        rejected = false 
                    });
                    victimBody.ClearTimedBuffs(logicVirus);
                }
            }
        }
    }
}
