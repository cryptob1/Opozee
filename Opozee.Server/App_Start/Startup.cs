using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using opozee;
using Owin;
using System;
using System.Web.Http;

[assembly: OwinStartup(typeof(Opozee.Server.App_Start.Startup))]
namespace Opozee.Server.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/OpozeeGrantResourceOwnerCredentialSecret"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(168), //1Week=168h => 168*60=10080 Minutes
                Provider = new AuthorizationServerProvider(),
                RefreshTokenProvider = new RefreshTokenProvider()
            };
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }
}