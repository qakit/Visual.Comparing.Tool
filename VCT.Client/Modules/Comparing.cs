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
		public Comparing(string projectName)
		{
			Shell.ProjectId = projectName;
		}

		/// <summary>
		/// Compare given image to the stable one which is stored on the server
		/// </summary>
		/// <param name="testingImageFile"></param>
		/// <returns></returns>
		public bool CompareImageToStable(FileInfo testingImageFile, string testName)
		{
			var stableHash = Shell.Do.GetStableFileHash(testName, testingImageFile.Name);
			var testingHash = Utils.ComputeFileHash(testingImageFile);

			//if hash for stable and testing files equal just return;
			if (!string.IsNullOrEmpty(stableHash) && string.Equals(stableHash, testingHash, StringComparison.InvariantCultureIgnoreCase))
			{
				Shell.Do.SendTestingFiles(null, testName);
				return true;
			}

			DirectoryInfo stableDirectory = testingImageFile.Directory.CreateSubdirectory("Stable");
			DirectoryInfo diffDirectory = testingImageFile.Directory.CreateSubdirectory("Diff");

			//get stable files and
			//if there are not any stable file for test we need generate diff directory and create it on server side;
			var success = Shell.Do.GetStableFile(stableDirectory, testingImageFile.Name, testName);
			if (!success)
			{
				Shell.Do.SendTestingFile(testingImageFile, testName);
				Shell.Do.SendDiffFiles(diffDirectory, testName);
				return false;
			}

			var stableImageFile = new FileInfo(Path.Combine(stableDirectory.FullName, testingImageFile.Name));
			var diffFile = new FileInfo(Path.Combine(diffDirectory.FullName, testingImageFile.Name));

			var equal = CompareImageFiles(testingImageFile, stableImageFile, diffFile);
			if (!equal)
			{
				Shell.Do.SendDiffFile(diffFile, testName);
				Shell.Do.SendStableFile(stableImageFile, testName);
				Shell.Do.SendTestingFile(testingImageFile, testName);
			}

			return equal;
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

			var images = GetResultImages(testingImagesDirectory);
			if (images.Count == 0) return false;

			foreach (FileInfo image in images)
			{
				if (!CompareImageToStable(image, testName))
					equal = false;
			}

			return equal;
		}

		private bool CompareImageFiles(FileInfo testingImageFile, FileInfo stableImageFile, FileInfo diffFile)
		{
			return ImageFileComparer.ComparingFilesAreEqual(stableImageFile, testingImageFile, diffFile);
		}

		internal static List<FileInfo> GetResultImages(DirectoryInfo directoryToSearch)
		{
			if (directoryToSearch == null) return new List<FileInfo>();
			var imageFiles = directoryToSearch.GetFiles(new[] { "*.png", "*.bmp", "*.jpeg", "*.jpg", "*.gif" },
				SearchOption.TopDirectoryOnly);
			return imageFiles.ToList();
		}
	}
}
