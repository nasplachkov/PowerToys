using System;
using System.Collections.Generic;
using System.Text;

namespace Community.Plugin.Weather
{
    #pragma warning disable 0649

    class WeatherDataMain
    {
        public Double temp;
        public Double feels_like;
        public Double temp_min;
        public Double temp_max;
    }

    class WeatherType
    {
        public Double id;
        public String main;
        public String description;
        public String icon;
    }

    class WeatherData
    {
        public WeatherDataMain main;
        public List<WeatherType> weather;
    }
}
