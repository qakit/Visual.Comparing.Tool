using System;
using System.IO;
using System.Windows.Forms;
using OpenQA.Selenium.Remote;
using VCT.Client;

namespace VCT.Test
{
	public sealed class TestCore
	{
		private static readonly TestCore instance = new TestCore();

		private TestCore()
		{
			Console.WriteLine("Creating instance");
			//necessary to identify time when all tests are finished;
			var vctCore = new Core();
			vctCore.SuiteStarted();

			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
		}

		public bool IsPageScreensAreEqual(RemoteWebDriver driver, FileInfo outputScreenFile, string testName)
		{
			var screenshotMaker = new ScreenshotMaker(driver);
			screenshotMaker.MakeFullScreenshot(outputScreenFile);
			var stableDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, "STABLE"));
			var diffDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, "DIFF"));

			var vctCore = new Core();
			//Save testing files anyway
			vctCore.SaveTestingFiles(outputScreenFile.Directory, testName);

			//get stable files and
			//if there are not stable files for test we need generate diff directory and create it on server side with testing files;
			var success = vctCore.GetStableFiles(stableDirectory, testName);
			if (!success)
			{
				if (!diffDirectory.Exists) diffDirectory.Create();

				vctCore.SaveDiffFiles(diffDirectory, testName);
				return false;
			}

			var equal = CompareScreenshots(outputScreenFile, stableDirectory, diffDirectory);
			if (!equal)
			{
				vctCore.SaveDiffFiles(diffDirectory, testName);
			}
			return equal;
		}

		private bool CompareScreenshots(FileInfo outputTestingFile, DirectoryInfo stableFilesFolder, DirectoryInfo diffDirectory)
		{
			var stableImageFile = new FileInfo(Path.Combine(stableFilesFolder.FullName, outputTestingFile.Name));

			return ImageFileComparer.ComparingFilesAreEqual(stableImageFile, outputTestingFile, diffDirectory);
		}

		private void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
		{
			var vctCore = new Core();
			vctCore.SuiteCompleted();
			Console.WriteLine("Suite has been finished");
			//Pass end time of group test to server
		}

		public static TestCore Instance
		{
			get { return instance; }
		}
	}
}
