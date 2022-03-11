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

[assembly: SearchableAttribute.OptIn]
namespace PlugInChipsMod
{
    [BepInPlugin(MODGUID, MODNAME, MODVERSION)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI))]
    public class PlugInChips : BaseUnityPlugin
    {
        public const string MODNAME = "Plug-In Chips Mod";
        public const string MODVERSION = "1.0.0";
        public const string MODGUID = "com.RumblingJOSEPH.PlugInChipsMod";

        public static AssetBundle assetBundle;
        public static SerializableContentPack serializeableContentPack;
        private static ContentPack contentPack;

        private bool load = true;
        public bool contentpackLoaded = false;
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
            if (!load) { Logger.LogMessage("Failed to load in assetbundle, check file name/path."); }

            Buffs.Init();

            SearchableAttribute.ScanAssembly(Assembly.GetExecutingAssembly());
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
            Logger.LogMessage("Grabbing Content Pack");
            serializeableContentPack = assetBundle.LoadAsset<SerializableContentPack>("PlugInChipsContentPack");
            contentPack = serializeableContentPack.CreateContentPack();
            ContentPackProvider.contentPack = contentPack;
        }

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
