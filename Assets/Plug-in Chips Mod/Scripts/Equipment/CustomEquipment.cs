using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using RoR2;
using R2API;
using System.Reflection;
using PlugInChipsMod;
using UnityEngine;

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

        public virtual bool usetargeting { get; } = false;

        public EquipmentDef equipmentDef;

        public ConfigEntry<bool> enabled;

        public abstract void Init(ConfigFile config);

        protected virtual void SetupConfig(ConfigFile config) { }

        protected virtual void SetupLanguage()
        {
            LanguageAPI.Add(equipmentDef.nameToken, Name);
            LanguageAPI.Add(equipmentDef.pickupToken, Pickup);
            LanguageAPI.Add(equipmentDef.descriptionToken, Desc);
            LanguageAPI.Add(equipmentDef.loreToken, Lore);
        }

        protected virtual void SetupHooks()
        {
            On.RoR2.EquipmentSlot.Update += UpdateTargeting;
        }

        public static void InitializeEquipment()
        {
            var Equipment = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(CustomEquipment)));
            foreach (var equipment in Equipment)
            {
                CustomEquipment equip = (CustomEquipment)Activator.CreateInstance(equipment);
                Enabled(ref equip);
                if (!equip.enabled.Value) { continue; }
                equip.Init(PlugInChips.instance.Config);
            }
        }

        private static void Enabled(ref CustomEquipment customItem)
        {
            customItem.enabled = PlugInChips.instance.Config.Bind<bool>("Equipment: " + customItem.Name, "Enabled?", true, "Should this equipment be enabled?");
        }


        public GameObject TargetingIndicatorPrefabBase = null;
        //Based on MysticItem's targeting code.
        protected void UpdateTargeting(On.RoR2.EquipmentSlot.orig_Update orig, EquipmentSlot self)
        {
            orig(self);

            if (self.equipmentIndex == equipmentDef.equipmentIndex)
            {
                var targetingComponent = self.GetComponent<TargetingControllerComponent>();
                if (!targetingComponent)
                {
                    targetingComponent = self.gameObject.AddComponent<TargetingControllerComponent>();
                    targetingComponent.VisualizerPrefab = TargetingIndicatorPrefabBase;
                }

                if (self.stock > 0)
                {
                    targetingComponent.ConfigureTargetFinderForEnemies(self);
                }
                else
                {
                    targetingComponent.Invalidate();
                    targetingComponent.Indicator.active = false;
                }
            }
        }
    }

    //Totally not yoinked from KomradeSpectre. Clueless
    public class TargetingControllerComponent : MonoBehaviour
    {
        public GameObject TargetObject;
        public GameObject VisualizerPrefab;
        public Indicator Indicator;
        public BullseyeSearch TargetFinder;
        public Action<BullseyeSearch> AdditionalBullseyeFunctionality = (search) => { };

        public void Awake()
        {
            Indicator = new Indicator(gameObject, null);
        }

        public void OnDestroy()
        {
            Invalidate();
        }

        public void Invalidate()
        {
            TargetObject = null;
            Indicator.targetTransform = null;
        }

        public void ConfigureTargetFinderBase(EquipmentSlot self)
        {
            if (TargetFinder == null) TargetFinder = new BullseyeSearch();
            TargetFinder.teamMaskFilter = TeamMask.allButNeutral;
            TargetFinder.teamMaskFilter.RemoveTeam(self.characterBody.teamComponent.teamIndex);
            TargetFinder.sortMode = BullseyeSearch.SortMode.Angle;
            TargetFinder.filterByLoS = true;
            float num;
            Ray ray = CameraRigController.ModifyAimRayIfApplicable(self.GetAimRay(), self.gameObject, out num);
            TargetFinder.searchOrigin = ray.origin;
            TargetFinder.searchDirection = ray.direction;
            TargetFinder.maxAngleFilter = 10f;
            TargetFinder.viewer = self.characterBody;
        }

        public void ConfigureTargetFinderForEnemies(EquipmentSlot self)
        {
            ConfigureTargetFinderBase(self);
            TargetFinder.teamMaskFilter = TeamMask.GetUnprotectedTeams(self.characterBody.teamComponent.teamIndex);
            TargetFinder.RefreshCandidates();
            TargetFinder.FilterOutGameObject(self.gameObject);
            AdditionalBullseyeFunctionality(TargetFinder);
            PlaceTargetingIndicator(TargetFinder.GetResults());
        }

        public void ConfigureTargetFinderForFriendlies(EquipmentSlot self)
        {
            ConfigureTargetFinderBase(self);
            TargetFinder.teamMaskFilter = TeamMask.none;
            TargetFinder.teamMaskFilter.AddTeam(self.characterBody.teamComponent.teamIndex);
            TargetFinder.RefreshCandidates();
            TargetFinder.FilterOutGameObject(self.gameObject);
            AdditionalBullseyeFunctionality(TargetFinder);
            PlaceTargetingIndicator(TargetFinder.GetResults());

        }

        public void PlaceTargetingIndicator(IEnumerable<HurtBox> TargetFinderResults)
        {
            HurtBox hurtbox = TargetFinderResults.Any() ? TargetFinderResults.First() : null;

            if (hurtbox)
            {
                TargetObject = hurtbox.healthComponent.gameObject;
                Indicator.visualizerPrefab = VisualizerPrefab;
                Indicator.targetTransform = hurtbox.transform;
            }
            else
            {
                Invalidate();
            }
            Indicator.active = hurtbox;
        }
    }
}
