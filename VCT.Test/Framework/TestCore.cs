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

			DirectoryInfo stableDirectory = outputScreenFile.Directory.CreateSubdirectory("Stable");
			DirectoryInfo diffDirectory = outputScreenFile.Directory.CreateSubdirectory("Diff");
			Client.Shell.ProjectId = "her";

			//Save testing files anyway
			var stableHash = Client.Shell.Do.GetStableFileHash(testName, outputScreenFile.Name);
			var testingHash = Client.Shell.Do.ComputeFileHash(outputScreenFile);

			//if hash for stable and testing files equal just return;
			if (!string.IsNullOrEmpty(stableHash) && string.Equals(stableHash, testingHash, StringComparison.InvariantCultureIgnoreCase))
			{
				Client.Shell.Do.SendTestingFiles(null, testName);
				return true;
			}

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
				Client.Shell.Do.SendStableFiles(stableDirectory, testName);
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
