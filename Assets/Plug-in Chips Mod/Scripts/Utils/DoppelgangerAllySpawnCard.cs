using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Navigation;
using System;

namespace PlugInChipsMod.Scripts
{
    //Used to create doppelganger spawns for lunar tear equipment
    //This is simply RoR2.Artifacts.DoppelgangerSpawnCard but without making the player team a target in the prespawnsetupcallback method and I gave them damageboost items
    public class DoppelgangerAllySpawnCard : CharacterSpawnCard
    {
        public static DoppelgangerAllySpawnCard doppelgangerAllySpawnCard;
        private CharacterMaster cm;
        private Inventory inventory;
        private bool effectOff;

        public static DoppelgangerAllySpawnCard FromMaster(CharacterMaster characterMaster, bool effectOff = true)
        {
            if (!characterMaster) { return null; }
            CharacterBody cb = characterMaster.GetBody();
            if (!cb) { return null; }

            doppelgangerAllySpawnCard = ScriptableObject.CreateInstance<DoppelgangerAllySpawnCard>();
            doppelgangerAllySpawnCard.hullSize = cb.hullClassification;
            doppelgangerAllySpawnCard.nodeGraphType = (cb.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground);
            doppelgangerAllySpawnCard.prefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(cb.bodyIndex));
            doppelgangerAllySpawnCard.sendOverNetwork = true;
            doppelgangerAllySpawnCard.runtimeLoadout = new Loadout();
            doppelgangerAllySpawnCard.cm = characterMaster;
            doppelgangerAllySpawnCard.cm.loadout.Copy(doppelgangerAllySpawnCard.runtimeLoadout);
            doppelgangerAllySpawnCard.inventory = characterMaster.inventory;
            doppelgangerAllySpawnCard.effectOff = effectOff;

            return doppelgangerAllySpawnCard;
        }

        public override Loadout GetRuntimeLoadout()
        {
            return this.cm.loadout;
        }

        public override Action<CharacterMaster> GetPreSpawnSetupCallback()
        {
            Action<CharacterMaster> baseCallback = base.GetPreSpawnSetupCallback();
            return (Action<CharacterMaster>)(characterMaster =>
            {
                characterMaster.inventory.CopyItemsFrom(this.inventory, (Func<ItemIndex, bool>)(_ => true));
                Action<CharacterMaster> action = baseCallback;
                if (action != null)
                {
                    action(characterMaster);
                }
                if (!doppelgangerAllySpawnCard.effectOff)
                {
                    characterMaster.inventory.GiveItem(RoR2Content.Items.InvadingDoppelganger);
                    characterMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, 250);
                }
                else
                {
                    characterMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, 50);
                }
            });
        }
    }
}
