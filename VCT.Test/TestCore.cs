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
			//necessary to identify time when all tests are finished;
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
		}

		public bool IsPageScreensAreEqual(RemoteWebDriver driver, FileInfo outputScreenFile, string testName)
		{
			var screenshotMaker = new ScreenshotMaker(driver);
			screenshotMaker.MakeFullScreenshot(outputScreenFile);
			var stableDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, "STABLE"));
			var diffDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, "DIFF"));

			var vctCore = new Core();
			var success = vctCore.GetStableFiles(stableDirectory, testName);
			if (!success)
			{
				vctCore.SaveTestingFiles(outputScreenFile.Directory, testName);
				return false;
			}

			var equal = CompareScreenshots(outputScreenFile, stableDirectory, diffDirectory);
			if (!equal)
			{
				vctCore.SaveTestingFiles(outputScreenFile.Directory, testName);
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
			//Pass end time of group test to server
			MessageBox.Show("All is done");
		}

		public static TestCore Instance
		{
			get { return instance; }
		}
	}
}
