using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VCT.Client.Comparers;
using VCT.Sdk;
using VCT.Sdk.Extensions;

namespace VCT.Client.Modules
{
	public class Comparing
	{
		private readonly TestInfo _testInfo;

		public Comparing(string projectName, TestInfo testInfo)
		{
			_testInfo = testInfo;
			Shell.ProjectId = projectName;
		}

		/// <summary>
		/// Compare given image to the stable one which is stored on the server
		/// </summary>
		/// <param name="testingImageFile"></param>
		/// <returns></returns>
		public bool CompareImageToStable(FileInfo testingImageFile)
		{
			//TODO:
			//1. Ask for hash if hash equal when all is OK (send corresponding message so server could use stable image as testing image on preview)
			//2. If hash different send current image file to server! and let the server compare images and send you request
			//3. In case of multiple files send all files
			string testName = _testInfo.TestName;

			var stableHash = Shell.Do.GetStableFileHash(testName, _testInfo.Artifacts[0].Name, _testInfo).Result;
			//if hash empty it means that there is no stable file so send testing file with no processing request
			//TODO send file to server here just for saving as is without comparing
			if (string.IsNullOrEmpty(stableHash))
			{
				Shell.Do.SendTestingFile(testingImageFile, testName);
				return false;
			}

			//if hash not empty we must send testing file to server and server must compare it to stable
			//put stable / testing / diff to correct folders
			var testingHash = Utils.ComputeFileHash(testingImageFile);
			if (!string.Equals(stableHash, testingHash))
			{
				Shell.Do.SendTestingFile(testingImageFile, testName);
				return false;
			}

			Shell.Do.SayTestOkToServer(testName);
			return true;
//
//			//if hash for stable and testing files equal just return;
//			if (!string.IsNullOrEmpty(stableHash) && string.Equals(stableHash, testingHash, StringComparison.InvariantCultureIgnoreCase))
//			{
//				Shell.Do.SendTestingFiles(null, _testInfo.TestName);
//				return true;
//			}
//
//			DirectoryInfo stableDirectory = testingImageFile.Directory.CreateSubdirectory("Stable");
//			DirectoryInfo diffDirectory = testingImageFile.Directory.CreateSubdirectory("Diff");
//
//			//get stable files and
//			//if there are not any stable file for test we need generate diff directory and create it on server side;
//			var success = Shell.Do.GetStableFile(stableDirectory, testingImageFile.Name, testName);
//			if (!success)
//			{
//				Shell.Do.SendTestingFile(testingImageFile, testName);
//				Shell.Do.SendDiffFiles(diffDirectory, testName);
//				return false;
//			}
//
//			var stableImageFile = new FileInfo(Path.Combine(stableDirectory.FullName, testingImageFile.Name));
//			var diffFile = new FileInfo(Path.Combine(diffDirectory.FullName, testingImageFile.Name));
//
//			var equal = CompareImageFiles(testingImageFile, stableImageFile, diffFile);
//			if (!equal)
//			{
//				Shell.Do.SendDiffFile(diffFile, testName);
//				Shell.Do.SendStableFile(stableImageFile, testName);
//				Shell.Do.SendTestingFile(testingImageFile, testName);
//			}
		}

		/// <summary>
		/// Compare all images in specified firectory to stable files in the server. Will do comparison for all images can be long for large number of files
		/// </summary>
		/// <param name="testingImagesDirectory">Directory with images to compare</param>
		/// <param name="testName">Name of the test which produced testing images</param>
		/// <returns></returns>
		public bool CompareImagesToStable(DirectoryInfo testingImagesDirectory, string testName)
		{
			var equal = true;

			var images = testingImagesDirectory.GetImageFiles();
			if (images.Count == 0) return false;

			foreach (FileInfo image in images)
			{
				if (!CompareImageToStable(image))
					equal = false;
			}

			return equal;
		}

		private bool CompareImageFiles(FileInfo testingImageFile, FileInfo stableImageFile, FileInfo diffFile)
		{
			return ImageFileComparer.ComparingFilesAreEqual(stableImageFile, testingImageFile, diffFile);
		}
	}
}
