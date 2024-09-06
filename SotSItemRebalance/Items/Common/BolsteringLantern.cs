using System;
using System.Collections.Generic;
using Ror2;
using R2API;
using UnityEngine.AddressableAssets;
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using IL.RoR2;
using RoR2;


// VANILLA: 20% damage boost when health is below 50%
// TODO: 20% movement speed boost when health is below 50%
// BL called LowerHealthHigherDamage

namespace SotSItemRebalance.Items
{
    public class BolsteringLantern
    {
        internal static bool Enable = true;
        internal static float SpeedBuff = 0.2f;

        public BolsteringLantern()
        {
            if (!Enable) { return; }
            Main.logSource.LogInfo("Changing Bolstering Lantern");
            //ClampConfig();
            U//pdateText();
            //UpdateItemDef();
            //Hooks();
        }

        private void ClampConfig()
        {
            SpeedBuff = Math.Max(0f, SpeedBuff);
        }

        private void UpdateText()
        {
            Main.logSource("Updating Bolstering Lantern Item Text");
            // TODO: change pickup text to match in-game pickup text
            string pickup = "Slightly increases movement speed when below half health";
            string desc = "When below <style=cIsHealth>50%</style> health receive a movement speed increase of <style=cIsUtility>20%</style> <style=cStack>(+20% movement speed increase per item stack)</style>";

            LanguageAPI.Add("ITEM_LOWERHEALTHHIGHERDAMAGE", pickup + ".");
            LanguageAPI.Add("ITEM_LOWERHEALTHHIGHERDAMAGE", desc + ".");
        }

        private void UpdateItemDef()
        {
            // load original asset from game
            ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>("RoR2/Items/LowerHealthHigherDamage/LowerHealthHigherDamage.asset").WaitForCompletion();
            if (itemDef)
            {
                List<ItemTag> itemTags = itemDef.tags.ToList();
                // enemies can have this item
                itemTags.Remove(ItemTag.AIBlacklist);
                // Mythrix can have this item
                itemDef.Remove(ItemTag.BrotherBlacklist);
                itemDef.tags = itemTags.ToArray();
            }
        }

        private void Hooks()
        {
            Main.logSource.LogInfo("Applying IL Modifications to Bolstering Lantern");
            IL.RoR2.CharacterBody.RecalculateStats += new ILContext.Manipulator(IL_RecalculateStats);
            SharedHooks.Handle_GetStatCoefficients_Actions += GetStatCoefficients;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args, Inventory inventory)
        {
            if (sender.inventory)
            {
                // get number of stacks and apply buffs accordingly
                int lanternCount = sender.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage);
                float preSpeed = sender.moveSpeed;
                if (lanternCount > 0)
                {
                    args.baseMoveSpeedAdd += lanternCount * SpeedBuff * preSpeed;
                }
            }
        }

        private void IL_RecalculateStats(ILContext il)
        {
            // ...
            // in RoR2.CharacterBody.RecalculateStats
            // num52 = inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage);
            //...
            // if (HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff)) ... damage += baseDamage * (float)CurrentHealthLevel * 0.1f * (float)num52 * 0.5f
            ILCursor ilcursor = new ILCursor(il);

        }
    }
}