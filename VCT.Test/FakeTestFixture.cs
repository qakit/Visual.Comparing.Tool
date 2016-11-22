using System.IO;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using VCT.Client;
using VCT.Test.Framework;

namespace VCT.Test
{
	[TestFixture]
	public class FakeTestFixture
	{
		private RemoteWebDriver driver;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
//			DirectoryInfo dump = new DirectoryInfo(@"C:\projects\Visual.Comparing.Tool\Output");
//			if (!dump.Exists) dump.Create();
//			foreach (FileInfo file in dump.GetFiles())
//			{
//				file.Delete();
//			}
//			foreach (DirectoryInfo dir in dump.GetDirectories())
//			{
//				dir.Delete(true);
//			}
		}


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

			driver.Manage().Window.Maximize();
			driver.Navigate().GoToUrl("http://www.google.com/");
//			var equal = SampleTestCore.IsPageScreensAreEqual(driver, outputScreen, TestContext.CurrentContext.Test.FullName);

			var comparer = new VisualComparer("TestProject", TestContext.CurrentContext.Test.FullName, driver);
			var equal = comparer.VerifyPage();
			var equal2 = comparer.VerifyPage();

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
