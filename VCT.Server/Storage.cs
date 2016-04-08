using System;
using System.IO;
using System.Linq;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	public class Storage
	{
		private readonly DirectoryInfo _rootDirectory;
		private const string HistoryFileName = "suiteInformation.txt";

		public Storage()
		{
			//TODO: move it to app.config or evaluate from code
			_rootDirectory = new DirectoryInfo(Config.AppSettings["storage"]);
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
		public DirectoryInfo HistoryFilesDirectory { get; set; }

		public DirectoryInfo StableTestDirectory(string testName)
		{
			return CreateSubdirectory(StableFilesDirectory, testName);
		}

		public DirectoryInfo TestingTestDirectory(string testName)
		{
			return CreateSubdirectory(TestingFilesDirectory, testName);
		}

		public DirectoryInfo DiffTestDirectory(string testName)
		{
			return CreateSubdirectory(DiffFilesDirectory, testName);
		}

		public DirectoryInfo HistoryTestFilesDirectory(string date)
		{
			return CreateSubdirectory(HistoryFilesDirectory, date);
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
			HistoryFilesDirectory = new DirectoryInfo(Path.Combine(_rootDirectory.FullName, "History"));

			if (!StableFilesDirectory.Exists) StableFilesDirectory.Create();
			if (!TestingFilesDirectory.Exists) TestingFilesDirectory.Create();
			if (!DiffFilesDirectory.Exists) DiffFilesDirectory.Create();
			if (!HistoryFilesDirectory.Exists) HistoryFilesDirectory.Create();
		}

		/// <summary>
		/// Writes info text to suiteInfo.txt file
		/// </summary>
		/// <param name="infoText">text</param>
		/// <param name="removeIfExists">do we need to remove previous file if it exists (optional. Default - false)</param>
		public void WriteHistoryInfo(string infoText, bool removeIfExists = false)
		{
			var historyFile = new FileInfo(Path.Combine(TestingFilesDirectory.FullName, HistoryFileName));
			Console.WriteLine("IF file exists {0}", historyFile.Exists);
			if (removeIfExists)
			{
				Console.WriteLine("Removing file");
				if (historyFile.Exists)
				{
					historyFile.Delete();
				}
			}

			Console.WriteLine("Writing info");
			using (var writer = new StreamWriter(historyFile.FullName, true))
			{
				writer.WriteLine(infoText);
			}
		}

		public void BackUpPreviousRunForHistory()
		{
			//get history file
			var historyFile = new FileInfo(Path.Combine(TestingFilesDirectory.FullName, HistoryFileName));
			if (!historyFile.Exists) return;
			//get completed date
			var completeDateLine = (from line in File.ReadAllLines(historyFile.FullName)
				where line.StartsWith("completed")
				select line).FirstOrDefault();
			string completedDate = "";
			if (completeDateLine != null)
			{
				completedDate = completeDateLine.Split('|')[1];
			}
			else
			{
				return;
			}

			//get history directory named as completed date
			var historyDirectory = new DirectoryInfo(Path.Combine(HistoryFilesDirectory.FullName, completedDate));
			historyDirectory.Create();

			//move all failed tests to this directory
			//first get all testing directories
			//as stable can contain many directories we need only failed one so need to match directories in stable folder to testing
			//and move only these directories
			var testingDirectories = TestingFilesDirectory.GetDirectories();
			var testingDirectoryNames = (from dir in testingDirectories select dir.Name).ToArray();

			TestingFilesDirectory.MoveTo(Path.Combine(historyDirectory.FullName, "TestingFiles"));
			DiffFilesDirectory.MoveTo(Path.Combine(historyDirectory.FullName, "DiffFiles"));
			CreateSubdirectory(historyDirectory, "StableFiles");

			//after moving folder i need it re-create
			//P.S. TestingFilesDirectory after moving changes it's ref to another one o_O
			CreateFoldersHierarhy();

			foreach (DirectoryInfo stableTestDir in StableFilesDirectory.GetDirectories())
			{
				if (testingDirectoryNames.Contains(stableTestDir.Name))
				{
					stableTestDir.CopyTo(Path.Combine(historyDirectory.FullName, "StableFiles", stableTestDir.Name));
				}
			}
		}
	}
}
