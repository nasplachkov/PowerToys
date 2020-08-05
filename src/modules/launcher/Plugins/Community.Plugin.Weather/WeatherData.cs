using System;
using System.Collections.Generic;

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

    class WeatherSys
    {
        public String country;
    }

    class WeatherDataNow
    {
        public WeatherDataMain main;
        public List<WeatherType> weather;
        public String name;
        public WeatherSys sys;
    }

    class WeatherDataFiveDayItem
    {
        public long dt;
        public WeatherDataMain main;
        public List<WeatherType> weather;
    }

    class WeatherDataFiveDayCity
    {
        public String name;
        public String country;
    }

    class WeatherDataFiveDay
    {
        public int cnt;
        public List<WeatherDataFiveDayItem> list;
        public WeatherDataFiveDayCity city;
    }
}
