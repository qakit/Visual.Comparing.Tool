using System;
using Microsoft.Owin.Hosting;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	class Program
	{
		static readonly string BaseAddress = Config.AppSettings["rootUrl"];

		static void Main(string[] args)
		{
			WebApp.Start<Startup>(BaseAddress);
			Console.WriteLine("Server started and waiting");
			Console.ReadLine();
		}
	}
}
