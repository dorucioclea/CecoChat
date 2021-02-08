﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CecoChat.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CecoChat.Server.Profile.Security
{
    [ApiController]
    [Route("api/session")]
    public sealed class SessionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IJwtOptions _jwtOptions;
        private readonly IClock _clock;

        private readonly SigningCredentials _signingCredentials;
        private readonly JwtSecurityTokenHandler _jwtTokenHandler;

        public SessionController(
            ILogger<SessionController> logger,
            IOptions<JwtOptions> jwtOptions,
            IClock clock)
        {
            _logger = logger;
            _jwtOptions = jwtOptions.Value;
            _clock = clock;

            byte[] secret = Encoding.UTF8.GetBytes(_jwtOptions.Secret);
            _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature);
            _jwtTokenHandler = new();
            _jwtTokenHandler.OutboundClaimTypeMap.Clear();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateSession([FromBody] CreateSessionRequest request)
        {
            Claim[] claims =
            {
                new(JwtRegisteredClaimNames.Sub, "1", ClaimValueTypes.Integer64),
                new(ClaimTypes.Role, "user")
            };
            string accessToken = CreateAccessToken(claims);
            _logger.LogInformation("User {0} authenticated and assigned user ID 1.", request.Username);

            CreateSessionResponse response = new() {AccessToken = accessToken};
            return Ok(response);
        }

        private string CreateAccessToken(Claim[] claims)
        {
            DateTime expiration = _clock.GetNowUtc().Add(_jwtOptions.AccessTokenExpiration);

            JwtSecurityToken jwtToken = new(_jwtOptions.Issuer, _jwtOptions.Audience, claims, null, expiration, _signingCredentials);
            string accessToken = _jwtTokenHandler.WriteToken(jwtToken);

            return accessToken;
        }
    }
}