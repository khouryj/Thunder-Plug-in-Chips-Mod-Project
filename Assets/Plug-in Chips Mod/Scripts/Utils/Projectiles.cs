using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlugInChipsMod.PlugInChips;
using RoR2;
using R2API;
using UnityEngine.Networking;
using RoR2.Projectile;

namespace PlugInChipsMod.Scripts
{
    public class Projectiles
    {
        public static GameObject shockwaveProjectile;
        private static GameObject model;

        public static void Init()
        {
            shockwaveProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/FMJ"), "ShockwaveProjectile", true);
            model = assetBundle.LoadAsset<GameObject>("shockwaveProjectile.prefab");

            model.AddComponent<NetworkIdentity>();
            model.AddComponent<ProjectileGhostController>();

            var projectileController = shockwaveProjectile.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = model;

            PrefabAPI.RegisterNetworkPrefab(shockwaveProjectile);
            ContentAddition.AddProjectile(shockwaveProjectile);
        }
    }
}
