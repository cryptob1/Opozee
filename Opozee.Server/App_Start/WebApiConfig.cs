using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using opozee.Library.API;
using System.Web.Configuration;
using opozee.Controllers.API;

using Microsoft.Owin.Security.OAuth;


namespace opozee
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configure Web API to use only bearer token authentication. 
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            
            // Web API configuration and services
            config.EnableCors();
            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
            //config.Filters.Add(new BasicAuthenticationAttribute());

            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["APILogger"].ToString()) == true)
            {
                config.MessageHandlers.Add(new LoggingHandler());
            }

            //API Defalte Compression
            if (Convert.ToBoolean(WebConfigurationManager.AppSettings["APIDefalteCompression"].ToString()) == true)
            {
                config.Filters.Add(new DeflateCompressionAttribute());
            }
        }
    }
}
