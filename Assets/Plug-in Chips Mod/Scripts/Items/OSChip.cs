using BepInEx.Configuration;
using System.Collections;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static PlugInChipsMod.Scripts.Utilities;
using static PlugInChipsMod.PlugInChips;
using RoR2.Items;
using System;
using RoR2.Projectile;

namespace PlugInChipsMod.Scripts
{
    public class OSChip : CustomItem<OSChip>
    {
        public override string Name => "OS Chip";
        public override string Pickup => "The combined power of all plug-in chips. <style=cIsVoid>Corrupts all plug-in chips.</style>";
        public override string Desc => "This chip will apply two randomly selected <style=cIsUtility>SUPERCHARGED</style> effects of the other plugin chips for <style=cIsUtility>30 seconds</style>, then cooldown for <style=cIsUtility>15 seconds</style>. <style=cIsVoid>Corrupts all plug-in chips</style>";
        public override string Lore => "The core chip of any unit.\n <style=cIsHealth>Note: Removal of this chip means total malfunction of unit. Do not remove under any circumstances.</style>";
        //Set dlc requirement for void item, see base for implementation
        public override bool dlcRequired => true;
        //ItemDef.Pair array used to add void conversions to the dictionary in the base game
        private static ItemDef.Pair[] conversions;
        //variables used for handling buffs
        private static BuffDef ActiveBuff1, ActiveBuff2;
        private static BuffDef[] buffs;
        private static System.Random rnd = new System.Random();
        private float cooldownTime, buffTime;


        public override void Init(ConfigFile config)
        {
            itemDef = osChip;
            cooldownTime = 15f;
            buffTime = 30f;
            ActiveBuff1 = null;
            ActiveBuff2 = null;
            buffs = new BuffDef[] { SuperAntiChain, SuperDeadlyHeal, SuperOffensiveHeal, SuperShockwave, SuperTaunt };

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupHooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += new Action<DamageReport>(superDeadlyHeal);
            On.RoR2.HealthComponent.TakeDamage += superTaunt;
            On.RoR2.GlobalEventManager.OnHitEnemy += superOffensive;
            On.RoR2.CharacterBody.OnTakeDamageServer += superAntiChain;
            On.RoR2.CharacterBody.OnSkillActivated += superShockwave;
            Stage.onStageStartGlobal += RestartBuff;
            On.RoR2.CharacterBody.OnInventoryChanged += Detect;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CycleBuffs;
            Run.onRunStartGlobal += Reset;
            On.RoR2.Items.ContagiousItemManager.Init += InitCorrupted;
        }

        private void InitCorrupted(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            conversions = new ItemDef.Pair[5];
            conversions[0] = new ItemDef.Pair
            {
                itemDef1 = deadlyHeal,
                itemDef2 = osChip
            };
            conversions[1] = new ItemDef.Pair
            {
                itemDef1 = shockwave,
                itemDef2 = osChip
            };
            conversions[2] = new ItemDef.Pair
            {
                itemDef1 = antiChainDamage,
                itemDef2 = osChip
            };
            conversions[3] = new ItemDef.Pair
            {
                itemDef1 = offensiveHeal,
                itemDef2 = osChip
            };
            conversions[4] = new ItemDef.Pair
            {
                itemDef1 = tauntUp,
                itemDef2 = osChip
            };
            //This method comes from Utilities but it simply adds it to the array in the dictionary
            InitializeCorruptedItem(conversions);
            orig();
        }

        //Resets buff values to default at the beginning of every run to avoid bugs
        private void Reset(Run obj)
        {
            ActiveBuff1 = null;
            ActiveBuff2 = null;
            cooldownTime = 15f;
            buffTime = 30f;
        }
        //Hook on buff lost to add the new buffs
        private void CycleBuffs(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (self && buffDef)
            {
                if (buffDef == cooldown)
                {
                    GetCurrentBuffs();
                    self.AddTimedBuff(ActiveBuff1, buffTime);
                    self.AddTimedBuff(ActiveBuff2, buffTime);
                }
                else if (buffDef == ActiveBuff1 || buffDef == ActiveBuff2)
                {
                    self.AddTimedBuff(cooldown, cooldownTime);
                    ActiveBuff1 = null;
                    ActiveBuff2 = null;
                }
            }
            orig(self, buffDef);
        }
        //Hook to add the buff the first time character acquires the item in the run
        private void Detect(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (self)
            {
                if (self.inventory.GetItemCount(itemDef) >= 1 && ActiveBuff1 == null && ActiveBuff2 == null && !self.HasBuff(cooldown))
                {
                    GetCurrentBuffs();
                    self.AddTimedBuff(ActiveBuff1, buffTime);
                    self.AddTimedBuff(ActiveBuff2, buffTime);
                }
            }
            orig(self);
        }
        //Gives the local player the buffs at the beginning of each stage
        private void RestartBuff(Stage obj)
        {
            ActiveBuff1 = null;
            ActiveBuff2 = null;
            CharacterBody cb = PlayerCharacterMasterController.instances[0].body;
            if (cb && cb.inventory.GetItemCount(itemDef) > 0)
            {
                GetCurrentBuffs();
                cb.AddTimedBuff(ActiveBuff1, buffTime);
                cb.AddTimedBuff(ActiveBuff2, buffTime);
                return;
            }
        }

