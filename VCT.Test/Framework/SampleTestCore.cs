using System.IO;
using OpenQA.Selenium.Remote;
using VCT.Client.Modules;

namespace VCT.Test.Framework
{
	public static class SampleTestCore
	{
		public static bool IsPageScreensAreEqual(RemoteWebDriver driver, FileInfo outputScreenFile, string testName)
		{
			var screenshotMaker = new ScreenshotMaker(driver);
			screenshotMaker.MakeFullScreenshot(outputScreenFile);

			var comparingModule = new Comparing("TestProject");
			return comparingModule.CompareImageToStable(outputScreenFile, testName);
		}
	}
}
