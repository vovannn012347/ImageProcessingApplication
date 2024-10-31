using System;
using System.Collections.Generic;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using Microsoft.Owin.Security.DataHandler.Encoder;

namespace ImageProcessingApplication.Code
{
    public static class TokenHelper
    {
        public static string GenerateAnonymousToken(string ipAddress)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Constants.AnonymousUserConstant),
                new Claim(ClaimTypes.Name, Constants.AnonymousUserConstant),
                new Claim(Constants.ClaimTypes.IpAddress, ipAddress)
            };

            string siteName = ConfigurationManager.AppSettings[Constants.AppSettingKeys.Site];
            string jwtsecret = ConfigurationManager.AppSettings[Constants.AppSettingKeys.JwtSecret];
            var secret = TextEncodings.Base64Url.Decode(jwtsecret);

            var key = new SymmetricSecurityKey(secret);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: siteName,
                audience: Constants.ClientTypes.Any,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateAuthenticatedToken(string userId, string username, string ipAddress)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(Constants.ClaimTypes.IpAddress, ipAddress)
            };

            string siteName = ConfigurationManager.AppSettings[Constants.AppSettingKeys.Site];
            string jwtsecret = ConfigurationManager.AppSettings[Constants.AppSettingKeys.JwtSecret];
            var secret = TextEncodings.Base64Url.Decode(jwtsecret);

            var key = new SymmetricSecurityKey(secret);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: siteName,
                audience: Constants.ClientTypes.Any,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}