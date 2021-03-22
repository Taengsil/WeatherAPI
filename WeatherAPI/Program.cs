﻿using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherAPI
{
        public class WeatherForecast
        {
            public int temp { get; set; }
            public int winddegrees { get; set; }
            public string winddirection { get; set; }
            public string weather { get; set; }
        }
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Enter a city name and a state code:");
            // In the e-mail, the input is in one line. However, the user could type one line individually. The following will
            // check if the string is inputted in one go ("Chicago, IL") and split it into the correct variables, or if the user
            // inserts them separately, the program will pick up the second line correctly.
            string CityName = Console.ReadLine();
            if (CityName.Contains(' '))
            {
                string[] word = CityName.Split(' ');
                CityName = word[0];
                string StateName = word[1];
                string StateCode = GetStateCode(StateName);
                await GetWeather(CityName, StateCode);
            }
            else
            {
                string StateName = Console.ReadLine();
                string StateCode = GetStateCode(StateName);
                await GetWeather(CityName, StateCode);
            }
        }
        public static string GetStateCode(string StateName)
        {
            StateName = StateName.ToUpper();
            string StateCode = "US-" + StateName;
            return StateCode;
        }
        public static async Task GetWeather(string CityName, string StateCode)
        {
            string apiKey = "b54f94bdd520de56cb775654e9f83954";
            string startUrl = "http://api.openweathermap.org/data/2.5/weather?q=";
            string baseUrl = startUrl + CityName + "," + StateCode + "&units=imperial&appid=" + apiKey;
            try
            {
                using (WebClient client = new WebClient())
                {
                    // Downloading the JSON string to var json, then deserializing it as a dynamic object in var data;
                    var json = client.DownloadString(baseUrl);
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    //if there is data, print it
                    if (data != null)
                    {
                        WeatherForecast WeatherData = new WeatherForecast();
                        WeatherData.temp = data.main.temp;
                        WeatherData.winddegrees = data.wind.deg;
                        WeatherData.weather = data.weather[0].main;
                        WorkWeatherData(CityName, WeatherData);
                    }
                    //else, if there is no data, print "no data"
                    else
                    {
                        Console.WriteLine("No data");
                    }
                }
                    
              

            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception Hit:");
                Console.WriteLine(exception);
            }
        }
        public static void WorkWeatherData(string CityName, WeatherForecast WeatherData)
        {
            Console.WriteLine("\n"+CityName + " weather:");

            //conversion to celsius
            float tempcelsius = (WeatherData.temp - 32) * 5 / 9;
            //degrees fahrenheit + celsius print
            Console.WriteLine("It's currently " + WeatherData.temp + " degrees Fahrenheit out there.\nThat's " + tempcelsius + " degrees Celsius.\n");
            //returns wind direction as string
            WeatherData.winddirection = GetWindDirection(WeatherData.winddegrees);
            Console.WriteLine("The wind is currently blowing {0}.\n", WeatherData.winddirection);
            //weather schemes, umbrella determination, etc.
            if (WeatherData.temp > 60 && WeatherData.weather == "Clear Sky")
            {
                Console.WriteLine("It's a nice day outside.\n");
            }
            else if (WeatherData.temp < 60)
            {
                Console.WriteLine("You should bring a coat.\n");
            }
            if (WeatherData.weather.ToLower() == "rain" || WeatherData.weather.ToLower() == "snow" || WeatherData.weather.ToLower() == "extreme")
            {
                Console.WriteLine("You should bring an umbrella.\n");
            }
            if (WeatherData.weather.ToLower() == "clouds")
            {
                Console.WriteLine("It's cloudy outside.\n");
            }
        }
        public static string GetWindDirection(int winddegrees)
        {
            if (winddegrees == 0)
            {
                // |^
                return "North";
            }
            else if (winddegrees > 0 && winddegrees < 90)
            {
                // /^
                return "North-East";
            }
            else if (winddegrees == 90)
            {
                // ->
                return "East";
            }
            else if (winddegrees > 90 && winddegrees < 180)
            {
                // \v
                return "South-East";
            }
            else if (winddegrees == 180)
            {
                // |v
                return "South";
            }
            else if (winddegrees > 180 && winddegrees < 270)
            {
                // /v
                return "South-West";
            }
            else if (winddegrees == 270)
            {
                // <-
                return "West";
            }
            else if (winddegrees > 270 && winddegrees < 360)
            {
                // \^
                return "North-West";
            }
            else if (winddegrees == 360)
            {
                // |^
                return "North";
            }
            return "Invalid wind direction data";
        }
    }
}
