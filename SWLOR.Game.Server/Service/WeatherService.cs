﻿using System.Collections.Generic;
using System.Linq;
using NWN;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.Event.Area;
using SWLOR.Game.Server.Event.Module;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Messaging;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.NWN.Enum;
using SWLOR.Game.Server.NWN.Enum.Area;
using SWLOR.Game.Server.NWN.Enum.VisualEffect;
using SWLOR.Game.Server.ValueObject;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

/*
    Name: WeatherService
    Author: Mithreas
    Date: 29 May 08, ported to cs 1 Jan 2019
    Description:  Implements a module wide weather system.

    Integration:
    * Call OnModuleHeartbeat() from heartbeat script.  Calls:
    * Call OnAreaEnter from area entry script.
    * Call OnCombatRoundEnd() for each PC or NPC in combat, each round
    * Call OnCreatureSpawn() in the creature spawn script. 
    * Optional: set all areas to 0% rain/snow (their weather will be overridden)
    * Optional: place area property objects as follows.
    * Climate: Wet or Climate: Dry for especially damp/dry areas
    * Climate: Hot or Climate: Cold for especially warm/cool areas
    * Climate: Sheltered or Climate: Exposed ...etc.

    Functionality:
    All three scales range from a score of 1 to 10.  Depending on the score
    different effects occur as follows.
    Heat
    rain turns to snow when heat < 4
    Humidity
    water rate drops by additional 0.5 per humidity point under 5
    rain/snow happens if humidity > 7
    Wind
    All creatures get 10% speed reduction when wind > 8
    Characters in combat must pass a discipline check or reflex save vs a DC
    of 10 + character level every round or be KD'd when wind == 10.
    Weather changes faster when wind is higher.

    Weather changes every 11 game hours minus the wind score.

    Multiple planet support
    - Areas are named PlanetName - areaname
    - Planets have distinct climates, implemented as +/- on each scale
    - Different climates can have different hazards - acid rain, sandstorms etc.
*/
namespace SWLOR.Game.Server.Service
{
    public static class WeatherService
    {
        public static void SubscribeEvents()
        {
            MessageHub.Instance.Subscribe<OnAreaEnter>(message => OnAreaEnter());
            MessageHub.Instance.Subscribe<OnModuleHeartbeat>(message => OnModuleHeartbeat());
        }

        // Feedback texts.
        const string FB_T_WEATHER_LIGHTNING = "You were hit by the bolt of lightning!";
        const string FB_T_WEATHER_MESSAGE_CLOUDY = "Clouds move across the sky at a brisk pace, driven by strong wind.";
        const string FB_T_WEATHER_MESSAGE_COLD_CLOUDY = "Cold air is punctuated by an overcast sky, the clouds thick and dark.";
        const string FB_T_WEATHER_MESSAGE_COLD_WINDY = "A chill wind is in the air, cutting like a knife.";
        const string FB_T_WEATHER_MESSAGE_COLD_MILD = "It is quite cold out. Wrap up warm by wearing winter clothing.";
        const string FB_T_WEATHER_MESSAGE_FREEZING = "The air is bitingly cold right now. Make sure you are wrapped up warm and have plenty of rations.";
        const string FB_T_WEATHER_MESSAGE_MILD = "It is lovely and sunny here.";
        const string FB_T_WEATHER_MESSAGE_MILD_NIGHT = "The weather is fine tonight.";
        const string FB_T_WEATHER_MESSAGE_MIST = "It is still and very humid, the mist hangs in the air about you.";
        const string FB_T_WEATHER_MESSAGE_WARM_CLOUDY = "It is hot, and clouds are dotted around. Travels will be tiring - you should wear light clothing.";
        const string FB_T_WEATHER_MESSAGE_WARM_MILD = "It is warm and calm here. Make sure you have enough to drink in the extra heat, and wear light clothing.";
        const string FB_T_WEATHER_MESSAGE_WARM_WINDY = "Warm gusts of wind ripple the air here, and there are a worrying number of clouds casting shadows over the earth. We might experience thunderstorms, so be careful.";
        const string FB_T_WEATHER_MESSAGE_RAIN_NORMAL = "It is raining. Your travels will be a little difficult because of it.";
        const string FB_T_WEATHER_MESSAGE_RAIN_WARM = "It is raining, and the air is humid. Thunderstorms are likely, and it will be more difficult to make progress on your journey.";
        const string FB_T_WEATHER_MESSAGE_SCORCHING = "The heat is blazing here! You should wear something to protect your face and hands, if you can.";
        const string FB_T_WEATHER_MESSAGE_SNOW = "It is snowing right now! Remember to wrap up warm and pack extra provisions.";
        const string FB_T_WEATHER_MESSAGE_STORM = "There is a thunderstorm at the moment. It will be quite dangerous out.";
        const string FB_T_WEATHER_MESSAGE_WINDY = "The wind is very strong, and there are many clouds in the sky. The weather could shift at any time.";

        const string FB_T_WEATHER_DUST_STORM = "There is a dust storm!  Visibility is drastically reduced.";
        const string FB_T_WEATHER_SAND_STORM = "There is a sand storm! Take cover immediately, these storms are very dangerous!";

