using System;
using System.IO;
using System.Threading;
using ImageMagick;

namespace VCT.Server
{
	internal class ImageFileComparer
	{
		private const int TrustLevel = 7; //means how much pixel we can ignore before failed comparing

		internal static bool ComparingFilesAreEqual(string expectedImageFile, string actualImageFile, DirectoryInfo diffDirectory)
		{
			return ComparingFilesAreEqual(new FileInfo(expectedImageFile), new FileInfo(actualImageFile), diffDirectory);
		}

		internal static bool ComparingFilesAreEqual(FileInfo expectedImageFile, FileInfo actualImageFile, FileInfo diffFile)
		{
			if (expectedImageFile == null || actualImageFile == null)
			{
				Thread.Sleep(50);
				expectedImageFile.Refresh();
				actualImageFile.Refresh();
				Console.WriteLine("Images are null, try to refresh");
			}

			using (var stableVersionOutputImageFile = new MagickImage(expectedImageFile.FullName))
			{
				using (var testingVersionOutputImageFile = new MagickImage(actualImageFile.FullName))
				{
					using (var outputErrorImage = new MagickImage())
					{
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
							if (diffFile.Directory != null && !diffFile.Directory.Exists) diffFile.Directory.Create();

							outputErrorImage.Write(diffFile);
							return false;
						}

						return true;
					}
				}
			}
		}

		internal static bool ComparingFilesAreEqual(FileInfo expectedImageFile, FileInfo actualImageFile, DirectoryInfo diffDirectory)
		{
			var diffFile = new FileInfo(Path.Combine(
				diffDirectory.FullName,
				actualImageFile.Name));
			return ComparingFilesAreEqual(expectedImageFile, actualImageFile, diffFile);
		}
	}
}
