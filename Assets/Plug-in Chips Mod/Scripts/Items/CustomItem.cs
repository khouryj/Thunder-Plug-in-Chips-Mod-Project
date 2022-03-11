using BepInEx.Configuration;
using System;
using RoR2;
using R2API;
using UnityEngine;

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
    }
}