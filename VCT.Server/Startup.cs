using System;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	public class Startup
	{
		// This code configures Web API. The Startup class is specified as a type
		// parameter in the WebApp.Start method.
		public void Configuration(IAppBuilder appBuilder)
		{
			// Configure Web API for self-host. 
			var config = new HttpConfiguration();

			config.MapHttpAttributeRoutes();
			
			var options = new SimpleLoggerOptions
			{
				Log = (key, value) => Console.WriteLine("{0}:{1}", key, value),
				RequestKeys = new[] { "owin.RequestPath", "owin.RequestMethod" },
				ResponseKeys = new[] { "owin.ResponseStatusCode", "owin.ResponseBody" }
			};
			var fsOptions = new FileServerOptions
			{
				EnableDefaultFiles = true,
				FileSystem = new PhysicalFileSystem(Config.AppSettings["rootDir"]),
				RequestPath = new PathString("")
			};
			fsOptions.DefaultFilesOptions.DefaultFileNames = new[] {"react_index.html"};

			appBuilder.UseFileServer(fsOptions);
			appBuilder.UseWebApi(config);
			appBuilder.UseSimpleLogger(options);
		}
	}
}