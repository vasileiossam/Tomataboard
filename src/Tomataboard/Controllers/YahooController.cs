﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Tomataboard.Services;
using Tomataboard.Services.AccessTokens;
using Tomataboard.Services.Locations;
using Tomataboard.Services.Weather.Yahoo;

namespace Tomataboard.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class YahooController : OauthController
    {
        public YahooController(
            ILogger<YahooWeatherService> logger,
            IYahooWeatherService service,
            ICookiesService<OauthToken> cookiesService,
            IAccessTokensRepository accessTokensRepository) : base(logger, service, cookiesService, accessTokensRepository)
        {
            _cookieKey = "YahooRequestToken";
        }

        public override async Task<ActionResult> Execute()
        {
            try
            {
                var melbourneLocation = new Location()
                {
                    City = "Melbournze",
                    CountryCode = "AUS"
                };
                var serializedMelbourne = JsonConvert.SerializeObject(melbourneLocation);

                var service = _service as YahooWeatherService;
                if (service != null)
                {
                    return View(await service.Execute(serializedMelbourne));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("Error");
            }
            return View("Error");
        }

        public override async Task<ActionResult> Refresh()
        {
            try
            {
                var accessToken = _accessTokensRepository.Find(_service.GetType().Name);
                if (accessToken != null)
                {
                    var service = _service as YahooWeatherService;
                    var newAccessToken = await service.RefreshAccessToken(accessToken);
                    if (newAccessToken != null)
                    {
                        _accessTokensRepository.Add(_service.GetType().Name, newAccessToken);
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View("Error");
            }
        }
    }
}