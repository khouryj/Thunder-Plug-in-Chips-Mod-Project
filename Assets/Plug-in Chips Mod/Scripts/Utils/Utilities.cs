using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using RoR2;
using static PlugInChipsMod.PlugInChips;

namespace PlugInChipsMod.Scripts
{
    public class Utilities
    {
        public static ItemDef offensiveHeal;
        public static ItemDef deadlyHeal;
        public static ItemDef tauntUp;
        public static ItemDef antiChainDamage;
        public static ItemDef shockwave;
        public static ItemDef osChip;
        public static ItemDef IronPipe;
        public static ItemDef VirtuousContract;

        public static BuffDef SuperTaunt;
        public static BuffDef SuperDeadlyHeal;
        public static BuffDef SuperOffensiveHeal;
        public static BuffDef SuperAntiChain;
        public static BuffDef SuperShockwave;
        public static BuffDef cooldown;
        public static BuffDef Taunted;

        public static void Init()
        {
            offensiveHeal = serializeableContentPack.itemDefs[2];
            deadlyHeal = serializeableContentPack.itemDefs[1];
            tauntUp = serializeableContentPack.itemDefs[4];
            antiChainDamage = serializeableContentPack.itemDefs[0];
            shockwave = serializeableContentPack.itemDefs[3];
            osChip = serializeableContentPack.itemDefs[5];
            IronPipe = serializeableContentPack.itemDefs[6];
            VirtuousContract = serializeableContentPack.itemDefs[7];

            Taunted = serializeableContentPack.buffDefs[2];
            SuperTaunt = serializeableContentPack.buffDefs[5];
            SuperDeadlyHeal = serializeableContentPack.buffDefs[6];
            SuperOffensiveHeal = serializeableContentPack.buffDefs[7];
            SuperAntiChain = serializeableContentPack.buffDefs[8];
            SuperShockwave = serializeableContentPack.buffDefs[9];
            cooldown = serializeableContentPack.buffDefs[10];
        }

        public static void InitializeCorruptedItem(ItemDef.Pair[] pairs)
        {
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pairs);
        }
    }
}
