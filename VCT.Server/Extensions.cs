using System.IO;

namespace VCT.Server
{
	public static class Extensions
	{
		public static void CopyTo(this DirectoryInfo sourceDirectory, string targetDirectory)
		{
			var targetDirectoryInfo = new DirectoryInfo(targetDirectory);

			CopyDir(sourceDirectory, targetDirectoryInfo);
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
