using System.IO;

namespace VCT.Server
{
	public class Storage
	{
		private readonly DirectoryInfo _rootDirectory;

		public Storage()
		{
			//TODO: move it to app.config or evaluate from code
			_rootDirectory = new DirectoryInfo(@"C:\projects\VCT\Storage");
			CreateFoldersHierarhy();
		}

		public Storage(DirectoryInfo rootDirectory)
		{
			_rootDirectory = rootDirectory;
			CreateFoldersHierarhy();
		}

		public DirectoryInfo StableFilesDirectory { get; set; }
		public DirectoryInfo TestingFilesDirectory { get; set; }
		public DirectoryInfo DiffFilesDirectory { get; set; }

		public DirectoryInfo StableTestDirectory(string testName)
		{
			return CreateSubdirectory(StableFilesDirectory, testName);
		}

		public DirectoryInfo TestingTestDirectory(string testName)
		{
			return CreateSubdirectory(TestingFilesDirectory, testName);
		}

		private DirectoryInfo CreateSubdirectory(DirectoryInfo parentDirectory, string directoryName)
		{
			var subdirectory = new DirectoryInfo(Path.Combine(parentDirectory.FullName, directoryName));
			if (!subdirectory.Exists) subdirectory.Create();
			return subdirectory;
		}

		private void CreateFoldersHierarhy()
		{
			StableFilesDirectory = new DirectoryInfo(Path.Combine(_rootDirectory.FullName, "StableFiles"));
			TestingFilesDirectory = new DirectoryInfo(Path.Combine(_rootDirectory.FullName, "TestingFiles"));
			DiffFilesDirectory = new DirectoryInfo(Path.Combine(_rootDirectory.FullName, "DiffFiles"));

			if (!StableFilesDirectory.Exists) StableFilesDirectory.Create();
			if (!TestingFilesDirectory.Exists) TestingFilesDirectory.Create();
			if (!DiffFilesDirectory.Exists) DiffFilesDirectory.Create();
		}
	}
}
