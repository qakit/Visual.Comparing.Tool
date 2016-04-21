using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using VCT.Test.Framework;

namespace VCT.Test
{
	[TestFixture]
	public class FakeTestFixture
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

			//Client.Shell.ServerAddress = @"http://10.98.4.67:80";

			var outputScreen = NewFile(@"C:\projects\VCT\Output\Google\google.png");
			var outputScreen2 = NewFile(@"C:\projects\VCT\Output\Google\google2.png");



			//driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("http://www.google.com/");
			var equal = TestCore.IsPageScreensAreEqual(driver, outputScreen, TestContext.CurrentContext.Test.FullName);
			var equal2 = TestCore.IsPageScreensAreEqual(driver, outputScreen2, TestContext.CurrentContext.Test.FullName);

			Assert.IsTrue(equal);
		}

		[Test]
		public void VKTest()
		{
			var outputScreen = NewFile(@"C:\projects\VCT\Output\VK\vk.png");


			//driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("https://vk.com");
			var equal = TestCore.IsPageScreensAreEqual(driver, outputScreen, TestContext.CurrentContext.Test.FullName);
			
			Assert.IsTrue(equal);
		}

		[Test]
		public void YandexTest()
		{
			var outputScreen = NewFile(@"C:\projects\VCT\Output\Yandex\yandex.png");

			//driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("https://yandex.ru");
			var equal = TestCore.IsPageScreensAreEqual(driver, outputScreen, TestContext.CurrentContext.Test.FullName);

			Assert.IsTrue(equal);
		}




		private FileInfo NewFile(string path)
		{
			var result = new FileInfo(path);
			if (! result.Directory.Exists) result.Directory.Create();
			return result;
		}
	}
}
