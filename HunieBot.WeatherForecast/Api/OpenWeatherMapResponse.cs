using HunieBot.WeatherForecast.Api.Response;
using Newtonsoft.Json;

namespace HunieBot.WeatherForecast.Api
{
    public class OpenWeatherMapResponse
    {
        public coord coord { get; set; }
        public weather[] weather { get; set; }

        /// <summary>
        /// Internal parameter
        /// </summary>
        [JsonProperty("base")]
        public string Base { get; set; }

        public main main { get; set; }
        public wind wind { get; set; }
        public clouds clouds { get; set; }
        public rain rain { get; set; }
        public snow snow { get; set; }

        /// <summary>
        /// Time of data calculation, unix, UTC
        /// </summary>
        public string dt { get; set; }

        public sys sys { get; set; }

        /// <summary>
        /// City ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// City name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Internal parameter
        /// </summary>
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

    
}

namespace HunieBot.WeatherForecast.Api.Response
{
    public class coord
    {
        /// <summary>
        /// City geo location, longitude
        /// </summary>
        public string lon { get; set; }

        /// <summary>
        /// City geo location, latitude
        /// </summary>
        public string lat { get; set; }
    }

    /// <summary>
    /// more info Weather condition codes
    /// </summary>
    public class weather
    {
        /// <summary>
        /// Weather condition id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Group of weather parameters (Rain, Snow, Extreme etc.)
        /// </summary>
        public string main { get; set; }

        /// <summary>
        /// Weather condition within the group
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Weather icon id
        /// </summary>
        public string icon { get; set; }
    }

    public class main
    {
        /// <summary>
        /// Temperature. 
        /// Unit Default: Kelvin, 
        /// Metric: Celsius, 
        /// Imperial: Fahrenheit.
        /// </summary>
        public double temp { get; set; }

        /// <summary>
        /// Atmospheric pressure (on the sea level, if there is no sea_level or grnd_level data), hPa
        /// </summary>
        public string pressure { get; set; }

        /// <summary>
        /// Humidity, %
        /// </summary>
        public string humidity { get; set; }

        /// <summary>
        /// Minimum temperature at the moment. This is deviation from current temp that is possible for large cities and megalopolises geographically expanded (use these parameter optionally). 
        /// Unit Default: Kelvin, 
        /// Metric: Celsius, 
        /// Imperial: Fahrenheit.
        /// </summary>
        public string temp_min { get; set; }

        /// <summary>
        /// Maximum temperature at the moment. This is deviation from current temp that is possible for large cities and megalopolises geographically expanded (use these parameter optionally). 
        /// Unit Default: Kelvin, 
        /// Metric: Celsius, 
        /// Imperial: Fahrenheit.
        /// </summary>
        public string temp_max { get; set; }

        /// <summary>
        /// Atmospheric pressure on the sea level, hPa
        /// </summary>
        public string sea_level { get; set; }

        /// <summary>
        /// Atmospheric pressure on the ground level, hPa
        /// </summary>
        public string grnd_level { get; set; }

        public double tempInFahrenheit => temp * 9 / 5 - 459.67;
        public double tempInCelcius => temp - 273.15;
    }

    public class wind
    {
        /// <summary>
        /// Wind speed. 
        /// Unit Default: meter/sec, 
        /// Metric: meter/sec, 
        /// Imperial: miles/hour.
        /// </summary>
        public string speed { get; set; }

        /// <summary>
        /// Wind direction, degrees (meteorological)
        /// </summary>
        public string deg { get; set; }
    }

    public class clouds
    {
        /// <summary>
        /// Cloudiness, %
        /// </summary>
        public string all { get; set; }
    }

    public class rain
    {
        /// <summary>
        /// Rain volume for the last 3 hours
        /// </summary>
        [JsonProperty("3h")]
        public string vol { get; set; }
    }

    public class snow
    {
        /// <summary>
        /// Snow volume for the last 3 hours
        /// </summary>
        [JsonProperty("3h")]
        public string vol { get; set; }
    }

    public class sys
    {
        /// <summary>
        /// Internal parameter
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Internal parameter
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Internal parameter
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Country code (GB, JP etc.)
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// Sunrise time, unix, UTC
        /// </summary>
        public string sunrise { get; set; }

        /// <summary>
        /// Sunset time, unix, UTC
        /// </summary>
        public string sunset { get; set; }
    }
}
