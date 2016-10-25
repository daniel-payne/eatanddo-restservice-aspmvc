using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

namespace VoyageMapper
{
    public class WebApiApplication : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Edit the base address of Service1 by replacing the "Service1" string below
            RouteTable.Routes.Add(new ServiceRoute("rest", new WebServiceHostFactory(), typeof(RestService)));
        }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
      //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

      if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
      {
        //These headers are handling the "pre-flight" OPTIONS call sent by the browser
        //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, DELETE, PUT");
        //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Authorization, Origin, Content-Type, Accept, X-Requested-With, Selection");
        //HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
        HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "432000");
        HttpContext.Current.Response.End();
      }
      //else
      //{
      //  HttpContext.Current.Response.AddHeader("Access-Control-Expose-Headers", "Authorization");
      //}
    }

  }
}
