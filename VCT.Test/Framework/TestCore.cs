using System;
using System.IO;
using OpenQA.Selenium.Remote;

namespace VCT.Test.Framework
{
	///we need it all just for detect when main sute of tests is started, and when over
	public sealed class TestCore
	{
		#region Naming Convention
		public const string Stable = "STABLE";
		public const string Diff = "DIFF";
		#endregion

		private static readonly TestCore instance = new TestCore();

		private TestCore()
		{
			Console.WriteLine("Creating instance");
			//necessary to identify time when all tests are finished;
			var vctCore = new Client.Core();
			vctCore.SuiteStarted();

			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
		}

		public bool IsPageScreensAreEqual(RemoteWebDriver driver, FileInfo outputScreenFile, string testName)
		{
			var screenshotMaker = new ScreenshotMaker(driver);
			screenshotMaker.MakeFullScreenshot(outputScreenFile);
			var stableDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, Stable));
			var diffDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, Diff));

			var vctCore = new Client.Core();
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
			var vctCore = new Client.Core();
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