        // Module and area variables.
        const string VAR_WEATHER_CHANGE = "VAR_WEATHER_CHANGE";
        const string VAR_WEATHER_HEAT = "VAR_WEATHER_HEAT";
        const string VAR_WEATHER_HUMIDITY = "VAR_WEATHER_HUMIDITY";
        const string VAR_WEATHER_MIST = "VAR_WEATHER_MIST";
        const string VAR_WEATHER_WIND = "VAR_WEATHER_WIND";
        const string VAR_WEATHER_ACID_RAIN = "VAR_WEATHER_ACID_RAIN";
        const string VAR_EFFECTOR = "VAR_WH_EFFECTOR";
        const string VAR_INITIALIZED = "VAR_WH_INITIALIZED";
        const string VAR_SKYBOX = "VAR_WH_SKYBOX";
        const string VAR_TIMESTAMP = "VAR_WH_TIMESTAMP";
        const string VAR_FOG_SUN = "VAR_WH_FOG_SUN";
        const string VAR_FOG_MOON = "VAR_WH_FOG_MOON";
        const string VAR_FOG_C_SUN = "VAR_WH_FOG_C_SUN";
        const string VAR_FOG_C_MOON = "VAR_WH_FOG_C_MOON";

        
        struct PlanetaryClimate
        {
            public int Heat_Modifier;
            public int Humidity_Modifier;
            public int Wind_Modifier;

            // Does the planet suffer from unusual weather?
            public bool Acid_Rain;
            public bool Dust_Storm;
            public bool Sand_Storm;
            //public bool Snow_Storm;

            // Allow overrides of text on different planets.
            public string special_cloudy;
            public string special_cold_cloudy;
            public string special_cold_mild;
            public string special_cold_windy;
            public string special_freezing;
            public string special_mild;
            public string special_mild_night;
            public string special_mist;
            public string special_warm_cloudy;
            public string special_warm_mild;
            public string special_warm_windy;
            public string special_rain_normal;
            public string special_rain_warm;
            public string special_scorching;
            public string special_snow;
            public string special_storm;
            public string special_windy;
        }

        private static PlanetaryClimate _GetClimate(NWObject oArea)
        {
            PlanetaryClimate climate = new PlanetaryClimate();

            //--------------------------------------------------------------------------
            // This line depends on the naming scheme PlanetName - AreaName.  Change it
            // if the area naming scheme changes!
            //--------------------------------------------------------------------------
            int index = NWScript.GetName(oArea).IndexOf("-");
            if (index <= 0) return climate;
            string planetName = NWScript.GetName(oArea).Substring(0, index - 1);

            if (planetName == "Viscara")
            {
                LoggingService.Trace(TraceComponent.Weather, "Planet is Viscara.");
                climate.Heat_Modifier = -2;
                climate.Humidity_Modifier = +2;
            }
            else if (planetName == "Tatooine")
            {
                LoggingService.Trace(TraceComponent.Weather, "Planet is Tatooine.");
                climate.Heat_Modifier = +5;
                climate.Humidity_Modifier = -8;

                climate.Sand_Storm = true;

                climate.special_cloudy = "A dusty wind sweeps through the desert; sparse clouds speed overhead.";
                climate.special_mild = "The sun shines brilliantly, but not oppressively, over the desert; the sky is clear.";
                climate.special_mild_night = "A clear night sky casts the desert in pale hues.";
                climate.special_warm_cloudy = "The shade of an overcast sky provides only minor relief to the sweltering temperatures.";
                climate.special_scorching = "The desert is baked with pervasive, inescapable heat; a haze blurs the horizon.";
                climate.special_warm_windy = "The hot wind wears at your face like a sandblaster.  A sand storm seems likely.";
                climate.special_windy = "A scouring wind sweeps across the desert, a sand storm cannot be far away.";
            }
            else if (planetName == "Mon Cala")
            {
                LoggingService.Trace(TraceComponent.Weather, "Planet is Mon Cala.");
                climate.Humidity_Modifier = 0;
                climate.Wind_Modifier = +1;
                climate.Heat_Modifier = +1;

                climate.special_cloudy = "Clouds build over the ocean, and the wind starts to pick up.  A storm could be brewing.";
                climate.special_cold_cloudy = "Thick clouds fill the sky, and a keen wind blows in off the ocean, exciting the waves.";
                climate.special_cold_mild = "It is cool, but calm.  The ocean is calm and beautiful.";
                climate.special_freezing = "A wave of cold air rolls in, stinging exposed flesh.";
                climate.special_mild = "The sea is calm, a faint breeze rippling through the trees.";
                climate.special_mild_night = "The sea is calm, and the sky towards the Galactic Core is full of stars.  In other directions, you see only a deep, unending black.";
                climate.special_mist = "A mist has blown in off the sea, moisture hanging heavy in the air.";
                climate.special_warm_cloudy = "The sea is choppy and the wind has picked up. An array of clouds marshals on the horizon, ready to sweep over you.";
                climate.special_warm_mild = "It is a beautiful day, warm and calm, though quite humid.";
                climate.special_rain_normal = "The ocean, affronted by the existence of patches of non-ocean on the surface of the planet, is attempting to reclaim the land by air drop.  In other words, it's raining.";
                climate.special_rain_warm = "A heavy rain shower is passing over, but is doing little to dispel the humidity in the air.";
                climate.special_snow = "It's snowing!  The local flora seems most surprised at this turn of events.";
                climate.special_storm = "A storm rips in off the sea, filling the sky with dramatic flashes.";
                climate.special_scorching = "The sun bakes the sand, making it extremely uncomfortable to those without insulated boots.";
                climate.special_cold_windy = "A chill wind sweeps over the isles, the moisture in the air cutting to the bone.";
                climate.special_warm_windy = "The wind is picking up, a warm front rolling over.  There could be a storm soon.";
                climate.special_windy = "A strong wind sweeps in.  The sea is choppy, waves crashing onto the beach.";
            }
            else if (planetName == "Hutlar")
            {
                LoggingService.Trace(TraceComponent.Weather, "Planet is Hutlar.");
                climate.Heat_Modifier = -5;
                climate.Humidity_Modifier = -8;

                climate.special_freezing = "A wave of cold air rolls in, stinging exposed flesh.";
                climate.special_snow = "It's snowing... again...";
                climate.special_windy = "A cold wind sweeps in.";
                climate.special_cold_windy = "A freezing wind stings exposed flesh";
                climate.special_cloudy = "Clouds build over head, and there is a occasional strong gust of wind.";
                climate.special_cold_cloudy = "The clouds over head build, a cold wind stings exposed flesh. Looks like it is going to snow.";
                climate.special_cold_mild = "It is cold, the sky is clear, and there is a gentle breeze.";
                //climate.Snow_Storm = true;
            }

            return climate;
        }

