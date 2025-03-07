﻿using StardewModdingAPI;
using StardewValley;
using System;

namespace PolyamorySweetKiss
{
    public static class EventPatches
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;


        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }
        public static bool Event_command_playSound_Prefix(Event @event, string[] args, EventContext context, Event __instance)
        {
            try
            {
                if (args[1] == "dwop" && __instance.isWedding && ModEntry.Config.CustomKissSound.Length > 0 && Kissing.kissEffect != null)
                {
                    Kissing.kissEffect.Play();
                    int num = __instance.CurrentCommand;
                    __instance.CurrentCommand = num + 1;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_command_playSound_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    }
}