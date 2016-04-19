using System;
using System.IO;
using OpenQA.Selenium.Remote;

namespace VCT.Test.Framework
{
	public static class TestCore
	{
		#region Naming Convention
		public const string Stable = "STABLE";
		public const string Diff = "DIFF";
		#endregion

		public static bool IsPageScreensAreEqual(RemoteWebDriver driver, FileInfo outputScreenFile, string testName)
		{
			var screenshotMaker = new ScreenshotMaker(driver);
			screenshotMaker.MakeFullScreenshot(outputScreenFile);
			var stableDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, Stable));
			var diffDirectory = new DirectoryInfo(Path.Combine(outputScreenFile.Directory.FullName, Diff));

			
			//Save testing files anyway
			Client.Shell.Do.SaveTestingFiles(outputScreenFile.Directory, testName);

			//get stable files and
			//if there are not stable files for test we need generate diff directory and create it on server side with testing files;
			var success = Client.Shell.Do.GetStableFiles(stableDirectory, testName);
			if (!success)
			{
				if (!diffDirectory.Exists) diffDirectory.Create();

				Client.Shell.Do.SaveDiffFiles(diffDirectory, testName);
				return false;
			}

			var equal = CompareScreenshots(outputScreenFile, stableDirectory, diffDirectory);
			if (!equal)
			{
				Client.Shell.Do.SaveDiffFiles(diffDirectory, testName);
			}
			return equal;
		}

		private static bool CompareScreenshots(FileInfo outputTestingFile, DirectoryInfo stableFilesFolder, DirectoryInfo diffDirectory)
		{
			var stableImageFile = new FileInfo(Path.Combine(stableFilesFolder.FullName, outputTestingFile.Name));

			return ImageFileComparer.ComparingFilesAreEqual(stableImageFile, outputTestingFile, diffDirectory);
		}
	}
}
