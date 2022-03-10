using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using static PlugInChipsMod.PlugInChips;

namespace PlugInChipsMod.Scripts
{
    public class Buffs
    {
        public static void Init()
        {
            serializeableContentPack.buffDefs[0].name = "Even if Our Words Seem Meaningless";
            serializeableContentPack.buffDefs[1].name = "Possessed by a Disease";
            serializeableContentPack.buffDefs[2].name = "Taunted";
            serializeableContentPack.buffDefs[3].name = "Weakened";
            serializeableContentPack.buffDefs[4].name = "antiChainCooldown";
        }
    }
}
