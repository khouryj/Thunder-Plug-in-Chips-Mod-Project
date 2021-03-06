using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using R2API;
using System.Linq;

namespace PlugInChipsMod.Scripts
{
    public class YorhaVisor : CustomEquipment<YorhaVisor>
    {
        public override string Name => "Yorha Visor";
        public override string Pickup => "Hack into your enemies and take control of them.";
        public override string Desc => "<style=cIsUtility>Hack an enemy</style>, becoming one of them for a time. Reaching <style=cDeath>critical health</style> in this state will respawn you as you originally were. Note: Can only hack one enemy at a time.";
        public override string Lore => "\"It doesn't matter, none of this matters! But if nothing matters, why do I long for humans like this? Why do I desire the touch of something that no longer exists?\" -9S";
        //Signifies to the base that I want to use targeting
        public override bool usetargeting => true;

        public override void Init(ConfigFile config)
        {
            equipmentDef = Utilities.YorhaVisor;

            if (usetargeting)
                SetupTargeting();

            SetupLanguage();
            SetupHooks();
        }
        //Sets up the targeting indicator for the equipment, this all comes from komradespectre's boilerplate
        private void SetupTargeting()
        {
            TargetingIndicatorPrefabBase = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/BossHunterIndicator"), "YorhaVisorIndicator", false);
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            TargetingIndicatorPrefabBase.GetComponentInChildren<SpriteRenderer>().transform.rotation = Quaternion.identity;
            TargetingIndicatorPrefabBase.GetComponentInChildren<TMPro.TextMeshPro>().color = new Color(0.423f, 1, 0.749f);
        }

        protected override void SetupHooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += CheckEquipment;
            On.RoR2.HealthComponent.TakeDamage += SwitchBack;
            base.SetupHooks();
        }
        //Swaps back to original character on low health, but does not save you from death
        private void SwitchBack(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (!self) { return; }
            if (self.health <= 0) { return; }
            if (self.isHealthLow)
            {
                var component = self.body.master.gameObject.GetComponent<YorhaVisorComponent>();
                if (component && component.transformed) { component.RevertCharacter(); }
            }
        }

        private bool CheckEquipment(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, RoR2.EquipmentDef equipmentDef)
        {
            if (equipmentDef == this.equipmentDef) 
            { 
                return UseEquipment(self); 
            }
            return orig(self, equipmentDef);
        }

        private bool UseEquipment(EquipmentSlot slot)
        {
            if (!slot.characterBody || !slot.characterBody.inputBank) { return false; }
            if (!slot.characterBody.master) { return false; }
            var component = slot.characterBody.master.gameObject.GetComponent<YorhaVisorComponent>();
            if (!component)
            {
                component = slot.characterBody.master.gameObject.AddComponent<YorhaVisorComponent>();
            }
            if (slot.stock <= 0 || !component) { return false; }
            if (component.transformed)
            {
                component.RevertCharacter();
                Utilities.YorhaVisor.cooldown = 130f;
                return true;
            }

            var targetcomponent = slot.GetComponent<TargetingControllerComponent>();
            if (!targetcomponent || !targetcomponent.TargetObject) { return false; }
            var chosenhurtbox = targetcomponent.TargetFinder.GetResults().First();
            if (!chosenhurtbox) { return false; }
            
            component.SwapCharacters(chosenhurtbox);
            Utilities.YorhaVisor.cooldown = 5f;
            return true;
        }
    }
    //Component that controls the visor behavior
    public class YorhaVisorComponent : MonoBehaviour
    {
        private CharacterMaster characterMaster;
        private CharacterBody origCharacterBody;
        private string origName;
        public bool transformed;
        //Initializes variables on component add to avoid NREs in hooks
        public void Awake()
        {
            transformed = false;
            characterMaster = this.gameObject.GetComponent<CharacterMaster>();
            origCharacterBody = characterMaster.GetBody();
            origName = BodyCatalog.GetBodyName(origCharacterBody.bodyIndex);
        }
        //Swaps character body with provided hurtbox
        public void SwapCharacters(HurtBox hurtBox)
        {
            origCharacterBody = characterMaster.GetBody();
            if (!characterMaster || !origCharacterBody) { return; }

            if (hurtBox)
            {
                characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(BodyCatalog.GetBodyName(hurtBox.healthComponent.body.bodyIndex));
                transformed = true;
                characterMaster.Respawn(origCharacterBody.transform.position, origCharacterBody.transform.rotation);
                var component = characterMaster.gameObject.GetComponent<OSChipComponent>();
                if (component)
                {
                    component.GetCurrentBuffs();
                    component.HandleBuffs(false);
                }
            }
        }
        //Reverts back to first characterbody when called
        public void RevertCharacter()
        {
            this.transformed = false;
            characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(origName);
            characterMaster.Respawn(characterMaster.GetBody().transform.position, characterMaster.GetBody().transform.rotation);
            var component = characterMaster.gameObject.GetComponent<OSChipComponent>();
            if (component)
            {
                component.GetCurrentBuffs();
                component.HandleBuffs(false);
            }
        }
    }
}