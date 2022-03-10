using RoR2;
using RoR2.Artifacts;
using BepInEx.Configuration;
using static PlugInChipsMod.PlugInChips;
using R2API;
using System;

namespace PlugInChipsMod.Scripts
{
    public class TheBlackFlower : CustomEquipment<TheBlackFlower>
    {
        public override string Name => "The Black Flower";
        public override string Pickup => "Chance to increase stats on use.Chance to spawn in doppelganger on use.Spawns doppelganger when you are killed.";
        public override string Desc => "60% chance of increasing <style=cIsDamage>base damage by 10%</style>, <style=cIsUtility>armor by 20, and movement speed by 10%.</style> for the rest of the stage. <style=cWorldEvent>40% chance of spawning in a doppelganger.</style> <style=cDeath>Dying with this equipment spawns a doppelganger.</style>";
        public override string Lore => "The Black Flower, created by the gods to destroy the world as we know it.\nIt is said that the flower slowly corrupts its wielder, causing their eyes to turn red and go crazy trying to destroy humanity.\nBe on the lookout for copies of yourself, they are sure to cause the world's destruction.\n<style=cIsHealth>You never know when the red eye disease will spread to you.</style>";

        private EquipmentDef blackFlower;
        public BuffDef Possessed;

        public override void Init(ConfigFile config)
        {
            blackFlower = serializeableContentPack.equipmentDefs[0];
            Possessed = serializeableContentPack.buffDefs[1];

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupLanguage()
        {
            LanguageAPI.Add(blackFlower.nameToken, Name);
            LanguageAPI.Add(blackFlower.pickupToken, Pickup);
            LanguageAPI.Add(blackFlower.descriptionToken, Desc);
            LanguageAPI.Add(blackFlower.loreToken, Lore);
        }

        protected override void SetupHooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += CheckEquipment;
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += SpawnDoppelganger;
        }

        private void SpawnDoppelganger(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, DamageReport damageReport, NetworkUser victimNetworkUser)
        {
            if (damageReport.victim)
            {

                var slot = damageReport.victim.body;
                if (slot?.equipmentSlot.equipmentIndex == blackFlower.equipmentIndex)
                {
                    ulong ul = (ulong)new System.Random().Next();
                    DoppelgangerInvasionManager.CreateDoppelganger(slot.master, new Xoroshiro128Plus(ul));
                }
            }
            orig(self, damageReport, victimNetworkUser);
        }

        private bool CheckEquipment(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == blackFlower) { return UseEquipment(self); }
            return orig(self, equipmentDef);
        }

        private bool UseEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.inputBank) { return false; }
            else
            {
                double rnd = new System.Random().NextDouble();
                if (rnd <= .4)
                {
                    ulong ul = (ulong)new System.Random().Next();
                    DoppelgangerInvasionManager.CreateDoppelganger(slot.characterBody.master, new Xoroshiro128Plus(ul));
                    return true;
                }
                else
                {
                    slot.characterBody.baseDamage *= 1.10f;
                    slot.characterBody.baseMoveSpeed *= 1.10f;
                    slot.characterBody.armor += 20;
                    slot.characterBody.AddBuff(Possessed);
                    return true;
                }
            }
        }
    }
}