        public static bool AdjustWeather()
        {
            LoggingService.Trace(TraceComponent.Weather, "Adjusting module weather");
            NWObject oMod = NWScript.GetModule();

            //--------------------------------------------------------------------------
            // Always change the weather the very first time
            //--------------------------------------------------------------------------
            if (oMod.GetLocalInt(VAR_INITIALIZED) == 0)
            {
                oMod.SetLocalInt(VAR_INITIALIZED, 1);
                _SetHumidity(NWScript.Random(10) + 1);
            }
            else if (NWScript.GetTimeHour() != oMod.GetLocalInt(VAR_WEATHER_CHANGE))
            {
                LoggingService.Trace(TraceComponent.Weather, "No change needed... yet.");
                return false;
            }

            //--------------------------------------------------------------------------
            // Adjust the indices.  Only humidity is affected by the current values.
            //--------------------------------------------------------------------------
            int nHeat = GetHeatIndex();
            int nHumidity = GetHumidity();
            int nWind = GetWindStrength();

            //--------------------------------------------------------------------------
            // Heat is affected by time of year.
            //--------------------------------------------------------------------------
            nHeat = NWScript.Random(5) + (6 - NWScript.abs(NWScript.GetCalendarMonth() - 6)); // (0-4 + 0-6)
            if (nHeat < 1) nHeat = 1;

            //--------------------------------------------------------------------------
            // Humidity is random but moves slowly.
            //--------------------------------------------------------------------------
            nHumidity = nHumidity + (NWScript.Random(2 * nWind + 1) - nWind);
            if (nHumidity > 10) nHumidity = 20 - nHumidity;
            if (nHumidity < 1) nHumidity = 1 - nHumidity;

            //--------------------------------------------------------------------------
            // Wind is more likely to be calm, but can change quickly.
            //--------------------------------------------------------------------------
            nWind = NWScript.d10(2) - 10;
            if (nWind < 1) nWind = 1 - nWind;

            LoggingService.Trace(TraceComponent.Weather, "New weather settings: heat - " + NWScript.IntToString(nHeat) +
                                           ", humidity - " + NWScript.IntToString(nHumidity) +
                                               ", wind - " + NWScript.IntToString(nWind));

            _SetHeatIndex(nHeat);
            _SetHumidity(nHumidity);
            _SetWindStrength(nWind);

            //--------------------------------------------------------------------------
            // Work out when to next change the weather.
            //--------------------------------------------------------------------------
            int nNextChange = NWScript.GetTimeHour() + (11 - nWind);
            if (nNextChange > 23) nNextChange -= 24;
            LoggingService.Trace(TraceComponent.Weather, "Change the weather next at hour " + NWScript.IntToString(nNextChange));
            oMod.SetLocalInt(VAR_WEATHER_CHANGE, nNextChange);

            // Update all occupied areas with the new settings.
            NWObject oPC = NWScript.GetFirstPC();
            while (NWScript.GetIsObjectValid(oPC))
            {
                SetWeather(NWScript.GetArea(oPC));
                oPC = NWScript.GetNextPC();
            }

            return true;
        }

        public static void SetWeather()
        {
            SetWeather(NWScript.OBJECT_SELF);
        }

