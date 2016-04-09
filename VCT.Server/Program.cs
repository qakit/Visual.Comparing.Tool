using System;
using Microsoft.Owin.Hosting;

namespace VCT.Server
{
	class Program
	{
		static string baseAddress = "http://localhost:9111/";

		static void Main(string[] args)
		{
			WebApp.Start<Startup>(baseAddress);
			Console.WriteLine("Server started and waiting");
			Console.ReadLine();
		}
	}
}
