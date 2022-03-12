using BepInEx.Configuration;
using System.Collections;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static PlugInChipsMod.Scripts.Utilities;
using static PlugInChipsMod.PlugInChips;
using HarmonyLib;
using RoR2.Items;
using System;

namespace PlugInChipsMod.Scripts
{
    [HarmonyPatch]
    public class OSChip : CustomItem<OSChip>
    {
        //Array of Buffdefs, use cooldowns for timing
        //Dont forget to reset everything on stage change if needed (think of logic!)
        //Stage change might actually be covered here

        public override string Name => "OS Chip";
        public override string Pickup => "The combined power of all plug-in chips. <style=cIsVoid>Corrupts all plug-in chips.</style>";
        public override string Desc => "test";
        public override string Lore => "The core chip of any unit.\n <style=cisHealth>Note: Removal of this chip means total malfunction of unit. Do not remove under any circumstances.</style>";
        public override bool dlcRequired => true;

        private static ItemDef.Pair[] conversions;
        private static Harmony harmony = PlugInChips.harmony;

        private static BuffDef ActiveBuff1, ActiveBuff2;
        private static BuffDef[] buffs;
        private static CharacterBody cb;
        private static System.Random rnd = new System.Random();
        private static float cooldownTime, buffTime;

        private static int prevItemCount;
        private static int ItemCount;
        private static DamageInfo di = new DamageInfo()
        {
            damageColorIndex = DamageColorIndex.SuperBleed,
            crit = false,
            rejected = false,
            damageType = DamageType.Generic
        };

        public override void Init(ConfigFile config)
        {
            itemDef = Utilities.osChip;
            ItemCount = 0;
            prevItemCount = 0;
            ActiveBuff1 = null;
            ActiveBuff2 = null;
            cb = null;
            buffs = new BuffDef[] { SuperAntiChain, SuperDeadlyHeal, SuperOffensiveHeal, SuperShockwave, SuperTaunt };
            
            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupHooks()
        {
            Inventory.onInventoryChangedGlobal += new Action<Inventory>(Removal);
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private void RoR2Application_onFixedUpdate()
        {
            if (!cb) { return; }
            
            if (ActiveBuff1 != null && ActiveBuff2 != null && !cb.HasBuff(cooldown) && !cb.HasBuff(ActiveBuff1) && !cb.HasBuff(ActiveBuff2))
            {
                cb.AddTimedBuff(cooldown, cooldownTime);
                ActiveBuff1 = null;
                ActiveBuff2 = null;
                return;
            }
            else if (!cb.HasBuff(cooldown) && ActiveBuff1 == null && ActiveBuff2 == null)
            {
                GetCurrentBuffs(ActiveBuff1, ActiveBuff2);
                cb.AddTimedBuff(ActiveBuff1, buffTime);
                cb.AddTimedBuff(ActiveBuff2, buffTime);
                return;
            }
        }

        private void Removal(Inventory obj)
        {
            if (obj)
            { 
                ItemCount = obj.GetItemCount(Utilities.osChip);
                if (ItemCount == prevItemCount) { return; }
                if (ItemCount > prevItemCount)
                {
                    prevItemCount++;
                    if (cb == null) { cb = obj.GetComponentInParent<CharacterBody>(); }
                }
                else if (ItemCount < prevItemCount)
                {
                    try
                    {
                        HealthComponent hc = obj.GetComponentInParent<HealthComponent>();
                        di.damage = hc.health * (new System.Random().Next(1, 45) / 100);
                        if (hc.body.teamComponent.teamIndex != TeamIndex.Monster)
                        {
                            Chat.AddMessage("<style=cisHealth>WARNING: REMOVAL OF THIS CHIP HAS DAMAGED UNIT, SEEK MAINTENANCE IMMEDIATELY");
                            hc.TakeDamage(di);
                        }
                    }
                    catch (Exception)
                    {
                        PlugInChips.instance.Logger.LogWarning("Gameobject of inventory doesnt have a healthcomponent");
                        return;
                    }
                }
            }
        }

        private static void GetCurrentBuffs(BuffDef one, BuffDef two)
        {
            one = buffs[rnd.Next(0, 4)];
            two = buffs[rnd.Next(0, 4)];
            while (two == one) { two = buffs[rnd.Next(0, 4)]; }
        }


        //Bubbet is the only one i have seen make a void item so far so this code is "borrowed" from him
        [HarmonyPrefix, HarmonyPatch(typeof(ContagiousItemManager), nameof(ContagiousItemManager.Init))]
        public static void CorruptedItems()
        {
            conversions = new ItemDef.Pair[5];
            conversions[0] = new ItemDef.Pair
            {
                itemDef1 = Utilities.deadlyHeal,
                itemDef2 = Utilities.osChip
            };
            conversions[1] = new ItemDef.Pair
            {
                itemDef1 = Utilities.shockwave,
                itemDef2 = Utilities.osChip
            };
            conversions[2] = new ItemDef.Pair
            {
                itemDef1 = Utilities.antiChainDamage,
                itemDef2 = Utilities.osChip
            };
            conversions[3] = new ItemDef.Pair
            {
                itemDef1 = Utilities.offensiveHeal,
                itemDef2 = Utilities.osChip
            };
            conversions[4] = new ItemDef.Pair
            {
                itemDef1 = Utilities.tauntUp,
                itemDef2 = Utilities.osChip
            };

            Utilities.InitializeCorruptedItem(conversions);
        }
    }
}