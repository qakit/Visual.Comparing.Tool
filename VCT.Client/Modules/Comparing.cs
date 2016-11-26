using System;
using System.IO;
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
				Shell.Do.SendTestingFile(testingImageFile, testName, _testInfo);
				return false;
			}

			//if hash not empty we must send testing file to server and server must compare it to stable
			//put stable / testing / diff to correct folders
			var imageBase64 = Utils.ImageToBase64(testingImageFile);
			var testingHash = Utils.HashFromBase64(imageBase64);
			if (!string.Equals(stableHash, testingHash))
			{
				Shell.Do.SendTestingFile(testingImageFile, testName, _testInfo);
				return false;
			}
			Console.WriteLine("Saying ok for test {0}", _testInfo.TestName);
			Shell.Do.SayTestOkToServer(testName, _testInfo);
			return true;
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
	}
}
