using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using BepInEx;
using R2API.Utils;
using R2API;
using System.Reflection;
using System;
using System.Linq;
using PlugInChipsMod.Scripts;
using ChipsItem = PlugInChipsMod.Scripts.CustomItem;
using ChipsEquipment = PlugInChipsMod.Scripts.CustomEquipment;
using Path = System.IO.Path;
using RoR2;
using SearchableAttribute = HG.Reflection.SearchableAttribute;
using BepInEx.Configuration;

[assembly: SearchableAttribute.OptIn]
namespace PlugInChipsMod
{
    [BepInPlugin(MODGUID, MODNAME, MODVERSION)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(PrefabAPI))]
    public class PlugInChips : BaseUnityPlugin
    {
        /*
        Big thanks to KomradeSpectre for the item creation tutorial, I would be lost without it.
        Also for the ItemBase and EquipmentBase classes as I pretty much used those for the CustomItem and CustomEquipment abstracts as well as the targeting component
        Thanks to bubbet for the help too with this project and for the code on custom void items
        Thanks to the modding community in general for being a big help.
        */
        public const string MODNAME = "Plug-In Chips Mod";
        public const string MODVERSION = "1.1.0";
        public const string MODGUID = "com.RumblingJOSEPH.PlugInChipsMod";
        public const string PREFIX = "PLUGINCHIPS_";

        public static AssetBundle assetBundle = null;
        public static SerializableContentPack serializeableContentPack = null;
        private static ContentPack contentPack = null;

        private bool load = true;
        public static PlugInChips instance;

        public static Dictionary<string, string> ShaderLookup = new Dictionary<string, string>()
        {
            {"stubbed hopoo games/deferred/standard", "shaders/deferred/hgstandard"},
            {"stubbed hopoo games/fx/cloud intersection remap", "shaders/fx/hgintersectioncloudremap" },
            {"stubbed hopoo games/fx/cloud remap", "shaders/fx/hgcloudremap" },
            {"stubbed hopoo games/fx/distortion", "shaders/fx/hgdistortion" },
            {"stubbed hopoo games/deferred/snow topped", "shaders/deferred/hgsnowtopped" },
            {"stubbed hopoo games/fx/solid parallax", "shaders/fx/hgsolidparallax" }
        };

        public BepInEx.Logging.ManualLogSource Logger;

        private void Awake()
        {
            instance = this;
            Logger = base.Logger;

            LoadAssetBundle();
            if (!load) { Logger.LogMessage("Failed to load in assetbundle, check file name/path."); return; }

            Projectiles.Init();
            Buffs.Init();
            PlugInChipsMod.Scripts.Utilities.Init();
            AddDeveloperPrefix();

            ContentPackProvider.Init(); //i hecking love content packs
            ChipsItem.InitializeItems();
            ChipsEquipment.InitializeEquipment();
        }

        [SystemInitializer]
        public static void SetupLanguage()
        {
            foreach(ChipsItem item in ChipsItem.items)
            {
                Language.english.SetStringByToken(item.itemDef.nameToken, item.Name);
                Language.english.SetStringByToken(item.itemDef.pickupToken, item.Pickup);
                Language.english.SetStringByToken(item.itemDef.descriptionToken, item.Desc);
                Language.english.SetStringByToken(item.itemDef.loreToken, item.Lore);
            }
            foreach(ChipsEquipment equip in ChipsEquipment.equipment)
            {
                Language.english.SetStringByToken(equip.equipmentDef.nameToken, equip.Name);
                Language.english.SetStringByToken(equip.equipmentDef.pickupToken, equip.Pickup);
                Language.english.SetStringByToken(equip.equipmentDef.descriptionToken, equip.Desc);
                Language.english.SetStringByToken(equip.equipmentDef.loreToken, equip.Lore);
            }
        }

        //on the off chance someone makes an item with the exact same token
        private void AddDeveloperPrefix()
        {
            foreach(ItemDef item in serializeableContentPack.itemDefs)
            {
                item.nameToken = PREFIX + item.nameToken;
                item.pickupToken = PREFIX + item.pickupToken;
                item.descriptionToken = PREFIX + item.descriptionToken;
                item.loreToken = PREFIX + item.loreToken;
            }
            foreach(EquipmentDef equipmentDef in serializeableContentPack.equipmentDefs)
            {
                equipmentDef.nameToken = PREFIX + equipmentDef.nameToken;
                equipmentDef.pickupToken = PREFIX + equipmentDef.pickupToken;
                equipmentDef.descriptionToken = PREFIX + equipmentDef.descriptionToken;
                equipmentDef.loreToken = PREFIX + equipmentDef.loreToken;
            }
        }

        private void LoadAssetBundle()
        {
            if (assetBundle == null)
            {
                string dir = Path.GetDirectoryName(Info.Location);
                assetBundle = AssetBundle.LoadFromFile(Path.Combine(dir, "pluginchipassets"));
            }
            load = assetBundle;
            if (!load) { return; }
            ShaderConversion(assetBundle);
            GrabContentPack();
        }

        private void GrabContentPack()
        {
            //Logger.LogMessage("Grabbing Content Pack");
            serializeableContentPack = assetBundle.LoadAsset<SerializableContentPack>("PlugInChipsContentPack");
            contentPack = serializeableContentPack.CreateContentPack();
            ContentPackProvider.contentPack = contentPack;
        }
        //From komradesprectre
        private static void ShaderConversion(AssetBundle assets)
        {
            var materialAssets = assets.LoadAllAssets<Material>().Where(material => material.shader.name.StartsWith("Stubbed"));

            foreach (Material material in materialAssets)
            {
                var replacementShader = LegacyResourcesAPI.Load<Shader>(ShaderLookup[material.shader.name.ToLowerInvariant()]);
                if (replacementShader) { material.shader = replacementShader; }
            }
        }
    }

    public class ContentPackProvider : IContentPackProvider
    {
        public string identifier => PlugInChips.MODNAME;
        public static ContentPack contentPack;
        public static string ContentPackName = "PlugInChipsContentPack";

        internal static void Init()
        {
            ContentManager.collectContentPackProviders += AddContent;
        }

        private static void AddContent(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentPackProvider());
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
