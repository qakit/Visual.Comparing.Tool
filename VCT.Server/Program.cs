using System;
using System.Linq;
using Microsoft.Owin.Hosting;
using VCT.Server.Entities;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	class Program
	{
		static readonly string BaseAddress = Config.AppSettings["rootUrl"];

		static void Main(string[] args)
		{
			using (var storage = new StorageContext())
			{
				storage.Projects.FirstOrDefault();
				storage.SaveChanges();
			}
			WebApp.Start<Startup>(BaseAddress);
			Console.WriteLine("Server started and waiting");
			Console.ReadLine();
		}
	}
}
