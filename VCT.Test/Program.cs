using System;
using System.IO;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;


namespace VCT.Test
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			RemoteWebDriver driver = new ChromeDriver();
			FileInfo outputScreen = new FileInfo(@"C:\projects\VCT\Output\TESTOUTPUT.png");
			var core = TestCore.Instance;

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("https://vk.com");
//			driver.Navigate().GoToUrl("http://www.google.com/");
			var equal = core.IsPageScreensAreEqual(driver, outputScreen, "FAKETEST22");
			Console.WriteLine(equal);
			driver.Quit();

			Console.ReadLine();
		}
	}
}
