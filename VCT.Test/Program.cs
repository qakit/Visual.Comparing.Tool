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
			FileInfo outputScreen = new FileInfo(@"C:\projects\VCT\Output\VK\vk.png");
			if (!outputScreen.Directory.Exists) outputScreen.Directory.Create();
			var core = TestCore.Instance;

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("https://vk.com");
			var equal = core.IsPageScreensAreEqual(driver, outputScreen, "VCT.Test.SecondFakeTestFixture.GoogleTest");
			
			driver.Quit();

			Console.ReadLine();
		}
	}
}