        public static void SetWeather(NWObject oArea)
        {
            
            if (oArea.GetLocalInt(VAR_INITIALIZED) == 0)
            {
                if (NWScript.GetIsAreaInterior(oArea) == true ||
                    NWScript.GetIsAreaAboveGround(oArea) == false)
                    return;
                oArea.SetLocalInt(VAR_SKYBOX, (int)NWScript.GetSkyBox(oArea));
                oArea.SetLocalInt(VAR_FOG_SUN, NWScript.GetFogAmount(FogType.Sun, oArea));
                oArea.SetLocalInt(VAR_FOG_MOON, NWScript.GetFogAmount(FogType.Moon, oArea));
                oArea.SetLocalInt(VAR_FOG_C_SUN, (int)NWScript.GetFogColor(FogType.Sun, oArea));
                oArea.SetLocalInt(VAR_FOG_C_MOON, (int)NWScript.GetFogColor(FogType.Moon, oArea));
                oArea.SetLocalInt(VAR_INITIALIZED, 1);
            }

            int nHeat = GetHeatIndex(oArea);
            int nHumidity = GetHumidity(oArea);
            int nWind = GetWindStrength(oArea);
            bool bStormy = NWScript.GetSkyBox(oArea) == Skybox.GrassStorm;
            bool bDustStorm = (oArea.GetLocalInt("DUST_STORM") == 1);
            bool bSandStorm = (oArea.GetLocalInt("SAND_STORM") == 1);

            //--------------------------------------------------------------------------
            // Process weather rules for this area.
            //--------------------------------------------------------------------------
            if (nHumidity > 7 && nHeat > 3)
            {
                if (nHeat < 6 && nWind < 3)
                {
                    NWScript.SetWeather(oArea, WeatherType.Clear);
                }
                else NWScript.SetWeather(oArea, WeatherType.Rain);
            }
            else if (nHumidity > 7) NWScript.SetWeather(oArea, WeatherType.Snow);
            else NWScript.SetWeather(oArea, WeatherType.Clear);

            //--------------------------------------------------------------------------
            // Stormy if heat is greater than 4 only; if already stormy then 2 in 3
            // chance of storm clearing, otherwise x in 20 chance of storm starting,
            // where x is the wind level.
            //--------------------------------------------------------------------------
            if (nHeat > 4 && nHumidity > 7 &&
                ((bStormy && NWScript.d20() - nWind < 1) || (bStormy && NWScript.d3() == 1)))
            {
                LoggingService.Trace(TraceComponent.Weather, "A thunderstorm is now raging in " + NWScript.GetName(oArea));
                NWScript.SetSkyBox(Skybox.GrassStorm, oArea);
                Thunderstorm(oArea);
                oArea.SetLocalInt("GS_AM_SKY_OVERRIDE", 1);
                bStormy = true;
            }
            else
            {
                NWScript.SetSkyBox((Skybox)oArea.GetLocalInt(VAR_SKYBOX), oArea);
                oArea.DeleteLocalInt("GS_AM_SKY_OVERRIDE");
                bStormy = false;
            }

            // Does this area suffer from dust or sand storms?
            if (!bStormy && nWind >= 9 && NWScript.d3() == 1)
            {
                // Dust storm - low visibility but no damage.
                if (_GetClimate(oArea).Dust_Storm)
                {
                    NWScript.SetFogColor(FogType.Sun, FogColor.Brown , oArea);
                    NWScript.SetFogColor(FogType.Moon, FogColor.Brown , oArea);
                    NWScript.SetFogAmount(FogType.Sun, 80, oArea);
                    NWScript.SetFogAmount(FogType.Moon, 80, oArea);

                    oArea.SetLocalInt("DUST_STORM", 1);
                    bDustStorm = true;
                }
                else if (_GetClimate(oArea).Sand_Storm)
                {
                    NWScript.SetFogColor(FogType.Sun, FogColor.OrangeDark, oArea);
                    NWScript.SetFogColor(FogType.Moon, FogColor.OrangeDark, oArea);
                    NWScript.SetFogAmount(FogType.Sun, 80, oArea);
                    NWScript.SetFogAmount(FogType.Moon, 80, oArea);

                    oArea.SetLocalInt("SAND_STORM", 1);
                    bSandStorm = true;
                }
            }
            else if (bDustStorm || bSandStorm)
            {
                // End the storm.
                oArea.DeleteLocalInt("DUST_STORM");
                oArea.DeleteLocalInt("SAND_STORM");

                NWScript.SetFogColor(FogType.Sun, (FogColor)oArea.GetLocalInt(VAR_FOG_C_SUN), oArea);
                NWScript.SetFogColor(FogType.Moon, (FogColor)oArea.GetLocalInt(VAR_FOG_C_MOON), oArea);
                NWScript.SetFogAmount(FogType.Sun, oArea.GetLocalInt(VAR_FOG_SUN), oArea);
                NWScript.SetFogAmount(FogType.Moon, oArea.GetLocalInt(VAR_FOG_MOON), oArea);
                bSandStorm = false;
                bDustStorm = false;
            }

            LoggingService.Trace(TraceComponent.Weather, "Area weather settings for area: " + NWScript.GetName(oArea) +
                                                  ", heat - " + NWScript.IntToString(nHeat) +
                                              ", humidity - " + NWScript.IntToString(nHumidity) +
                                                  ", wind - " + NWScript.IntToString(nWind) +
                                                 ", thunderstorm - " + bStormy.ToString() +
                                                 ", sand storm - " + bSandStorm.ToString() +
                                                 ", dust storm - " + bDustStorm.ToString());
        }

        public static Weather GetWeather()
        {
            return GetWeather(NWScript.OBJECT_SELF);
        }

        public static Weather GetWeather(NWObject oArea)
        {
            LoggingService.Trace(TraceComponent.Weather, "Getting current weather for area: " + NWScript.GetName(oArea));

            if (NWScript.GetIsAreaInterior(oArea) == true || NWScript.GetIsAreaAboveGround(oArea) == false)
            {
                return Weather.Invalid;
            }

            int nHeat = GetHeatIndex(oArea);
            int nHumidity = GetHumidity(oArea);
            int nWind = GetWindStrength(oArea);

            if (nHumidity > 7 && nHeat > 3 && nHeat < 6 && nWind < 3)
            {
                return Weather.Foggy;
            }

            // Rather unfortunately, the default method is also called GetWeather. 
            return NWScript.GetWeather(oArea);
        }

