﻿/* 
 * File Author: Ciorba Bogdan
 * Last Modified: 23.03.2021 18:38
**/ 


using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WeatherAPI
{

    public interface IWeatherService
    {
        Task<DataClass> GetWeatherData(string cityName, string stateCode);
    }

    public class WeatherService
    {
        private HttpClient httpClient;

        public WeatherService()
        {
            //TODO instantiate httpClient

        }
        async Task<DataClass> GetWeatherData(string cityName, string stateCode)
        {
            string baseUrl = "";
            GenerateUrl(ref baseUrl);

            //TODO create uri
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);


            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                // perhaps check some headers before deserialising

                try
                {
                    return await response.Content.ReadFromJsonAsync<DataClass>();
                }
                catch (NotSupportedException) // When content type is not valid
                {
                    Console.WriteLine("The content type is not supported.");
                }
                catch (JsonException) // Invalid JSON
                {
                    Console.WriteLine("Invalid JSON.");
                }
            }

            return null;

        }

        /* Fetching API Key, Generating baseUrl */
        private static void GenerateUrl(ref string baseUrl)
        {
            /* fetching data from appsettings.json */
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            var section = config.GetSection(nameof(WeatherClientConfig));
            var weatherClientConfig = section.Get<WeatherClientConfig>();

            /* generating baseUrl */

            baseUrl = weatherClientConfig.WeatherAPIUrl + WeatherGeneratorFromAPI.CityName + "," + WeatherGeneratorFromAPI.StateCode + weatherClientConfig.Options + weatherClientConfig.apiKey;
        }


    }
    public class WeatherGeneratorFromAPI
    {
        /* generating cityname and statecode as class variables for later use */
        public static string CityName;
        public static string StateCode;
        public static bool inputIsCorrect = true;

        /* generating WeatherData as new WeatherForecast object */
        private static WeatherForecast WeatherData = new WeatherForecast();
      


        /* Fetches string from url and returns value as string*/
        private static async Task<string> Fetch(string url)
        {
            /* fetching string from url */
            using var client = new HttpClient();
            var content = await client.GetStringAsync(url);

            /* returning value as string */
            return content;
        }

        /* Prints the output of the data object as a pretty-printed json */
        private static async Task PrintData (string json)
        {
            /** The commented code uses System.Text.Json to create
             *  a serialized .JSON file, however it does not 
             *  pretty-print and replaces the '\n' character with
             *  '\u0020'                                          **/
            //JsonSerializerOptions options = new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //};
            string path = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            string fileName = Path.Combine(path, "apidata.json");
            using (StreamWriter file = File.CreateText(fileName))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(file, json);
            }
            //using FileStream createstream = File.Create(fileName);
            //await System.Text.Json.JsonSerializer.SerializeAsync(createstream, json, options);
        }

        /* Transforming from data object to WeatherData object */
        private static void ExtractWeatherData(dynamic data)
        {
            
            WeatherData.temp = data.main.temp;
            WeatherData.winddegrees = data.wind.deg;
            WeatherData.weather = data.weather[0].main;
        }

        /* Method for the messages */
        public static void WorkWeatherData()
        {
            /* formatting helper, just shows City Name and weather */
            Console.WriteLine("\n"+CityName + " weather:");

            /* prints temperature in fahrenheit and celsius */
            DetermineTemperature(WeatherData.temp);

            /** input: wind degrees, wind direction
             *  returns wind direction as string
             *  output: WeatherData.winddirection **/
            string winddirection ="";
            CalculateWindDirection(WeatherData.winddegrees, ref winddirection);


            Console.WriteLine("The wind is currently blowing {0}.\n", winddirection);

            /* based on the temperature and weather condition, sends a message that it is a nice day outside */
            DetermineNiceDay(WeatherData.temp, WeatherData.weather);

            /* based on the temperature, sends a message that the user should use a coat */
            DetermineCoat(WeatherData.temp);

            /* based on the value of the weather field, sends a message that the user should bring an umbrella */
            DetermineUmbrella(WeatherData.weather);

            /* based on the value of the weather field, sends a message that it is cloudy */
            DetermineClouds(WeatherData.weather);
        }

        /* Prints out a message with the temperature in Fahrenheit and Celsius */
        private static void DetermineTemperature(double temperature)
        {
            /* conversion to celsius */
            double tempcelsius = (temperature - 32) * 5 / 9;
            tempcelsius = (double)System.Math.Round(tempcelsius, 2);
            /* degrees fahrenheit + celsius print */
            Console.WriteLine("It's currently " + temperature + " degrees Fahrenheit.\nThat's " + (tempcelsius) + " degrees Celsius.\n");
        }

        /* Sets a threshold for a nice day and then sends a message if the conditions meet the threshold */
        private static void DetermineNiceDay(double temperature, string weather)
        {
            /* threshold for nice day message set as variable */
            string nicedaythreshold = "Clear Sky";

            if (temperature > 60 && weather == nicedaythreshold)
            {
                Console.WriteLine("It's a nice day outside.\n");
            }
        }

        /* Defining what bad weather is for umbrella message */
        private static void DetermineUmbrella (string weather)
        {
            string[] badweather = { "snow", "rain", "extreme" };
            if (weather.ToLower() == badweather[0] || weather.ToLower() == badweather[1] || weather.ToLower() == badweather[2])
            {
                Console.WriteLine("You should bring an umbrella.\n");
            }
        }

        /* Defining what clouds are for cloudy message */
        private static void DetermineClouds (string weather)
        {
            string cloudthreshold = "clouds";
            if (weather.ToLower() == cloudthreshold)
            {
                Console.WriteLine("It's cloudy outside.\n");
            }
        }

        /* Defines what coat threshold is for coat message */
        private static void DetermineCoat (double temperature)
        {
            int coatthreshold = 60;
            if (temperature < coatthreshold)
            {
                Console.WriteLine("You should bring a coat.\n");
            }
        }

        /* Checks which way the wind blows by comparing which part of the circle the orientation is */
        private static void CalculateWindDirection(int winddegrees, ref string winddirection)
        {
            if (winddegrees == 0)
            {
                /* |^ */
                winddirection = "North";
            }
            else if (winddegrees > 0 && winddegrees < 90)
            {
                /* /^ */
                winddirection = "North-East";
            }
            else if (winddegrees == 90)
            {
                /* -> */
                winddirection = "East";
            }
            else if (winddegrees > 90 && winddegrees < 180)
            {
                /* \v */
                winddirection = "South-East";
            }
            else if (winddegrees == 180)
            {
                /* |v */
                winddirection = "South";
            }
            else if (winddegrees > 180 && winddegrees < 270)
            {
                /* /v */
                winddirection = "South-West";
            }
            else if (winddegrees == 270)
            {
                /* <- */
                winddirection = "West";
            }
            else if (winddegrees > 270 && winddegrees < 360)
            {
                /* \^ */
                winddirection = "North-West";
            }
            else if (winddegrees == 360)
            {
                /* |^ */
                winddirection = "North";
            }
                /* in case of the wind value somehow being above 360 degrees */
            else winddirection = "Invalid wind direction data";
        }
    }
}
