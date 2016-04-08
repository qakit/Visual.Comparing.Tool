using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VCT.Server
{
	public static class Extensions
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

		public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo directory, string[] searchPatterns)
		{
			return GetFiles(directory, searchPatterns, SearchOption.AllDirectories);
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
			if(!targetDirectory.Exists) targetDirectory.Create();
			foreach (var fileInfo in sourceDirectory.GetFiles())
			{
				var filePath = Path.Combine(targetDirectory.FullName, fileInfo.Name);
				fileInfo.CopyTo(filePath, false);
			}

			foreach (DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
			{
				var directoryPath = Path.Combine(targetDirectory.FullName, directoryInfo.Name);
				CopyDir(directoryInfo, new DirectoryInfo(directoryPath));
			}
		}
	}
}
