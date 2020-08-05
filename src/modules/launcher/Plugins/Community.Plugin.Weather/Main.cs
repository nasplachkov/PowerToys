using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace Community.Plugin.Weather
{
    public static class Constants
    { 
        public const int DEBOUNCE_TIMEOUT = 500;
        public const int MIN_SEARCH = 4;
    }

    public class Main : IPlugin, ISavable, IPluginI18n, IDisposable
    {
        private PluginInitContext Context { get; set; }
        private string IconPath { get; set; }
        private bool _disposed = false;

        private PluginJsonStorage<Settings> storage;
        private Settings settings;
        private CancellationTokenSource tokenSource;
        private HttpClient client;
        private List<Result> results;

        public Main()
        {
            storage = new PluginJsonStorage<Settings>();
            settings = storage.Load();
            tokenSource = null;
            client = new HttpClient();
        }

        public void Save()
        {
            storage.Save();
        }

        public void Init(PluginInitContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(paramName: nameof(context));
            }

            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query)
        {
            results = new List<Result>();
            if (query.FirstSearch.ToLower() == "weather" && query.SecondSearch.Length >= Constants.MIN_SEARCH)
            {
                GetWeatherData(query.SecondSearch, new CancellationTokenSource(TimeSpan.FromSeconds(5))).Wait();
            }
            return results;
        }

        private async Task GetWeatherData(String city, CancellationTokenSource tokenSource)
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
            }
            try
            {
                this.tokenSource = tokenSource;
                await Task.Delay(Constants.DEBOUNCE_TIMEOUT, tokenSource.Token);
            } catch (TaskCanceledException)
            {
                return;
            }
            switch (settings.results)
            {
                default:
                case Settings.ResultType.now:
                    await GetWeatherNow(city);
                    break;
                case Settings.ResultType.fiveDay:
                    await GetWeatherFiveDay(city);
                    break;
            }
        }

        private async Task GetWeatherNow(String city)
        {
            var data = await client.GetAsync($"{Settings.WEATHER_NOW}?q={city}&units={settings.units.ToString("f")}&APPID={settings.apiKey}");
            if (data.IsSuccessStatusCode)
            {
                var weatherString = await data.Content.ReadAsStringAsync();
                var weatherData = JsonConvert.DeserializeObject<WeatherDataNow>(weatherString);
                if (weatherData != null)
                {
                    // TODO: Translations
                    results.Add(
                        new Result
                        {
                            Title = $"{weatherData.weather[0]?.main} - {weatherData.main.temp.ToString("N1")}{GetDegrees()}",
                            SubTitle = $"{weatherData.name}, {weatherData.sys.country}",
                            IcoPath = $"Images/{weatherData.weather[0]?.icon}.png",
                            Score = 300,
                        }
                    );
                }
            }
        }

        private async Task GetWeatherFiveDay(String city)
        {
            var data = await client.GetAsync($"{Settings.WEATHER_FIVE_DAY}?q={city}&units={settings.units.ToString("f")}&APPID={settings.apiKey}");
            if (data.IsSuccessStatusCode)
            {
                var weatherString = await data.Content.ReadAsStringAsync();
                var weatherData = JsonConvert.DeserializeObject<WeatherDataFiveDay>(weatherString);
                if (weatherData != null)
                {
                    WeatherDataFiveDayItem item;
                    var startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var today = DateTime.Today;
                    var now = startTime.AddSeconds(weatherData.list[0].dt);
                    for (int i = 0, len = weatherData.cnt; i < len; i++)
                    {
                        var tomorrow = today.AddDays(1);

                        String type = "";
                        String icon = "";
                        Double tempMin = int.MaxValue;
                        Double tempMax = int.MinValue;
                        Double temp = 0;

                        // Summary for a day
                        do
                        {
                            item = weatherData.list[i];

                            if (item.main.temp_max > tempMax) tempMax = item.main.temp_max;
                            if (item.main.temp_min < tempMin) tempMin = item.main.temp_min;
                            if (now == startTime.AddSeconds(item.dt))
                            {
                                type = item.weather[0].main;
                                icon = item.weather[0].icon;
                                temp = item.main.temp;
                            }
                            i++;
                        } while (startTime.AddSeconds(item.dt) < tomorrow && i < len);

                        if (type.Length == 0)
                        {
                            item = weatherData.list[weatherData.cnt - 1];
                            type = item.weather[0].main;
                            icon = item.weather[0].icon;
                            temp = item.main.temp;
                        }

                        String title;
                        if (results.Count == 0)
                        {
                            title = $"Now, {type}: {temp.ToString("N1")}{GetDegrees()}";
                        }
                        else
                        {
                            title = $"{today.ToString("dddd")}, {type}, {tempMin.ToString("N1")}/{tempMax.ToString("N1")} {GetDegrees()}";
                        }

                        results.Add(new Result
                        {
                            // TODO: Translations
                            Title = title,
                            SubTitle = $"{weatherData.city.name}, {weatherData.city.country}",
                            IcoPath = $"Images/{icon}.png",
                            Score = 300 - i,
                        });

                        today = tomorrow;
                        now = now.AddDays(1);
                    }
                }
            }
        }

        private String GetDegrees()
        {
            switch (settings.units)
            {
                default:
                case Settings.Units.metric:
                    return "°C";
                case Settings.Units.imperial:
                    return "°F";
            }
        }

        private String GetDate(DateTime date)
        {
            switch (settings.units)
            {
                default:
                case Settings.Units.metric:
                    return date.ToString("dd.MM");
                case Settings.Units.imperial:
                    return date.ToString("MM/dd");
            }
        }

        // Todo : Update with theme based IconPath
        private void UpdateIconPath(Theme theme)
        {
            IconPath = "Images/weather.png";
        }

        private void OnThemeChanged(Theme _, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        public string GetTranslatedPluginTitle()
        {
            return Context.API.GetTranslation("wox_plugin_calculator_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return Context.API.GetTranslation("wox_plugin_calculator_plugin_description");
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context.API.ThemeChanged -= OnThemeChanged;
                    _disposed = true;
                }
            }
        }
    }
}