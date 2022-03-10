using BepInEx.Configuration;
using System;
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


        public abstract void Init(ConfigFile config);

        protected virtual void SetupConfig(ConfigFile config) { }

        protected abstract void SetupLanguage();

        protected virtual void SetupHooks() { }
    }
}