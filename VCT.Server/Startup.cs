using System;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using VCT.Server.Helpers;
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
				RequestKeys = new[] {"owin.RequestPath", "owin.RequestMethod"},
				ResponseKeys = new[] {"owin.ResponseStatusCode", "owin.ResponseBody"}
			};
			var fsOptions = new FileServerOptions
			{
				EnableDefaultFiles = true,
				FileSystem = new PhysicalFileSystem(Config.AppSettings["rootDir"]),
			};
			fsOptions.DefaultFilesOptions.DefaultFileNames = new[] {"index.html"};

			appBuilder.Map("/api", apiBuilder =>
			{
				apiBuilder.Use((ctx, next) => next());
				apiBuilder.UseWebApi(config);
			});

			appBuilder.Use(async (ctx, next) =>
			{
				//BAD BAD way to redirect all not api calls to index.html
				//TODO find another
				var path = ctx.Request.Path.ToString();
				if (!path.StartsWith("/bundle.js") && !path.StartsWith("/images") && !path.StartsWith("/Storage"))
				{
					ctx.Request.Path = new PathString("/index.html");
				}
				await next();
			});
			appBuilder.UseFileServer(fsOptions);

			appBuilder.Use<SimpleLogger>(options);
		}
	}
}