        public static void OnCombatRoundEnd(NWObject oCreature)
        {
            NWObject oArea = NWScript.GetArea(oCreature);
            if (oArea.GetLocalInt(VAR_INITIALIZED) == 0)
                return;

            int nWind = GetWindStrength(oArea);

            if (nWind > 9) _DoWindKnockdown(oCreature);
        }

        public static void ApplyAcid(NWObject oTarget, NWObject oArea)
        {
            if ((NWObject)NWScript.GetArea(oTarget) != oArea) return;
            if (NWScript.GetIsDead(oTarget)) return;
            if (NWScript.GetIsPC(oTarget) && NWScript.GetIsPC(NWScript.GetMaster(oTarget)) == false) return;

            //apply
            Effect eEffect =
                NWScript.EffectLinkEffects(
                    NWScript.EffectVisualEffect(VisualEffect.Vfx_Imp_Acid_S),
                    NWScript.EffectDamage(
                        NWScript.d6(1),
                        DamageType.Acid));

            NWScript.ApplyEffectToObject(DurationType.Instant, eEffect, oTarget);

            NWScript.DelayCommand(6.0f, () => { ApplyAcid(oTarget, oArea); });
        }

        public static void ApplySandstorm(NWObject oTarget, NWObject oArea)
        {
            if ((NWObject)NWScript.GetArea(oTarget) != oArea) return;
            if (NWScript.GetIsDead(oTarget)) return;
            if (NWScript.GetIsPC(oTarget) && NWScript.GetIsPC(NWScript.GetMaster(oTarget)) == false) return;

            //apply
            Effect eEffect =
                NWScript.EffectLinkEffects(
                    NWScript.EffectVisualEffect(VisualEffect.Vfx_Imp_Flame_S),
                    NWScript.EffectDamage(
                        NWScript.d6(2),
                        NWScript.DAMAGE_TYPE_BLUDGEONING));

            NWScript.ApplyEffectToObject(DurationType.Instant, eEffect, oTarget);

            NWScript.DelayCommand(6.0f, () => { ApplySandstorm(oTarget, oArea); });
        }

