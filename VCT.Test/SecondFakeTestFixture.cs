﻿using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace VCT.Test
{
	[TestFixture]
	public class SecondFakeTestFixture
	{
		private RemoteWebDriver driver;

		[SetUp]
		public void SetUp()
		{
			driver = new ChromeDriver();
		}

		[TearDown]
		public void TearDown()
		{
			driver.Quit();
		}

		[Test]
		public void GoogleTest()
		{
			FileInfo outputScreen = new FileInfo(@"C:\projects\VCT\Output\Google\google.png");
			if (!outputScreen.Directory.Exists) outputScreen.Directory.Create();
			var core = TestCore.Instance;

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("http://www.google.com/");
			var equal = core.IsPageScreensAreEqual(driver, outputScreen, TestContext.CurrentContext.Test.FullName);

			Assert.IsTrue(equal);
		}

		[Test]
		public void VKTest()
		{
			FileInfo outputScreen = new FileInfo(@"C:\projects\VCT\Output\VK\vk.png");
			if (!outputScreen.Directory.Exists) outputScreen.Directory.Create();
			var core = TestCore.Instance;

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("https://vk.com");
			var equal = core.IsPageScreensAreEqual(driver, outputScreen, TestContext.CurrentContext.Test.FullName);
			
			Assert.IsTrue(equal);
		}
	}
}
