using System;
using System.CodeDom;
using System.Net;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Threading.Tasks;
using HunieBot.WeatherForecast.Response;
using Newtonsoft.Json;

namespace HunieBot.WeatherForecast
{
    [HunieBot(nameof(WeatherForecast))]
    public class WeatherForecast
    {

        private const string ApiKey = "get your own key!";
        private string ApiUrl = $"http://api.openweathermap.org/data/2.5/weather?APPID={ApiKey}&q={{0}}";

        [HandleCommand(CommandEvent.CommandReceived | CommandEvent.AnyMessageReceived, UserPermissions.User, true, commands: new[] { "w", "weather" })]
        public async Task HandleCommand(IHunieCommand command, ILogging logger)
        {
            try
            {
                var webClient = new WebClient();
                var requestUrl = string.Format(ApiUrl, string.Join(" ", command.ParametersArray));
                var request = webClient.DownloadString(requestUrl);
                //logger.Trace($"Got weather info: \n{request}");
                var deserializedRequest = JsonConvert.DeserializeObject<OpenWeatherMapResponse>(request);

                var messageTitle = $"{deserializedRequest.GetWeatherEmoji} __**Weather** for {deserializedRequest.name}__";
                var messageTemp =
                    $"{deserializedRequest.main.tempInCelcius:N2}°C / {deserializedRequest.main.tempInFahrenheit:N2}°F, '{deserializedRequest.weather[0].description}'";
                var messageCloud = $"{deserializedRequest.clouds.all}% Clouds, Windspeed {deserializedRequest.wind.speed}m/s";
                var messagePressure =
                    $"Barometric pressure: {deserializedRequest.main.pressure}hpa @{deserializedRequest.main.humidity}% humidity";

                var fullMessage = $"{messageTitle}\n{messageTemp}\n{messageCloud}\n{messagePressure}\n{command.User.Mention}";

                await command.Channel.SendMessage(fullMessage);
            }
            catch (Exception e)
            {
                logger.Debug($"WeatherForecast: {e.Message}");
                await command.Channel.SendMessage($"WeatherForecast ran into an error: {e.Message}");
            }
        }

    }
}

// im storing everything as strings because i dont plan on actually doing anything with this information outside of echoing it to the channel in a certain format
namespace HunieBot.WeatherForecast.Response
{
    public class OpenWeatherMapResponse
    {
        public coord coord { get; set; }
        public weather[] weather { get; set; }
        [JsonProperty("base")]
        public string Base { get; set; }
        public main main { get; set; }
        public wind wind { get; set; }
        public clouds clouds { get; set; }
        public rain rain { get; set; }
        public snow snow { get; set; }
        public string dt { get; set; }
        public sys sys { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string cod { get; set; }

        public string GetWeatherEmoji
        {
            get
            {
                switch (weather[0].icon)
                {
                    case "01d": // clear sky day
                        return ":sunny:";
                    case "01n": // clear sky night
                        return ":waxing_gibbous_moon:";
                    case "02d": // few clouds day
                        return ":white_sun_small_cloud:";
                    case "02n": // few clouds night
                        return ":cloud:";
                    case "03d": // scattered clouds day
                        return ":white_sun_small_cloud:";
                    case "03n": // scattered clouds night
                        return ":cloud:";
                    case "04d": // broken clouds day
                        return ":white_sun_small_cloud:";
                    case "04n": // broken clouds night
                        return ":cloud:";
                    case "09d": // shower rain day
                        return ":white_sun_rain_cloud:";
                    case "09n": // shower rain night
                        return ":cloud_rain:";
                    case "10d": // rain day
                        return ":umbrella:";
                    case "10n": // rain night
                        return ":umbrella:";
                    case "11d": // thunderstorm day
                        return ":thunder_cloud_rain:";
                    case "11n": // thunderstorm night
                        return ":thunder_cloud_rain:";
                    case "13d": // snow day
                        return ":snowflake:";
                    case "13n": // snow night
                        return ":snowflake:";
                    case "50d": // mist day
                        return ":sweat_drops:";
                    case "50n": // mist night
                        return ":sweat_drops:";
                    default:
                        return "";
                }
            }
        }
    }

    public class coord
    {
        public string lon { get; set; }
        public string lat { get; set; }
    }

    public class weather
    {
        public string id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class main
    {
        public double temp { get; set; }
        public string pressure { get; set; }
        public string humidity { get; set; }
        public string temp_min { get; set; }
        public string temp_max { get; set; }
        public string sea_level { get; set; }
        public string grnd_level { get; set; }

        public double tempInFahrenheit => temp * 9 / 5 - 459.67;
        public double tempInCelcius => temp - 273.15;
    }

    public class wind
    {
        public string speed { get; set; }
        public string deg { get; set; }
    }

    public class clouds
    {
        public string all { get; set; }
    }

    public class rain
    {
        [JsonProperty("3h")]
        public string vol { get; set; }
    }

    public class snow
    {
        [JsonProperty("3h")]
        public string vol { get; set; }
    }

    public class sys
    {
        public string type { get; set; }
        public string id { get; set; }
        public string message { get; set; }
        public string country { get; set; }
        public string sunrise { get; set; }
        public string sunset { get; set; }
    }
}