        public static void DoWeatherEffects(NWObject oCreature)
        {
            NWObject oArea = NWScript.GetArea(oCreature);
            if (NWScript.GetIsAreaInterior(oArea) == 1 || NWScript.GetIsAreaAboveGround(oArea) == 0) return;

            int nHeat = GetHeatIndex(oArea);
            int nHumidity = GetHumidity(oArea);
            int nWind = GetWindStrength(oArea);
            bool bStormy = NWScript.GetSkyBox(oArea) == NWScript.SKYBOX_GRASS_STORM;
            bool bIsPC  = (NWScript.GetIsPC(oCreature) == 1);
            string sMessage = "";
            PlanetaryClimate climate = _GetClimate(oArea);

            //--------------------------------------------------------------------------
            // Apply acid rain, if applicable.  Stolen shamelessly from the Melf's Acid
            // Arrow spell.
            //--------------------------------------------------------------------------
            if (bIsPC && NWScript.GetWeather(oArea) == NWScript.WEATHER_RAIN && oArea.GetLocalInt(VAR_WEATHER_ACID_RAIN) == 1)
            {
                Effect eEffect =
                  NWScript.EffectLinkEffects(
                      NWScript.EffectVisualEffect(NWScript.VFX_IMP_ACID_S),
                      NWScript.EffectDamage(
                          NWScript.d6(2),
                          NWScript.DAMAGE_TYPE_ACID));

                NWScript.ApplyEffectToObject(DurationType.Instant, eEffect, oCreature);

                NWScript.DelayCommand(6.0f, () => { ApplyAcid(oCreature, oArea); });
            }
            else if (bIsPC && oArea.GetLocalInt("DUST_STORM") == 1)
            {
                sMessage = FB_T_WEATHER_DUST_STORM;
            }
            else if (bIsPC && oArea.GetLocalInt("SAND_STORM") == 1)
            {
                sMessage = FB_T_WEATHER_SAND_STORM;
                Effect eEffect =
                    NWScript.EffectLinkEffects(
                        NWScript.EffectVisualEffect(VisualEffect.Vfx_Imp_Flame_S),
                        NWScript.EffectDamage(
                            NWScript.d6(2),
                            DamageType.Bludgeoning));

                NWScript.ApplyEffectToObject(DurationType.Instant, eEffect, oCreature);

                NWScript.DelayCommand(6.0f, () => { ApplySandstorm(oCreature, oArea); });
            }                                    
            else if (bIsPC)
            {
                // Stormy weather
                if (bStormy)
                {
                    sMessage = string.IsNullOrWhiteSpace(climate.special_storm) ? FB_T_WEATHER_MESSAGE_STORM : climate.special_storm;
                }
                // Rain or mist
                else if (nHumidity > 7 && nHeat > 3)
                {
                    // Mist
                    if (nHeat < 6 && nWind < 3)
                    {
                        sMessage = string.IsNullOrWhiteSpace(climate.special_mist) ? FB_T_WEATHER_MESSAGE_MIST : climate.special_mist;
                    }
                    // Humid
                    else if (nHeat > 7)
                    {
                        sMessage = string.IsNullOrWhiteSpace(climate.special_rain_warm) ? FB_T_WEATHER_MESSAGE_RAIN_WARM : climate.special_rain_warm;
                    }
                    else
                    {
                        sMessage = string.IsNullOrWhiteSpace(climate.special_rain_normal) ? FB_T_WEATHER_MESSAGE_RAIN_NORMAL : climate.special_rain_normal;
                    }
                }
                // Snow
                else if (nHumidity > 7)
                {
                    sMessage = string.IsNullOrWhiteSpace(climate.special_snow) ? FB_T_WEATHER_MESSAGE_SNOW : climate.special_snow;
                }
                // Freezing
                else if (nHeat < 3)
                {
                    sMessage = string.IsNullOrWhiteSpace(climate.special_freezing) ? FB_T_WEATHER_MESSAGE_FREEZING : climate.special_freezing;
                }
                // Boiling
                else if (nHeat > 8)
                {
                    sMessage = string.IsNullOrWhiteSpace(climate.special_scorching) ? FB_T_WEATHER_MESSAGE_SCORCHING : climate.special_scorching;
                }
                // Cold
                else if (nHeat < 5)
                {
                    if (nWind < 5) sMessage = string.IsNullOrWhiteSpace(climate.special_cold_mild) ? FB_T_WEATHER_MESSAGE_COLD_MILD : climate.special_cold_mild;
                    else if (nWind < 8) sMessage = string.IsNullOrWhiteSpace(climate.special_cold_cloudy) ? FB_T_WEATHER_MESSAGE_COLD_CLOUDY : climate.special_cold_cloudy;
                    else sMessage = string.IsNullOrWhiteSpace(climate.special_cold_windy) ? FB_T_WEATHER_MESSAGE_COLD_WINDY : climate.special_cold_windy;
                }
                // Hot
                else if (nHeat > 6)
                {
                    if (nWind < 5) sMessage = string.IsNullOrWhiteSpace(climate.special_warm_mild) ? FB_T_WEATHER_MESSAGE_WARM_MILD : climate.special_warm_mild;
                    else if (nWind < 8) sMessage = string.IsNullOrWhiteSpace(climate.special_warm_cloudy) ? FB_T_WEATHER_MESSAGE_WARM_CLOUDY : climate.special_warm_cloudy;
                    else sMessage = string.IsNullOrWhiteSpace(climate.special_warm_windy) ? FB_T_WEATHER_MESSAGE_WARM_WINDY : climate.special_warm_windy;
                }
                else if (nWind < 5)
                {
                    if (!NWScript.GetIsNight()) sMessage = string.IsNullOrWhiteSpace(climate.special_mild) ? FB_T_WEATHER_MESSAGE_MILD : climate.special_mild;
                    else sMessage = string.IsNullOrWhiteSpace(climate.special_mild_night) ? FB_T_WEATHER_MESSAGE_MILD_NIGHT : climate.special_mild_night;
                }
                else if (nWind < 8) sMessage = string.IsNullOrWhiteSpace(climate.special_cloudy) ? FB_T_WEATHER_MESSAGE_CLOUDY : climate.special_cloudy;
                else sMessage = string.IsNullOrWhiteSpace(climate.special_windy) ? FB_T_WEATHER_MESSAGE_WINDY : climate.special_windy;

                NWScript.SendMessageToPC(oCreature, sMessage);
            }
        }
        
        public static int GetHeatIndex()
        {
            return GetHeatIndex(NWScript.OBJECT_SELF);
        }

        public static int GetHeatIndex(NWObject oArea)
        {
            //--------------------------------------------------------------------------
            // Areas may have one of the CLIMATE_* values stored in each weather var.
            //--------------------------------------------------------------------------
            NWObject oMod = NWScript.GetModule();
            int nHeat = oMod.GetLocalInt(VAR_WEATHER_HEAT);
            if (oArea.IsValid)
            {
                nHeat += oArea.GetLocalInt(VAR_WEATHER_HEAT);
                nHeat += _GetClimate(oArea).Heat_Modifier;
            }

            nHeat = (NWScript.GetIsNight() == true) ? nHeat - 2 : nHeat + 2;

            if (nHeat > 10) nHeat = 10;
            if (nHeat < 1) nHeat = 1;

            return nHeat;
        }

        public static int GetHumidity()
        {
            return GetHumidity(NWScript.OBJECT_SELF);
        }

        public static int GetHumidity(NWObject oArea)
        {
            //--------------------------------------------------------------------------
            // Areas may have one of the CLIMATE_* values stored in each weather var.
            //--------------------------------------------------------------------------
            NWObject oMod = NWScript.GetModule();
            int nHumidity = oMod.GetLocalInt(VAR_WEATHER_HUMIDITY);
            if (oArea.IsValid)
            {
                nHumidity += oArea.GetLocalInt(VAR_WEATHER_HUMIDITY);
                nHumidity += _GetClimate(oArea).Humidity_Modifier;
            }

            if (nHumidity > 10) nHumidity = 10;
            if (nHumidity < 1) nHumidity = 1;

            return nHumidity;
        }

