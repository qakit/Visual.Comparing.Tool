using System;
using System.IO;
using System.Threading;
using ImageMagick;

namespace VCT.Test.Framework
{
	public class ImageFileComparer
	{
		private const int TrustLevel = 7; //means how much pixel we can ignore before failed comparing

		public static bool ComparingFilesAreEqual(string expectedOutputImageFile, string actualOutputImageFile, DirectoryInfo diffDirectory)
		{
			return ComparingFilesAreEqual(new FileInfo(expectedOutputImageFile), new FileInfo(actualOutputImageFile), diffDirectory);
		}

		public static bool ComparingFilesAreEqual(FileInfo expectedOutputImageFile, FileInfo actualOutputImageFile, DirectoryInfo diffDirectory)
		{

			if (expectedOutputImageFile == null || actualOutputImageFile == null)
			{
				Thread.Sleep(50);
				expectedOutputImageFile.Refresh();
				actualOutputImageFile.Refresh();
				Console.WriteLine("Images are null, try to refresh");
			}

			var stableVersionOutputImageFile = new MagickImage(expectedOutputImageFile.FullName);
			var testingVersionOutputImageFile = new MagickImage(actualOutputImageFile.FullName);

			var outputErrorImage = new MagickImage();

			double errors;
			try
			{
				errors = testingVersionOutputImageFile.Compare(stableVersionOutputImageFile, ErrorMetric.Absolute, outputErrorImage);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error occured during comparing two images. Exception: {0}", e.Message);
				return false;
			}

			if (errors > TrustLevel)
			{
				if (!diffDirectory.Exists) diffDirectory.Create();

				var outputErrorFile = Path.Combine(
					diffDirectory.FullName, 
					Path.GetFileNameWithoutExtension(actualOutputImageFile.Name) + "_comparingResults.png");
				outputErrorImage.Write(outputErrorFile);
				return false;
			}
			return true;

		}
	}
}
