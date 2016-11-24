using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VCT.Sdk.Extensions;

namespace VCT.Sdk
{
	public class Utils
	{
		/// <summary>
		/// Computes hash for current file
		/// </summary>
		/// <param name="testingFile"></param>
		/// <returns></returns>
		public static string ComputeFileHash(FileInfo testingFile)
		{
			SHA256 hash = SHA256Managed.Create();

			var fileStream = testingFile.Open(FileMode.Open);
			var hashValue = hash.ComputeHash(fileStream);
			var result = GetHexByteArray(hashValue);

			fileStream.Close();
			return result;
		}

		public static string HashFromBase64(string base64String)
		{
			SHA512 hash = SHA512.Create();
			var hashValue = hash.ComputeHash(Encoding.UTF8.GetBytes(base64String));

			var result = GetHexByteArray(hashValue);
			return result;
		}

		public static string ImageToBase64(FileInfo imageFile)
		{
			var img = Image.FromFile(imageFile.FullName);
			return ImageToBase64(img, ImageFormat.Png);
		}

		public static string ImageToBase64(Image image, ImageFormat format)
		{
			using (var ms = new MemoryStream())
			{
				// Convert Image to byte[]
				image.Save(ms, format);
				byte[] imageBytes = ms.ToArray();

				// Convert byte[] to Base64 String
				string base64String = Convert.ToBase64String(imageBytes);
				return base64String;
			}
		}

		public static Image Base64ToImage(string base64String)
		{
			// Convert Base64 String to byte[]
			byte[] imageBytes = Convert.FromBase64String(base64String);
			var ms = new MemoryStream(imageBytes, 0,
			  imageBytes.Length);

			// Convert byte[] to Image
			ms.Write(imageBytes, 0, imageBytes.Length);
			Image image = Image.FromStream(ms, true);
			return image;
		}

		private static string GetHexByteArray(byte[] array)
		{
			var builder = new StringBuilder();
			int i;
			for (i = 0; i < array.Length; i++)
			{
				builder.Append(String.Format("{0:X2}", array[i]));
			}
			return builder.ToString();
		}

		/// <summary>
		/// Search for element in the list by it's possible name
		/// </summary>
		/// <param name="listToSearchIn">List of files to search in</param>
		/// <param name="fileNameToSearch">Expected file name</param>
		/// <returns><see cref="FileInfo"/> or null if no file exist in the list</returns>
		public static FileInfo GetFileByName(List<FileInfo> listToSearchIn, string fileNameToSearch)
		{
			return (from item in listToSearchIn
					where string.Equals(item.Name, fileNameToSearch, StringComparison.InvariantCultureIgnoreCase)
					select item).FirstOrDefault();
		}

		/// <summary>
		/// Gets the longest list from the specified
		/// </summary>
		/// <param name="lists">Lists</param>
		/// <returns>Longest list</returns>
		public static IEnumerable<FileInfo> FindLongest(params List<FileInfo>[] lists)
		{
			var longest = lists[0];
			for (var i = 1; i < lists.Length; i++)
			{
				if (lists[i].Count > longest.Count)
					longest = lists[i];
			}
			return longest;
		}

		public static List<FileInfo> GetResultImages(DirectoryInfo directoryToSearch)
		{
			if (directoryToSearch == null) return new List<FileInfo>();
			var imageFiles = directoryToSearch.GetFiles(new[] { "*.png", "*.bmp", "*.jpeg", "*.jpg", "*.gif" },
				SearchOption.TopDirectoryOnly);
			return imageFiles.ToList();
		}

		public static FileInfo GetUniqueFileName(FileInfo filePath)
		{
			int count = 1;

			string fileNameOnly = Path.GetFileNameWithoutExtension(filePath.FullName);
			string extension = Path.GetExtension(filePath.FullName);
			FileInfo newFullPath = filePath;

			while (newFullPath.Exists)
			{
				string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
				newFullPath = new FileInfo(Path.Combine(filePath.Directory.FullName, tempFileName + extension));
			}
			return newFullPath;
		}
	}
}
