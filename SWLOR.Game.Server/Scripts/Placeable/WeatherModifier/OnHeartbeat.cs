using NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.Service;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.Scripts.Placeable.WeatherModifier
{
    public class OnHeartbeat : IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWObject oSelf = NWScript.OBJECT_SELF;
            int nHeat = oSelf.GetLocalInt("WEATHER_HEAT");
            int nWind = oSelf.GetLocalInt("WEATHER_WIND");
            int nWet = oSelf.GetLocalInt("WEATHER_HUMIDITY");
            int nAcid = oSelf.GetLocalInt("WEATHER_ACID_RAIN");
            int nDust = oSelf.GetLocalInt("WEATHER_DUST_STORM");
            int nSand = oSelf.GetLocalInt("WEATHER_SAND_STORM");

            NWArea oArea = NWScript.GetArea(oSelf);
            WeatherService.SetAreaHeatModifier(oArea, nHeat);
            WeatherService.SetAreaHumidityModifier(oArea, nWet);
            WeatherService.SetAreaWindModifier(oArea, nWind);
            WeatherService.SetAreaAcidRain(oArea, nAcid);

            if (nDust > 0)
            {
                NWScript.SetFogColor(NWScript.FOG_TYPE_SUN, NWScript.FOG_COLOR_BROWN, oArea);
                NWScript.SetFogColor(NWScript.FOG_TYPE_MOON, NWScript.FOG_COLOR_BROWN, oArea);
                NWScript.SetFogAmount(NWScript.FOG_TYPE_SUN, 80, oArea);
                NWScript.SetFogAmount(NWScript.FOG_TYPE_MOON, 80, oArea);

                oArea.SetLocalInt("DUST_STORM", 1);

                foreach (NWObject player in oArea.Objects)
                {
                    if (player.IsPC) WeatherService.DoWeatherEffects(player);
                }
            }

            if (nSand > 0)
            {
                NWScript.SetFogColor(NWScript.FOG_TYPE_SUN, NWScript.FOG_COLOR_ORANGE_DARK, oArea);
                NWScript.SetFogColor(NWScript.FOG_TYPE_MOON, NWScript.FOG_COLOR_ORANGE_DARK, oArea);
                NWScript.SetFogAmount(NWScript.FOG_TYPE_SUN, 80, oArea);
                NWScript.SetFogAmount(NWScript.FOG_TYPE_MOON, 80, oArea);

                oArea.SetLocalInt("SAND_STORM", 1);

                foreach (NWObject player in oArea.Objects)
                {
                    if (player.IsPC) WeatherService.DoWeatherEffects(player);
                }
            }
            NWScript.DestroyObject(oSelf);
        }
    }
}
