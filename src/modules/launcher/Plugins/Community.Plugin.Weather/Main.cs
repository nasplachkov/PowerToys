using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Wox.Plugin;

namespace Community.Plugin.Weather
{
    public static class Constants
    { 
        public const int DEBOUNCE_TIMEOUT = 500;
        public const int MIN_SEARCH = 4;
    }

    public class Main : IPlugin, IPluginI18n, IDisposable
    {
        private PluginInitContext Context { get; set; }
        private string IconPath { get; set; }
        private bool _disposed = false;

        private CancellationTokenSource tokenSource;
        private HttpClient client;
        private String weatherUrl;
        private String units;
        private String apiKey;
        private static WeatherData weatherData = null;

        public Main()
        {
            tokenSource = null;
            client = new HttpClient();
            weatherUrl = "http://api.openweathermap.org/data/2.5/weather";
            // TODO: From settings
            units = "metric";
            apiKey = "2d7408c90e93434a6cae832bd5af2670";
        }

        public List<Result> Query(Query query)
        {
            if (query.FirstSearch.ToLower() == "weather" && query.SecondSearch.Length >= Constants.MIN_SEARCH)
            {
                GetWeatherData(query.SecondSearch, new CancellationTokenSource(TimeSpan.FromSeconds(5))).Wait();
                List<Result> results = new List<Result>();
                var weatherData = Main.weatherData;
                if (weatherData != null)
                {
                    results.Add(new Result
                    {
                        Title = weatherData.weather[0]?.main,
                        // TODO: Degrees depending on the units
                        SubTitle = $"{weatherData.main.temp.ToString()}°C",
                        IcoPath = $"Images/{weatherData.weather[0]?.icon}.png",
                        Score = 300,
                    });
                    return results;
                }
            }
            return new List<Result>();
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
            var data = await client.GetAsync($"{weatherUrl}?q={city}&units={units}&APPID={apiKey}");
            if (data.IsSuccessStatusCode)
            {
                var weatherString = await data.Content.ReadAsStringAsync();
                var weatherData = JsonConvert.DeserializeObject<WeatherData>(weatherString);
                Main.weatherData = weatherData;
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