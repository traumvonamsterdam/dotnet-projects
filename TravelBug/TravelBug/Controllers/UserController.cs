﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TravelBug.Infrastructure;
using TravelBug.Entities.UserData;
using TravelBug.Infrastructure.UserLogic;
using static TravelBug.Infrastructure.ExternalLoginService;
using System.Net.Http;
using System;
using TravelBug.Infrastructure.Exceptions;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace TravelBug.Web.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoginService _loginService;
        private readonly IRegisterService _registerService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IExternalLoginService _externalLogin;
        private readonly IEmailConfirmation _emailConfirmation;
        private readonly IConfiguration _config;

        public UserController(IHttpClientFactory httpClientFactory, ILoginService loginService, IRegisterService registerService, IRefreshTokenService refreshTokenService, IExternalLoginService externalLogin, IEmailConfirmation emailConfirmation, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _loginService = loginService;
            _registerService = registerService;
            _refreshTokenService = refreshTokenService;
            _externalLogin = externalLogin;
            _emailConfirmation = emailConfirmation;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<User> Login(LoginInput loginInput)
        {
            return await _loginService.Login(loginInput);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task Register(RegisterInput registerInput)
        {
            var user = await _registerService.CreateNewUser(registerInput);

            var emailToken = _registerService.GenerateEmailToken(user);

            var emailVerificationUrl = $"{registerInput.Origin}/user/verify-email?token={emailToken}&email={registerInput.Email}";

            await _registerService.SendEmail(registerInput.Email, emailVerificationUrl);

            await _registerService.RegisterUser(user, registerInput.Password);
        }

        [AllowAnonymous]
        [HttpGet("verify-email")]
        public async Task VerifyEmail(VerifyEmailInput verifyEmailInput)
        {
            await _registerService.VerifyEmail(verifyEmailInput);
        }

        [AllowAnonymous]
        [HttpGet("resend-email-verification")]
        public async Task ResendEmailVerification([FromQuery] ResendEmailInput input)
        {
            await _emailConfirmation.ResendEmail(input);
        }

        [AllowAnonymous]
        [HttpPost("facebook-login")]
        public async Task<ActionResult<User>> FacebookLogin(UserData request)
        {
            // Verify facebook token
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://graph.facebook.com/");

            var appId = _config["Facebook:AppId"];
            var appSecret = _config["Facebook:AppSecret"];

            var response = await httpClient.GetAsync($"debug_token?input_token={request.AccessToken}&access_token={appId}|{appSecret}");

            if (!response.IsSuccessStatusCode)
                throw new RestException(HttpStatusCode.BadRequest, new { User = "Problem validating token" });

            // Save user data
            return await _externalLogin.SaveUser(request, "facebook");
        }


        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<ActionResult<User>> GoogleLogin(UserData request)
        {
            // Verify google token
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://www.googleapis.com/oauth2/v3/");

            var response = await httpClient.GetAsync($"tokeninfo?access_token={request.AccessToken}");

            if (!response.IsSuccessStatusCode)
                throw new RestException(HttpStatusCode.BadRequest, new { User = "Problem validating token" });

            // Save user data
            return await _externalLogin.SaveUser(request, "google");
        }

        [HttpPost("refreshToken")]
        public async Task<User> RefreshToken(string refreshToken)
        {
            return await _refreshTokenService.GetRefreshToken(refreshToken);
        }
    }
}