        public static int GetWindStrength()
        {
            return GetWindStrength(NWScript.OBJECT_SELF);
        }

        public static int GetWindStrength(NWObject oArea)
        {
            //--------------------------------------------------------------------------
            // Areas will have one of the CLIMATE_* values stored in each weather var.
            //--------------------------------------------------------------------------
            NWObject oMod = NWScript.GetModule();
            int nWind = oMod.GetLocalInt(VAR_WEATHER_WIND);
            if (oArea.IsValid)
            {
                nWind += oArea.GetLocalInt(VAR_WEATHER_WIND);
                nWind += _GetClimate(oArea).Wind_Modifier;

                //----------------------------------------------------------------------
                // Automatic cover bonus for artificial areas such as cities (lots of
                // buildings).
                //----------------------------------------------------------------------
                if (NWScript.GetIsAreaNatural(oArea) == 0) nWind -= 1;
            }

            if (nWind > 10) nWind = 10;
            if (nWind < 1) nWind = 1;

            return nWind;
        }

        private static void _SetHeatIndex(int nHeat)
        {
            NWObject oMod = NWScript.GetModule();
            oMod.SetLocalInt(VAR_WEATHER_HEAT, nHeat);
        }

        private static void _SetHumidity(int nHumidity)
        {
            NWObject oMod = NWScript.GetModule();
            oMod.SetLocalInt(VAR_WEATHER_HUMIDITY, nHumidity);
        }

        private static void _SetWindStrength(int nWind)
        {
            NWObject oMod = NWScript.GetModule();
            oMod.SetLocalInt(VAR_WEATHER_WIND, nWind);
        }

        private static void _DoWindKnockdown(NWObject oCreature)
        {
            LoggingService.Trace(TraceComponent.Weather, "Checking whether " + NWScript.GetName(oCreature) + " is blown over");
            int nDC = (NWScript.GetHitDice(oCreature) / 2) + 10;
            int nDiscipline = NWScript.GetSkillRank(NWScript.SKILL_DISCIPLINE, oCreature);
            int nReflexSave = NWScript.GetReflexSavingThrow(oCreature);
            int nSuccess;

            if (nDiscipline > nReflexSave)
                nSuccess = NWScript.GetIsSkillSuccessful(oCreature, NWScript.SKILL_DISCIPLINE, nDC);
            else
                nSuccess = NWScript.ReflexSave(oCreature, nDC);

            if (nSuccess == 0)
            {
                NWScript.ApplyEffectToObject(DurationType.Temporary,
                                      NWScript.EffectKnockdown(),
                                      oCreature,
                                      6.0f);
                NWScript.FloatingTextStringOnCreature("*is unbalanced by a strong gust*", oCreature);
            }
        }

        public static void Thunderstorm(NWObject oArea)
        {
            // 1 in 3 chance of a bolt.
            if (NWScript.d3() != 1) return;

            // Pick a spot. Any spot.
            int nWidth = NWScript.GetAreaSize(NWScript.AREA_WIDTH, oArea);
            int nHeight = NWScript.GetAreaSize(NWScript.AREA_HEIGHT, oArea);
            int nPointWide = NWScript.Random(nWidth * 10);
            int nPointHigh = NWScript.Random(nHeight * 10);
            float fStrikeX = NWScript.IntToFloat(nPointWide) + (NWScript.IntToFloat(NWScript.Random(10)) * 0.1f);
            float fStrikeY = NWScript.IntToFloat(nPointHigh) + (NWScript.IntToFloat(NWScript.Random(10)) * 0.1f);

            // Now find out the power
            int nPower = NWScript.d100() + 10;

            // Fire ze thunderboltz!
            NWScript.DelayCommand(NWScript.IntToFloat(NWScript.Random(60)),
                () =>
                {
                    _Thunderstorm(NWScript.Location(oArea,
                                               NWScript.Vector(fStrikeX, fStrikeY),
                                               0.0f),
                                           nPower);
                }
                         );
        }

        private static void _Thunderstorm(NWLocation lLocation, int nPower)
        {
            float fRange = NWScript.IntToFloat(nPower) * 0.1f;
            // Caps on sphere of influence
            if (fRange < 3.0) fRange = 3.0f;
            if (fRange > 6.0) fRange = 6.0f;

            //Effects
            Effect eEffBolt = NWScript.EffectVisualEffect(NWScript.VFX_IMP_LIGHTNING_M);
            Effect eEffKnock = NWScript.EffectKnockdown();
            NWScript.ApplyEffectAtLocation(DurationType.Instant, eEffBolt, lLocation);

            Effect eEffDam;
            ObjectType nType;
            NWObject oObject = NWScript.GetFirstObjectInShape(Shape.Sphere, fRange, lLocation, false, ObjectType.Creature | ObjectType.Door | ObjectType.Placeable);
            while (NWScript.GetIsObjectValid(oObject) == 1)
            {
                nType = NWScript.GetObjectType(oObject);
                if ((nType & (ObjectType.Creature | ObjectType.Door | ObjectType.Placeable)) == 1)
                {
                    eEffDam = NWScript.EffectDamage(
                        NWScript.FloatToInt(NWScript.IntToFloat(nPower) - (NWScript.GetDistanceBetweenLocations(lLocation, NWScript.GetLocation(oObject)) * 10.0f)),
                        NWScript.DAMAGE_TYPE_ELECTRICAL);
                    NWScript.ApplyEffectToObject(DurationType.Instant, eEffDam, oObject);

                    if (nType == ObjectType.Creature)
                    {
                        if (NWScript.GetIsPC(oObject) == 1) NWScript.SendMessageToPC(oObject, FB_T_WEATHER_LIGHTNING);

                        NWScript.PlayVoiceChat(NWScript.VOICE_CHAT_PAIN1, oObject);
                        NWScript.ApplyEffectToObject(DurationType.Temporary, eEffKnock, oObject, NWScript.IntToFloat(NWScript.d6(1)));
                    }
                }
                oObject = NWScript.GetNextObjectInShape(NWScript.SHAPE_SPHERE, fRange, lLocation, 0, ObjectType.Creature | ObjectType.Door | ObjectType.Placeable);
            }
        }

