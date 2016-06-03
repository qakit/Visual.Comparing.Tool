using System;
using System.IO;
using System.Linq;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	public class Storage
	{
		public readonly DirectoryInfo Root;
		public static Hub Current;

		static readonly string HistoryFileName = Config.AppSettings["history"];

		public Storage()
		{
			Root = Directory.CreateDirectory(Config.AppSettings["storage"]);
		}

		public class Hub //nested bcoz it shouldn't be used without Storage context
		{
			public string DebugInfo()
			{
				return string.Join(" |\r", suiteDirectory.FullName, stableDirectory.FullName, testingDirectory.FullName, diffDirectory.FullName);
			}

			private readonly DirectoryInfo _suiteDirectory;
			private readonly DirectoryInfo _projectDirectory;
			private readonly DirectoryInfo _stableDirectory;
			private readonly DirectoryInfo _testingDirectory;
			private readonly DirectoryInfo _diffDirectory;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="suiteDirectory">Suite result directory</param>
			public Hub(DirectoryInfo suiteDirectory)
			{
				_suiteDirectory =		suiteDirectory;
				_projectDirectory =		suiteDirectory.Parent;
				_stableDirectory =	_suiteDirectory.CreateSubdirectory("StableFiles");
				_testingDirectory =	_suiteDirectory.CreateSubdirectory("TestingFiles");
				_diffDirectory =		_suiteDirectory.CreateSubdirectory("DiffFiles");
			}

			public DirectoryInfo suiteDirectory		{ get { return _suiteDirectory; } }
			public DirectoryInfo projectDirectory		{ get { return _projectDirectory; } }
			public DirectoryInfo stableDirectory		{ get { return _stableDirectory; } }
			public DirectoryInfo testingDirectory	{ get { return _testingDirectory; } }
			public DirectoryInfo diffDirectory		{ get { return _diffDirectory; } }


			#region old wrapers

			public DirectoryInfo StableTestDirectory(string testName)
			{
				return stableDirectory.CreateSubdirectory(testName);
			}

			public DirectoryInfo TestingTestDirectory(string testName)
			{
				return testingDirectory.CreateSubdirectory(testName);
			}

			public DirectoryInfo DiffTestDirectory(string testName)
			{
				return diffDirectory.CreateSubdirectory(testName);
			}

			#endregion

		}

		public DirectoryInfo GetLatestExistingStable(string testIdentifyer, DirectoryInfo directoryToSearch = null)
		{
			//TODO: dont search in all, search just in proj dir
			if (directoryToSearch == null) directoryToSearch = Root;

			var search = directoryToSearch.GetDirectories("*", SearchOption.AllDirectories)
							 .Where(d => d.Parent.Name.Equals("StableFiles", StringComparison.InvariantCultureIgnoreCase) &&
										 d.Name.Equals(testIdentifyer, StringComparison.InvariantCultureIgnoreCase) &&
										 d.GetFiles().Any())
							 .OrderBy(d => d.CreationTime);
			return search.LastOrDefault();
		}


		/// <summary>
		/// Writes info text to history file
		/// </summary>
		/// <param name="projId"></param>
		/// <param name="infoText">text</param>
		/// <param name="removeIfExists">do we need to remove previous file if it exists (optional. Default - false)</param>
		public void WriteLog(string projId, string infoText, bool removeIfExists = false)
		{
			DirectoryInfo projDir = Root.CreateSubdirectory(projId);

			var historyFile = new FileInfo(Path.Combine(projDir.FullName, HistoryFileName));

			using (var writer = new StreamWriter(historyFile.FullName, true))
			{
				writer.WriteLine(infoText);
			}
		}

		public void Allocate(string projId, string inceptionTime)
		{
			DirectoryInfo branch = Root.CreateSubdirectory(projId).CreateSubdirectory(inceptionTime);
			Current = new Hub(branch);
		}
	}
}
