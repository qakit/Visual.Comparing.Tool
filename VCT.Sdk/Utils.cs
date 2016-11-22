using System;
using System.Collections.Generic;
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
	}
}
