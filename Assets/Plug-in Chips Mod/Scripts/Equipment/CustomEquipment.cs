using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace PlugInChipsMod.Scripts
{
    public abstract class CustomEquipment<T> : CustomEquipment where T : CustomEquipment<T>
    {
        public static T instance { get; private set; }
        public CustomEquipment()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting CustomEquipment was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class CustomEquipment
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
