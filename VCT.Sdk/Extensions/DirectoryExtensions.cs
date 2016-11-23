using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VCT.Sdk.Extensions
{
	public static class DirectoryExtensions
	{
		public static void CopyTo(this DirectoryInfo sourceDirectory, string targetDirectory)
		{
			var targetDirectoryInfo = new DirectoryInfo(targetDirectory);

			CopyDir(sourceDirectory, targetDirectoryInfo);
		}

		public static void CopyTo(this DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
		{
			CopyDir(sourceDirectory, targetDirectory);
		}

		public static void ClearContent(this DirectoryInfo directory)
		{
			foreach (FileInfo fileInfo in directory.GetFiles())
			{
				fileInfo.Delete();
			}
			foreach (DirectoryInfo subDirectory in directory.GetDirectories())
			{
				subDirectory.ClearContent();
			}
		}

		public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo directory, string[] searchPatterns)
		{
			return GetFiles(directory, searchPatterns, SearchOption.AllDirectories);
		}

		public static List<FileInfo> GetImageFiles(this DirectoryInfo directoryToSearch)
		{
			if (directoryToSearch == null) return new List<FileInfo>();
			var imageFiles = directoryToSearch.GetFiles(new[] { "*.png", "*.bmp", "*.jpeg", "*.jpg", "*.gif" },
				SearchOption.TopDirectoryOnly);
			return imageFiles.ToList();
		}

		public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo directory, string[] searchPatterns,
			SearchOption option)
		{
			if (searchPatterns == null) throw new ArgumentNullException("searchPatterns");
			IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();
			return searchPatterns.Aggregate(files, (current, searchPattern) => current.Concat(directory.GetFiles(searchPattern, option)));
		}

		private static void CopyDir(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
		{
			if (!targetDirectory.Exists) targetDirectory.Create();
			//remove all previous files to avoid adding extra images;
			targetDirectory.ClearContent();
			foreach (var fileInfo in sourceDirectory.GetFiles())
			{
				var filePath = Path.Combine(targetDirectory.FullName, fileInfo.Name);
				fileInfo.CopyTo(filePath, true);
			}

			foreach (DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
			{
				var directoryPath = Path.Combine(targetDirectory.FullName, directoryInfo.Name);
				CopyDir(directoryInfo, new DirectoryInfo(directoryPath));
			}
		}
	}
}
