using BepInEx.Configuration;
using System;
using RoR2;
using R2API;
using UnityEngine;
using System.Reflection;
using System.Linq;
using PlugInChipsMod;
using RoR2.ExpansionManagement;

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

        public ItemDef itemDef;

        public abstract void Init(ConfigFile config);

        protected virtual void SetupConfig(ConfigFile config) { }

        protected virtual void SetupLanguage()
        {
            LanguageAPI.Add(itemDef.nameToken, Name);
            LanguageAPI.Add(itemDef.pickupToken, Pickup);
            LanguageAPI.Add(itemDef.descriptionToken, Desc);
            LanguageAPI.Add(itemDef.loreToken, Lore);
        }

        protected virtual void SetupHooks() { }

        [SystemInitializer(typeof(ItemCatalog))]
        public static void InitializeItems()
        {
            var Items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(CustomItem)));
            foreach (var item in Items)
            {
                CustomItem chipsItem = (CustomItem)Activator.CreateInstance(item);
                PlugInChips.instance.Logger.LogMessage("Initializing Items...");
                chipsItem.Init(PlugInChips.instance.Config);

                //From bubbet's itembase
                if (chipsItem.dlcRequired)
                {
                    chipsItem.itemDef.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");
                    chipsItem.ConsumedItems();
                }
            }
        }

        protected virtual void ConsumedItems() {}
    }
}