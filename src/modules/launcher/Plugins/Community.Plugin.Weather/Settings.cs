using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Community.Plugin.Weather
{
    class Settings
    {
        public static readonly String WEATHER_NOW = "http://api.openweathermap.org/data/2.5/weather";
        public static readonly String WEATHER_FIVE_DAY = "http://api.openweathermap.org/data/2.5/forecast";

        public String apiKey { get; set; } = "";

        [JsonConverter(typeof(StringEnumConverter))]
        public Units units = Units.metric;

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultType results = ResultType.now;

        public enum Units
        {
            metric,
            imperial
        }

        public enum ResultType
        {
            now,
            fiveDay
        }
    }
}
