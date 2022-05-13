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
        internal static Dictionary<GameObject, OSChipComponent> objectsWithComponent;

        public override void Init(ConfigFile config)
        {
            itemDef = osChip;

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
            On.RoR2.Items.ContagiousItemManager.Init += InitCorrupted;
            Run.onRunStartGlobal += ResetDictionary;
        }

        private void ResetDictionary(Run obj)
        {
            objectsWithComponent = new Dictionary<GameObject, OSChipComponent>();
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

        //Hook on buff lost to add the new buffs
        private void CycleBuffs(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            if (self && buffDef)
            {
                bool exists = objectsWithComponent.TryGetValue(self.masterObject, out OSChipComponent component);
                if (exists && component)
                {
                    if (buffDef == cooldown)
                    {
                        component.GetCurrentBuffs();
                        component.HandleBuffs(false);
                    }
                    else if (buffDef == component.ActiveBuff1 || buffDef == component.ActiveBuff2)
                    {
                        component.HandleBuffs(true);
                    }
                }
            }
            orig(self, buffDef);
        }
        //Hook to add the buff the first time character acquires the item in the run
        private void Detect(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.master && self.inventory)
            {
                bool exists = objectsWithComponent.TryGetValue(self.masterObject, out OSChipComponent component);
                if (self.inventory.GetItemCount(osChip) > 0)
                {
                    if (exists)
                    {
                        return;
                    }
                    objectsWithComponent.Add(self.masterObject, self.masterObject.AddComponent<OSChipComponent>());
                    objectsWithComponent[self.masterObject].InitializeComponent(self.master);
                }
                else
                {
                    if (exists && component)
                    {
                        UnityEngine.Object.Destroy(component);
                    }
                }
            }
        }
        //Gives the local player the buffs at the beginning of each stage
        private void RestartBuff(Stage obj)
        {
            bool exists = objectsWithComponent.TryGetValue(CharacterMaster.readOnlyInstancesList[0].gameObject, out OSChipComponent component);
            if (exists && component)
            {
                component.GetCurrentBuffs();
                component.HandleBuffs(false);
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
                            damage = self.baseDamage * CalculateChange(2f, .5f, inventoryCount),
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
    }

    public class OSChipComponent : MonoBehaviour
    {
        public CharacterMaster characterMaster;
        public BuffDef ActiveBuff1 = null, ActiveBuff2 = null;
        public static BuffDef[] buffs = new BuffDef[] {SuperTaunt, SuperShockwave, SuperOffensiveHeal, SuperDeadlyHeal, SuperAntiChain};
        public static System.Random rnd = new System.Random();
        private float cooldownTime, buffTime;

        private void Awake()
        {
            cooldownTime = 15f;
            buffTime = 30f;
        }

        private void OnDestroy()
        {
            OSChip.objectsWithComponent.Remove(this.gameObject);
        }

        //Randomly acquires two different buffs
        public void GetCurrentBuffs()
        {
            ActiveBuff1 = buffs[rnd.Next(0, 5)];
            ActiveBuff2 = buffs[rnd.Next(0, 5)];
            while (ActiveBuff2 == ActiveBuff1) { ActiveBuff2 = buffs[rnd.Next(0, 5)]; }
        }

        public void HandleBuffs(bool isCooldown)
        {
            if (isCooldown)
            {
                characterMaster.GetBody().AddTimedBuff(cooldown, cooldownTime);
                ActiveBuff1 = null;
                ActiveBuff2 = null;
                return;
            }
            characterMaster.GetBody().AddTimedBuff(ActiveBuff1, buffTime);
            characterMaster.GetBody().AddTimedBuff(ActiveBuff2, buffTime);

        }

        public void InitializeComponent(CharacterMaster characterMaster)
        {
            this.characterMaster = characterMaster;
            GetCurrentBuffs();
            HandleBuffs(false);
        }
    }
}