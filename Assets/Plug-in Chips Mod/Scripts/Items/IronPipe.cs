using BepInEx.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace PlugInChipsMod.Scripts
{
    public class IronPipe : CustomItem<IronPipe>
    {
        public override string Name => "Nier's Iron Pipe";
        public override string Pickup => "Increases crit damage. Small chance to crit again.";
        public override string Desc => "Increases critical strike damage by 5%.<style=cStack> +5% per stack</style> Upon critically striking an enemy, you have a 5% chance to crit again. <style=cStack>+3% per stack</style>";
        public override string Lore => "We weren't alone when we went to that room. There was a whole group of people. All of a sudden the books starting turning them into those <style=cStack>things.</style>\n\n" +
                                       "I took her and we ran to a nearby abandoned convenience store in search of food. I can hear them coming after us. No matter what I need to protect her.\n\n" +
                                       "<style=cSub>I won't let anyone hurt Yonah.</style>\n\nThey arrive and I use the black book to take out a group of them. Iron pipe in hand, I say:\n" +
                                       "<style=cWorldEvent>Stay away from my sister!</style>";

        
        public override void Init(ConfigFile config)
        {
            itemDef = Utilities.IronPipe;

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            //This line is to avoid NREs and ungodly projectile explosions if the attacker does not have a valid characterbody, i.e. tar pot explosion/elite projectiles
            CharacterBody cb = damageInfo.attacker?.GetComponent<CharacterBody>() ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
            if (!damageInfo.attacker || !cb || !cb.inventory)
            {
                orig(self, damageInfo);
                return;
            }
            int count = cb.inventory.GetItemCount(itemDef);
            if (count <= 0)
            {
                orig(self, damageInfo);
                return;
            }
            if (damageInfo.crit)
            {
                damageInfo.damage *= 1.05f + (.05f * (count - 1));
                if (Util.CheckRoll(5f + (3f * (count - 1)), cb.master.luck, cb.master)) //finally learned to use checkroll
                {
                    damageInfo.damage *= 2;
                    damageInfo.damageColorIndex = DamageColorIndex.SuperBleed;
                }
            }
            orig(self, damageInfo);
        }
    }
}
