using System;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using VCT.Client;

namespace VCT.Test
{
//	[TestFixture]
//	public class FakeTestFixture
//	{
//		private RemoteWebDriver driver;
//
//		[SetUp]
//		public void SetUp()
//		{
//			driver = new ChromeDriver();
//		}
//
//		[TearDown]
//		public void TearDown()
//		{
//			driver.Quit();
//		}
//
//		[Test]
//		public void GoogleTest()
//		{
//
//			driver.Manage().Window.Maximize();
//			driver.Navigate().GoToUrl("file:///C:/projects/Visual.Comparing.Tool/index.html");
//
//			var comparer = new VisualComparer("TestProject", TestContext.CurrentContext.Test.FullName, driver);
//			var equal = comparer.VerifyPage("Page1");
//
//			driver.Navigate().GoToUrl("http://www.google.com");
//			var equal2 = comparer.VerifyPage("Page2");
//
//			Assert.IsTrue(equal);
//			Assert.IsTrue(equal2);
//		}
//
//		[Test]
//		public void GoogleTest2()
//		{
//
//			driver.Manage().Window.Maximize();
//			driver.Navigate().GoToUrl("file:///C:/projects/Visual.Comparing.Tool/index.html");
//
//			var comparer = new VisualComparer("TestProject", TestContext.CurrentContext.Test.FullName, driver);
//			var equal = comparer.VerifyPage("Page1");
//
////			driver.Navigate().GoToUrl("file:///C:/projects/Visual.Comparing.Tool/index2.html");
////			var equal2 = comparer.VerifyPage("Page2");
//
//			Assert.IsTrue(equal);
////			Assert.IsTrue(equal2);
//		}
//	}

	[TestFixture(typeof(ChromeDriver))]
	[TestFixture(typeof(FirefoxDriver))]
	public class FakeTestFixture : Base
	{
		public FakeTestFixture(Type driverType)
			: base(driverType)
		{
		}

		[TearDown]
		public void TearDown()
		{
			Driver.Quit();
		}

		[Test]
		public void GoogleTest()
		{

			Driver.Manage().Window.Maximize();
			Driver.Navigate().GoToUrl("file:///C:/projects/Visual.Comparing.Tool/index2.html");

			var comparer = new VisualComparer("TestProject", "SAMPLE TEST1", Driver);
			var equal = comparer.VerifyPage("Page1");

//			driver.Navigate().GoToUrl("http://www.google.com");
//			var equal2 = comparer.VerifyPage("Page2");

			Assert.IsTrue(equal);
//			Assert.IsTrue(equal2);
		}

		[Test]
		public void GoogleTest2()
		{

			Driver.Manage().Window.Maximize();
			Driver.Navigate().GoToUrl("file:///C:/projects/Visual.Comparing.Tool/index.html");

			var comparer = new VisualComparer("TestProject", TestContext.CurrentContext.Test.FullName, Driver);
			var equal = comparer.VerifyPage("Page1");

			//			driver.Navigate().GoToUrl("file:///C:/projects/Visual.Comparing.Tool/index2.html");
			//			var equal2 = comparer.VerifyPage("Page2");

			Assert.IsTrue(equal);
			//			Assert.IsTrue(equal2);
		}
	}

	[TestFixture]
	public class Base
	{
		public RemoteWebDriver Driver;

		public Base(Type type)
		{
			Initialize(type);
		}

		private void Initialize(Type driverType)
		{
			if (driverType == typeof(ChromeDriver))
			{
				var options = new ChromeOptions();
				options.AddArguments("test-type", "--disable-popup-blocking", "--disable-extensions");
				Driver = (RemoteWebDriver)Activator.CreateInstance(driverType, options);
				return;
			}
			//TODO add settings for other drivers if necessary
			Driver = (RemoteWebDriver)Activator.CreateInstance(driverType);
		}
	}
}
