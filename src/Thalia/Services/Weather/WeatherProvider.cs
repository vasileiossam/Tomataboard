﻿using Microsoft.Extensions.Logging;
using Thalia.Services.Cache;
using Thalia.Services.Weather.OpenWeatherMap;
using Thalia.Services.Weather.Yahoo;

namespace Thalia.Services.Weather
{
    public class WeatherProvider : Provider<WeatherConditions>, IWeatherProvider
    {
        public WeatherProvider(ILogger logger, ICacheRepository<WeatherConditions> cacheRepository,
            IOpenWeatherMapService openWeatherMapService,
            IYahooWeatherService yahooWeatherService)
            : base(logger, cacheRepository)
        {
            _operations.Add(openWeatherMapService);
            _operations.Add(yahooWeatherService);
        }
    }
}
