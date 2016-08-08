using System;
using System.CodeDom;
using System.Net;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System.Threading.Tasks;
using HunieBot.WeatherForecast.Api;
using Newtonsoft.Json;

namespace HunieBot.WeatherForecast
{
    [HunieBot(nameof(WeatherForecast))]
    public class WeatherForecast
    {

        private const string ApiKey = "[insert api key]";
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

