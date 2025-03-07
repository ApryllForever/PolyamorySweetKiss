using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Quests;
using System;

namespace PolyamorySweetKiss
{
    public static class NPCPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }

        public static bool NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, GameLocation l, ref bool __result)
        {
            NPC fubar = new NPC();
            fubar = __instance;

            try
            {
                if (!Config.EnableMod || __instance.IsInvisible || __instance.isSleeping.Value || !who.canMove || who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(fubar)) || (who.pantsItem.Value?.ParentSheetIndex == 15 && (__instance.Name.Equals("Lewis") || __instance.Name.Equals("Marnie"))) || (__instance.Name.Equals("Krobus") && who.hasQuest("28")) || !who.IsLocalPlayer)
                    return true;




                if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points >= 3125 && who.mailReceived.Add("CF_Spouse"))
                {
                    Monitor.Log($"getting starfruit");
                    __instance.CurrentDialogue.Push(new Dialogue(__instance, Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001", false));
                    StardewValley.Object stardrop = ItemRegistry.Create<StardewValley.Object>("(O)434", 1, 0, false);
                    stardrop.CanBeSetDown = false;
                    stardrop.CanBeGrabbed = false;
                    Game1.player.addItemByMenuIfNecessary(stardrop, null);
                    __instance.shouldSayMarriageDialogue.Value = false;
                    __instance.currentMarriageDialogue.Clear();
                    __result = true;
                    return false;
                }

                if (
                    (who.friendshipData.ContainsKey(__instance.Name) && (who.friendshipData[__instance.Name].IsMarried() || who.friendshipData[__instance.Name].IsEngaged())) ||
                    ((__instance.datable.Value || Config.AllowNonDateableNPCsToHugAndKiss) && who.friendshipData.ContainsKey(__instance.Name) && !who.friendshipData[__instance.Name].IsMarried() && !who.friendshipData[__instance.Name].IsEngaged() && ((who.friendshipData[__instance.Name].IsDating() && Config.DatingKisses) || (who.getFriendshipHeartLevelForNPC(__instance.Name) >= Config.HeartsForFriendship && Config.FriendHugs)))
                    )
                {
                    __instance.faceDirection(-3);

                    if (__instance.Sprite.CurrentAnimation == null && !__instance.hasTemporaryMessageAvailable() && __instance.currentMarriageDialogue.Count == 0 && __instance.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !__instance.isMoving() && who.ActiveObject == null)
                    {
                        bool kissing = who.friendshipData[__instance.Name].IsDating() || who.friendshipData[__instance.Name].IsMarried() || who.friendshipData[__instance.Name].IsEngaged();
                        Monitor.Log($"{who.Name} {(kissing ? "kissing" : "hugging")} {__instance.Name}");

                        if (kissing && __instance.hasBeenKissedToday.Value && !Config.UnlimitedDailyKisses)
                        {
                            Monitor.Log($"already kissed {__instance.Name}");
                            return false;
                        }

                        __instance.faceGeneralDirection(who.getStandingPosition(), 0, false);
                        who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                        if (__instance.FacingDirection == 3 || __instance.FacingDirection == 1)
                        {
                            if (kissing)
                            {
                                Kissing.PlayerNPCKiss(who, __instance);
                            }
                            else
                            {
                                Kissing.PlayerNPCHug(who, __instance);
                            }
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Farmer_checkAction_Prefix(Farmer who, GameLocation location, ref bool __result, ref NetEvent1Field<long,NetLong> ___kissFarmerEvent)
        {
            if (Game1.CurrentEvent != null)
            {
                return true;
            }

            if (who.isRidingHorse()||who.hidden.Value||who.CurrentItem != null||!who.CanMove)
            {
                return true;
            }

            long? playerSpouseID = who.team.GetSpouse(who.UniqueMultiplayerID);

            if (playerSpouseID.HasValue && playerSpouseID == who.UniqueMultiplayerID)
            {
                return true;
            }

            try
            {


                if ( who.CanMove &&!who.isMoving() && !Game1.player.isMoving() && Utility.IsHorizontalDirection(Game1.player.getGeneralDirectionTowards(who.getStandingPosition(), -10, opposite: false, useTileCalculations: false)))
                {
                    who.Halt();
                    who.faceGeneralDirection(who.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
                    ___kissFarmerEvent.Fire(who.UniqueMultiplayerID);
                   // Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, base.Tile * 64f + new Vector2(16f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
                   // {
                   //     motion = new Vector2(0f, -0.5f),
                   //     alphaFade = 0.01f
                   // });
                   // Game1.player.playNearbySoundAll("dwop", null, SoundContext.NPC);
                    __result = true;
                    return false;


                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_checkAction_Prefix)}:\n{ex}", LogLevel.Error);

                return true;
            }
            return true;
        }

      
    }
}