        private static void OnAreaEnter()
        {
            using (new Profiler("WeatherService.OnAreaEnter"))
            {
                SetWeather();

                LoggingService.Trace(TraceComponent.Weather, "Applying weather to creature: " + NWScript.GetName(NWScript.GetEnteringObject()));

                DoWeatherEffects(NWScript.GetEnteringObject());

                NWArea oArea = (NWScript.OBJECT_SELF);
                int nHour = NWScript.GetTimeHour();
                int nLastHour = oArea.GetLocalInt("WEATHER_LAST_HOUR");

                if (nHour != nLastHour)
                {
                    if (!oArea.Data.ContainsKey("WEATHER_OBJECTS"))
                        oArea.Data["WEATHER_OBJECTS"] = new List<NWPlaceable>();
                    List<NWPlaceable> weatherObjects = oArea.Data["WEATHER_OBJECTS"];

                    LoggingService.Trace(TraceComponent.Weather, "Cleaning up old weather");

                    // Clean up any old weather placeables.
                    for (int x = weatherObjects.Count - 1; x >= 0; x--)
                    {
                        var placeable = weatherObjects.ElementAt(x);
                        placeable.Destroy();
                        weatherObjects.RemoveAt(x);
                    }

                    // Create new ones depending on the current weather.
                    int nWeather = GetWeather();
                    LoggingService.Trace(TraceComponent.Weather, "Current weather: " + nWeather.ToString());

                    if (nWeather == WEATHER_FOGGY)
                    {
                        // Get the size in tiles.
                        int nSizeX = NWScript.GetAreaSize(Dimension.Width, oArea);
                        int nSizeY = NWScript.GetAreaSize(Dimension.Height, oArea);

                        // We want one placeable per 8 tiles.
                        int nMax = (nSizeX * nSizeY) / 8;
                        LoggingService.Trace(TraceComponent.Weather, "Creating up to " + nMax.ToString() + " mist objects.");

                        for (int nCount = NWScript.d6(); nCount < nMax; nCount++)
                        {
                            Vector vPosition = NWScript.GetPosition(NWScript.GetEnteringObject());

                            // Vectors are in meters - 10 meters to a tile. 
                            vPosition.X = NWScript.IntToFloat(NWScript.Random(nSizeX * 10));
                            vPosition.Y = NWScript.IntToFloat(NWScript.Random(nSizeY * 10));

                            float fFacing = NWScript.IntToFloat(NWScript.Random(360));

                            string sResRef = "x3_plc_mist";

                            NWPlaceable oPlaceable = NWScript.CreateObject(ObjectType.Placeable, sResRef, NWScript.Location(oArea, vPosition, fFacing));
                            NWScript.SetObjectVisualTransform(oPlaceable, ObjectVisualTransform.Scale, NWScript.IntToFloat(200 + NWScript.Random(200)) / 100.0f);

                            weatherObjects.Add(oPlaceable);
                        }
                    }

                    oArea.Data["WEATHER_OBJECTS"] = weatherObjects;
                    oArea.SetLocalInt("WEATHER_LAST_HOUR", nHour);
                }
            }
        }

        private static void OnModuleHeartbeat()
        {
            NWObject oMod = NWScript.GetModule();
            int nHour = NWScript.GetTimeHour();
            int nLastHour = oMod.GetLocalInt("WEATHER_LAST_HOUR");

            if (nHour != nLastHour)
            {
                if (AdjustWeather())
                {
                    foreach (var player in NWModule.Get().Players)
                    {
                        DoWeatherEffects(player);
                    }
                }

                oMod.SetLocalInt("WEATHER_LAST_HOUR", nHour);
            }
        }

        public static void OnCreatureSpawn()
        {
            DoWeatherEffects(NWScript.OBJECT_SELF);
        }

        public static void SetAreaHeatModifier(NWObject oArea, int nModifier)
        {
            oArea.SetLocalInt(VAR_WEATHER_HEAT, nModifier);
        }

        public static void SetAreaWindModifier(NWObject oArea, int nModifier)
        {
            oArea.SetLocalInt(VAR_WEATHER_WIND, nModifier);
        }

        public static void SetAreaHumidityModifier(NWObject oArea, int nModifier)
        {
            oArea.SetLocalInt(VAR_WEATHER_HUMIDITY, nModifier);
        }

        public static void SetAreaAcidRain(NWObject oArea, int nModifier)
        {
            oArea.SetLocalInt(VAR_WEATHER_ACID_RAIN, nModifier);
        }
    }
}
