using RoR2;
using R2API;
using BepInEx.Configuration;
using static PlugInChipsMod.PlugInChips;
using System;

namespace PlugInChipsMod.Scripts
{
    public class LunarTear : CustomEquipment<LunarTear>
    {
        public override string Name => "Lunar Tear";
        public override string Pickup => "<style=cWorldEvent>It's like I'm carrying the weight of the world.</style>";
        public override string Desc => "<style=cWorldEvent>70% chance to spawn in a friendly doppelganger.</style> <style=cDeath>30% chance to lower stats.</style> <style=cIsUtility>Buffs allies when you are killed.</style>";
        public override string Lore => "The Lunar Tear - Legendary flower of almost perfect beauty.\nThis flower is said to be the opposite of the black flower.\nThe flower seems to have a will of its own, and wishes to assist its wielder.";
        
        public BuffDef Inspired;
        public BuffDef Weakened;

        public override void Init(ConfigFile config)
        {
            base.equipmentDef = serializeableContentPack.equipmentDefs[1];
            Inspired = serializeableContentPack.buffDefs[0];
            Weakened = serializeableContentPack.buffDefs[3];

            SetupLanguage();
            SetupHooks();
        }

        protected override void SetupHooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += CheckEquipment;
            On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += BuffAllies;
        }

        private void BuffAllies(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, DamageReport damageReport, NetworkUser victimNetworkUser)
        {
            if (damageReport.victim && damageReport.victim.body)
            {
                var playerbody = damageReport.victim.body;
                if (playerbody.equipmentSlot.equipmentIndex == base.equipmentDef.equipmentIndex)
                {

                    for (int index = CharacterMaster.readOnlyInstancesList.Count - 1; index >= 0; --index)
                    {
                        CharacterMaster readOnlyInstances = CharacterMaster.readOnlyInstancesList[index];
                        if (readOnlyInstances.teamIndex == TeamIndex.Player && (bool)(UnityEngine.Object)readOnlyInstances.playerCharacterMasterController)
                        {
                            CharacterBody body = readOnlyInstances.GetBody();
                            if (body)
                            {
                                body.baseDamage *= 1.20f;
                                body.baseMaxHealth *= 1.20f;
                                body.baseMoveSpeed *= 1.20f;
                                body.baseArmor += 50;
                                body.AddBuff(Inspired);
                            }
                        }
                    }
                }
            }
            orig(self, damageReport, victimNetworkUser);
        }

        private bool CheckEquipment(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == base.equipmentDef) { return UseEquipment(self); }
            return orig(self, equipmentDef);
        }

        private bool UseEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.inputBank) { return false; }
            else
            {
                double rnd = new System.Random().NextDouble();
                if (rnd <= .7)
                {
                    DoppelgangerAllySpawnCard spawnCard = DoppelgangerAllySpawnCard.FromMaster(slot.characterBody.master);
                    if (spawnCard)
                    {
                        SpawnCard.SpawnResult spawnResult = new SpawnCard.SpawnResult();
                        DirectorPlacementRule dpr = new DirectorPlacementRule();
                        DirectorSpawnRequest request = new DirectorSpawnRequest(spawnCard, dpr, new Xoroshiro128Plus((ulong)slot.characterBody.healthComponent.health));
                        request.teamIndexOverride = TeamIndex.Player;
                        request.summonerBodyObject = slot.characterBody.gameObject;
                        spawnCard.Spawn(slot.characterBody.corePosition, slot.characterBody.transform.localRotation, request, ref spawnResult);
                    }
                    return true;
                }
                else
                {
                    slot.characterBody.baseDamage -= (slot.characterBody.baseDamage * .1f);
                    slot.characterBody.baseMoveSpeed -= (slot.characterBody.baseMoveSpeed * .1f);
                    slot.characterBody.baseArmor -= 20f;
                    slot.characterBody.AddBuff(Weakened);
                    return true;
                }
            }
        }
    }
}