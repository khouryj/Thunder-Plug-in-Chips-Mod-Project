using BepInEx.Configuration;
using System;
using RoR2;
using R2API;
using UnityEngine;
using System.Reflection;
using System.Linq;
using PlugInChipsMod;
using RoR2.ExpansionManagement;
using RoR2.Items;
using System.Collections.Generic;

namespace PlugInChipsMod.Scripts
{
    public abstract class CustomItem<T> : CustomItem where T : CustomItem<T>
    {
        public static T instance { get; private set; }
        public CustomItem()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting CustomItem was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class CustomItem
    {
        public abstract string Name { get; }
        public abstract string Pickup { get; }
        public abstract string Desc { get; }
        public abstract string Lore { get; }

        public virtual bool dlcRequired { get; } = false;
        
        public ConfigEntry<bool> enabled;
        public ConfigEntry<bool> aiBlacklist;

        public ItemDef itemDef;
        public static List<CustomItem> items = new List<CustomItem>();

        public abstract void Init(ConfigFile config);

        protected virtual void SetupConfig(ConfigFile config) { }

        protected virtual void SetupLanguage()
        {
            //Unused
        }

        protected virtual void SetupHooks() { }

        public static void InitializeItems()
        {
            var Items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(CustomItem)));
            foreach (var item in Items)
            {
                CustomItem chipsItem = (CustomItem)Activator.CreateInstance(item);
                EnableAndBlacklist(ref chipsItem);
                if(!chipsItem.enabled.Value) { continue; }
                if (chipsItem.aiBlacklist.Value) { chipsItem.itemDef.tags = chipsItem.itemDef.tags.AddRangeToArray(new ItemTag[]{ItemTag.AIBlacklist}); }
                //PlugInChips.instance.Logger.LogMessage("Initializing Items...");
                
                chipsItem.Init(PlugInChips.instance.Config);
                items.Add(chipsItem);

                //From bubbet's itembase
                if (chipsItem.dlcRequired)
                {
                    chipsItem.itemDef.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");
                }
            }
        }

        private static void EnableAndBlacklist(ref CustomItem customItem)
        {
            string Name = customItem.Name == "Nier's Iron Pipe" ? "Iron Pipe" : customItem.Name;
            customItem.enabled = PlugInChips.instance.Config.Bind<bool>("Item: " + Name, "Enabled?", true, "Should this item be enabled?");
            customItem.aiBlacklist = PlugInChips.instance.Config.Bind<bool>("Item: " + Name, "AI Blacklist", false, "Should this item be restricted from AI in runs?");
        }
    }
}