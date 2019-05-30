using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using opozee.Enums;
using Opozee.Server.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Opozee.Server
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated(); // 
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {

            // Change authentication ticket for refresh token requests  
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            AuthService auth = new AuthService();

            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            context.Validated(identity);

            /*commented for now*/

            //var _userInfo = auth.Login(context.UserName, context.Password);

            //Authenticate the user credentials
            //if (_userInfo.status)
            //{
            //    var _user = _userInfo.user;
            //    identity.AddClaim(new Claim(ClaimTypes.Email, _user.Email));
            //    identity.AddClaim(new Claim(ClaimTypes.Name, _user.UserName));
            //    context.Validated(identity);

            //    //var props = new AuthenticationProperties(new Dictionary<string, string>
            //    //    {
            //    //         { "Id", _user.Id.ToString() },
            //    //         { "Email", _user.Email},
            //    //         { "Password", "" },
            //    //         { "Token", _user.Token == null ? "" :  _user.Token},
            //    //         { "ImageURL", _user.ImageURL == null ? "" :  _user.ImageURL },
            //    //         { "BalanceToken", _user.BalanceToken.ToString() },
            //    //         { "LastLoginDate", _user.LastLoginDate == null ? "" :_user.LastLoginDate.ToString() },
            //    //         { "ReferralCode", _user.ReferralCode == null ? "" :_user.ReferralCode.ToString() },
            //    //         { "TotalReferred", _user.TotalReferred.ToString() },
            //    //         { "IsSocialLogin", _user.IsSocialLogin.ToString() }

            //    //    });

            //    //var ticket = new AuthenticationTicket(identity, props);
            //    //context.Validated(ticket);

            //}
            ////else if (_userInfo.status == STATUS.EMAIL_NOT_VERIFIED)
            ////{
            ////    context.SetError("invalid_grant", "Provided username and password is incorrect");
            ////    return;
            ////}
            ////else if (_userInfo.status == STATUS.WRONG_PASSWORD)
            ////{
            ////    context.SetError("invalid_grant", "Provided password is incorrect");
            ////    return;
            ////}
            ////else if (_userInfo.status == STATUS.NOT_EXIST)
            ////{
            ////    context.SetError("invalid_grant", "Provided username and password is incorrect");
            ////    return;
            ////}
            //else
            //{
            //    context.SetError("invalid_grant", "Provided username and password is incorrect");
            //    return;
            //}
        }
    }


    //public override Task TokenEndpoint(OAuthTokenEndpointContext context)
    //{
    //    foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
    //    {
    //        context.AdditionalResponseParameters.Add(property.Key, property.Value);
    //    }

    //    return Task.FromResult<object>(null);
    //}


    public class RefreshTokenProvider : IAuthenticationTokenProvider
    {
        private static ConcurrentDictionary<string, AuthenticationTicket> _refreshTokens = new ConcurrentDictionary<string, AuthenticationTicket>();

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {

            var guid = Guid.NewGuid().ToString();

            // copy all properties and set the desired lifetime of refresh token  
            var refreshTokenProperties = new AuthenticationProperties(context.Ticket.Properties.Dictionary)
            {
                IssuedUtc = context.Ticket.Properties.IssuedUtc,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
            };

            var refreshTokenTicket = new AuthenticationTicket(context.Ticket.Identity, refreshTokenProperties);

            _refreshTokens.TryAdd(guid, refreshTokenTicket);

            // consider storing only the hash of the handle  
            context.SetToken(guid);
        }


        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            // context.DeserializeTicket(context.Token);
            AuthenticationTicket ticket;
            string header = context.OwinContext.Request.Headers["Authorization"];

            if (_refreshTokens.TryRemove(context.Token, out ticket))
            {
                context.SetTicket(ticket);
            }
        }
    }
}