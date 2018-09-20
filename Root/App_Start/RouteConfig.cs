using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StateOfOhioLibrary
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("ChartImg.axd/{*pathInfo}");
            routes.IgnoreRoute("{controller}/ChartImg.axd/{*pathInfo}");
            routes.IgnoreRoute("{controller}/{action}/ChartImg.axd/{*pathInfo}");

            routes.MapRoute(
                  "PageNotFound",
                  "PageNotFound/",
                  new { controller = "Common", action = "PageNotFound" }
              );
            routes.MapRoute(
                "Error",
                "Error/",
                new { controller = "Common", action = "Error" }
            );
            routes.MapRoute(
                "PageUnderConstruction",
                "PageUnderConstruction/",
                new { controller = "Common", action = "PageUnderConstruction" }
            );
            routes.MapRoute(
             "ForgotPassword",
             "ForgotPassword/",
             new { controller = "Login", action = "ForgotPassword" }
         );
            routes.MapRoute(
           "PasswordResetSent",
           "PasswordResetSent/",
           new { controller = "Login", action = "PasswordResetSent" }
           );
            routes.MapRoute(
            "ResetPassword",
            "ResetPassword/",
            new { controller = "Login", action = "ResetPassword" }
             );

            routes.MapRoute(
            "ChangePassword",
            "ChangePassword/",
            new { controller = "Login", action = "ChangePassword" }
             );
            routes.MapRoute(
             "Custom",
             "Custom/",
             new { controller = "Custom", action = "Index" }
            );
            routes.MapRoute(
            "Custom/id",
            "Custom/id",
            new { controller = "Custom", action = "DownloadPdf", Year = UrlParameter.Optional, customColumnIdField1 = UrlParameter.Optional, customColumnIdField2 = UrlParameter.Optional, customColumnIdField3 = UrlParameter.Optional, customColumnIdField4 = UrlParameter.Optional, customColumnIdField5 = UrlParameter.Optional, customColumnIdField6 = UrlParameter.Optional }
            );
            routes.MapRoute(
            "Template/CustomTemplate",
            "template/CustomTemplate/",
            new { controller = "CustomTemplate", action = "Index" }
           );
            routes.MapRoute(
            "PasswordChanged",
            "PasswordChanged/",
             new { controller = "Login", action = "PasswordChanged" }
           );
            routes.MapRoute(
             name: "Default",
             url: "{controller}/{action}/{id}",
             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