        private void superShockwave(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            if (self.HasBuff(SuperShockwave))
            {
                var inventoryCount = self.inventory.GetItemCount(itemDef);
                if (inventoryCount > 0)
                {
                    if (skill.skillFamily == self.GetComponent<SkillLocator>().primary.skillFamily)
                    {
                        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
                        {
                            owner = self.gameObject,
                            damage = self.baseDamage * (2f + (.5f * (inventoryCount - 1))),
                            position = self.corePosition,
                            rotation = Util.QuaternionSafeLookRotation(self.inputBank.aimDirection),
                            crit = false,
                            projectilePrefab = Projectiles.shockwaveProjectile
                        };
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                }
            }
            orig(self, skill);
        }

        private void superAntiChain(On.RoR2.CharacterBody.orig_OnTakeDamageServer orig, CharacterBody self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self && self.inventory)
            {
                if (self.HasBuff(SuperAntiChain))
                {
                    var inventoryCount = self.inventory.GetItemCount(itemDef);
                    if (inventoryCount > 0 && !self.HasBuff(RoR2Content.Buffs.Immune))
                    {
                        if (!damageReport.isFallDamage)
                        {
                            self.AddTimedBuff(RoR2Content.Buffs.Immune, 3f + (.01f * (inventoryCount - 1)));
                        }
                    }
                }
            }
        }

        private void superOffensive(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            if (victim && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>() ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                if (attackerBody && attackerBody.inventory)
                {
                    if (attackerBody.HasBuff(SuperOffensiveHeal))
                    {
                        var inventoryCount = attackerBody.inventory.GetItemCount(itemDef);
                        if (inventoryCount > 0)
                        {
                            attackerBody.healthComponent.Heal(damageInfo.damage * (.20f + (.05f * (inventoryCount - 1))), default(ProcChainMask));
                        }
                    }
                }
            }
        }

        private void superTaunt(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo?.attacker)
            {
                int inventoryCount = 0;
                CharacterBody victimbody;
                CharacterBody attackerbody;
                try
                {
                    victimbody = self.GetComponent<CharacterBody>();
                    attackerbody = damageInfo.attacker.GetComponent<CharacterBody>();
                    inventoryCount = attackerbody.inventory.GetItemCount(itemDef);
                }
                catch (Exception)
                {
                    orig(self, damageInfo);
                    return;
                }
                if (attackerbody.HasBuff(SuperTaunt))
                {
                    if (inventoryCount > 0 && !victimbody.HasBuff(Taunted) && victimbody != attackerbody)
                    {
                        victimbody.AddBuff(Taunted);
                    }
                    else if (inventoryCount > 0 && victimbody.HasBuff(Taunted))
                    {
                        damageInfo.damage *= 3f + (.25f * (inventoryCount - 1));
                    }
                }
            }
            orig(self, damageInfo);
        }

        private void superDeadlyHeal(DamageReport obj)
        {
            if (obj.attackerBody)
            {
                if (obj.attackerBody.HasBuff(SuperDeadlyHeal))
                {
                    var inventoryCount = obj.attackerBody.inventory.GetItemCount(itemDef);
                    if (inventoryCount > 0)
                    {
                        obj.attackerBody.healthComponent.HealFraction(.3f + (.06f * (inventoryCount - 1)), default(ProcChainMask));
                    }
                }
            }
        }
        //Randomly acquires two different buffs
        private static void GetCurrentBuffs()
        {
            ActiveBuff1 = buffs[rnd.Next(0, 5)];
            ActiveBuff2 = buffs[rnd.Next(0, 5)];
            while (ActiveBuff2 == ActiveBuff1) { ActiveBuff2 = buffs[rnd.Next(0, 5)]; }
            PlugInChips.instance.Logger.LogMessage("buffs selected: " + ActiveBuff1.name + ", " + ActiveBuff2.name);
        }
    }
}