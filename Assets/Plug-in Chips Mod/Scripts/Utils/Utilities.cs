﻿using System.Collections;
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

        public static void Init()
        {
            offensiveHeal = serializeableContentPack.itemDefs[2];
            deadlyHeal = serializeableContentPack.itemDefs[1];
            tauntUp = serializeableContentPack.itemDefs[4];
            antiChainDamage = serializeableContentPack.itemDefs[0];
            shockwave = serializeableContentPack.itemDefs[3];
            osChip = serializeableContentPack.itemDefs[5];
            PlugInChips.instance.Logger.LogMessage(osChip.nameToken);
        }

        public static void InitializeCorruptedItem(ItemDef.Pair[] pairs)
        {
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pairs);
            PlugInChips.instance.Logger.LogMessage("Created corrupted item");
        }
    }
}
