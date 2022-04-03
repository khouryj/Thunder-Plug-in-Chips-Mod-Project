using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoR2;
using static PlugInChipsMod.PlugInChips;

namespace PlugInChipsMod.Scripts
{
    public static class Utilities
    {
        public static ItemDef offensiveHeal;
        public static ItemDef deadlyHeal;
        public static ItemDef tauntUp;
        public static ItemDef antiChainDamage;
        public static ItemDef shockwave;
        public static ItemDef osChip;
        public static ItemDef IronPipe;
        public static ItemDef VirtuousContract;
        public static ItemDef VirtuousTreaty;

        public static EquipmentDef YorhaVisor;

        public static BuffDef SuperTaunt;
        public static BuffDef SuperDeadlyHeal;
        public static BuffDef SuperOffensiveHeal;
        public static BuffDef SuperAntiChain;
        public static BuffDef SuperShockwave;
        public static BuffDef cooldown;
        public static BuffDef Taunted;
        public static BuffDef LogicVirus;

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
            VirtuousTreaty = serializeableContentPack.itemDefs[8];

            YorhaVisor = serializeableContentPack.equipmentDefs[2];

            Taunted = serializeableContentPack.buffDefs[2];
            SuperTaunt = serializeableContentPack.buffDefs[5];
            SuperDeadlyHeal = serializeableContentPack.buffDefs[6];
            SuperOffensiveHeal = serializeableContentPack.buffDefs[7];
            SuperAntiChain = serializeableContentPack.buffDefs[8];
            SuperShockwave = serializeableContentPack.buffDefs[9];
            cooldown = serializeableContentPack.buffDefs[10];
            LogicVirus = serializeableContentPack.buffDefs[11];
        }
        //Borrowed from harmony because it was breaking my build, this isnt mine at all I dont even understand this
        public static T[] AddRangeToArray<T>(this T[] sequence, T[] items) => (sequence ?? Enumerable.Empty<T>()).Concat<T>(items).ToArray<T>();

        public static void InitializeCorruptedItem(ItemDef.Pair[] pairs)
        {
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(pairs);

        }
        //Checks validity of a gameobject in preparation to acquire CharacterBody/use inventory
        public static bool CheckObject(GameObject gameObject)
        {
            if (!gameObject) { return false; }
            return gameObject.GetComponent<CharacterBody>() && gameObject.GetComponent<CharacterBody>().inventory;
        }
        //Returns characterbody of gameobject or null if not valid
        public static CharacterBody GetCharacterBody(this GameObject gameObject)
        {
            return CheckObject(gameObject) ? gameObject.GetComponent<CharacterBody>() : null;
        }

        public static float CalculateChange(float baseIncrease, float stackIncrease, int itemCount)
        {
            return baseIncrease + (stackIncrease * (itemCount-1));
        }
    }
}
