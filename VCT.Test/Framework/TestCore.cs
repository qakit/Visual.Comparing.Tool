using System;
using System.Configuration;
using System.Diagnostics;
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

			DirectoryInfo stableDirectory = outputScreenFile.Directory.CreateSubdirectory("Stable");
			DirectoryInfo diffDirectory = outputScreenFile.Directory.CreateSubdirectory("Diff");

			
			//Save testing files anyway
			Client.Shell.Do.SendTestingFiles(outputScreenFile.Directory, testName);

			//get stable files and
			//if there are not stable files for test we need generate diff directory and create it on server side with testing files;
			var success = Client.Shell.Do.GetStableFiles(stableDirectory, testName);
			if (!success)
			{
				Client.Shell.Do.SendDiffFiles(diffDirectory, testName);
				return false;
			}

			var equal = CompareScreenshots(outputScreenFile, stableDirectory, diffDirectory);
			if (!equal)
			{
				Client.Shell.Do.SendDiffFiles(diffDirectory, testName);
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
