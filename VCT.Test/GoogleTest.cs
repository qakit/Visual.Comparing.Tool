using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace VCT.Test
{
	[TestFixture]
	public class GoogleTest
	{
		[Test]
		public void GoogleInputBoxScreenshotTest()
		{
			RemoteWebDriver driver = new ChromeDriver();
			var core = new VCTCore(driver);

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("http://www.google.com");
			var searchELement = driver.FindElement(By.CssSelector(".sbibod"));
			core.MakeElementScreenshot(searchELement);

			driver.Quit();
		}

		[Test]
		public void FogFullPageTest()
		{
			RemoteWebDriver driver = new ChromeDriver();
			var core = new VCTCore(driver);

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("http://www.csszengarden.com/");
			
			core.MakeFullScreenshot();
			driver.Quit();
		}
	}
}
