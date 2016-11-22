using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using VCT.Client.Modules;
using VCT.Sdk;

namespace VCT.Client
{
	public class VisualComparer
	{
		private readonly string _projectName;
		private readonly string _testName;
		private readonly RemoteWebDriver _driver;
		private readonly Size _windowSize;
		private readonly DirectoryInfo _tempWorkingDirectory;

		public VisualComparer(string projectName, string testName, RemoteWebDriver driver)
		{
			_projectName = projectName;
			_testName = testName;
			_driver = driver;
			_windowSize = _driver.Manage().Window.Size;
			_tempWorkingDirectory = GetTemporaryDirectory();
		}

		public VisualComparer(string projectName, string testName, RemoteWebDriver driver, Size windowSize)
		{
			_projectName = projectName;
			_driver = driver;
			_windowSize = windowSize;
			_tempWorkingDirectory = GetTemporaryDirectory();
		}

		public bool VerifyPage(string tag = "")
		{
			var fileName = string.Format("{0}.png", string.IsNullOrEmpty(tag) ? _testName : tag);

			//make screenshot
			var screenshotMaker = new ScreenshotMaker(_driver);
			var outputFile = Utils.GetUniqueFileName(new FileInfo(Path.Combine(_tempWorkingDirectory.FullName, fileName)));

			screenshotMaker.MakeFullPageScreenshot(outputFile);

			//compare output screenshot with existing file on the server
			var comparingModule = new Comparing(_projectName, new TestInfo
			{
				Browser = _driver.Capabilities.BrowserName,
				TestName = _testName,
				WindowSize = _windowSize,
				Artifacts = new List<FileInfo> {outputFile}
			});

			var areEqual = comparingModule.CompareImageToStable(outputFile);

			return areEqual;
		}

		public void VerifyElement(IWebElement element, string tag = "", int margins = 0)
		{
			
		}

		public DirectoryInfo GetTemporaryDirectory()
		{
			string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			return Directory.CreateDirectory(tempDirectory);
		}

		//I KNOW that it's very very bad way to do it.
		//must be replaced 
		~VisualComparer()
		{
			_tempWorkingDirectory.Delete(true);
		}
	}
}
