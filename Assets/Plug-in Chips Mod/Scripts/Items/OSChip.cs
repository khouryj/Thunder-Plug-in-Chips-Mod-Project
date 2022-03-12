using BepInEx.Configuration;
using System.Collections;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PlugInChipsMod.Scripts
{
    public class OSChip : CustomItem<OSChip>
    {
        public override string Name => "OS Chip";
        public override string Pickup => "The combined power of all plug-in chips. <style=cIsVoid>Corrupts all plug-in chips.</style>";
        public override string Desc => "test";
        public override string Lore => "The core chip of any unit.\n <style=cisHealth>Note: Removal of this chip means total malfunction of unit. Do not remove under any circumstances.</style>";
        
        private ItemDef.Pair[] conversions;
        public override bool dlcRequired => true;

        public override void Init(ConfigFile config)
        {
            base.itemDef = Utilities.osChip;
            conversions = new ItemDef.Pair[5];

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupHooks()
        {
            
        }

        protected override void ConsumedItems()
        {
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

            List<ItemDef.Pair> list1 = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].ToList();
            List<ItemDef.Pair> list2 = conversions.ToList();
            list1.AddRange(list2);

            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = list1.ToArray();
        }
    }
}