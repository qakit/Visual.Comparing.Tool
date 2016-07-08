using System.Collections.Generic;
using System.IO;
using System.Linq;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	public class Storage
	{
		public readonly DirectoryInfo StorageDirectory;

		public Storage()
		{
			StorageDirectory = Directory.CreateDirectory(Config.AppSettings["storage"]);
		}

		public List<StorageProject> Projects
		{
			get
			{
				return StorageDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Select(d => new StorageProject(d)).ToList();
			}
		} 

		public StorageProject Project(string projectId)
		{
			var projectDirectory = StorageDirectory.GetDirectories(projectId, SearchOption.TopDirectoryOnly).FirstOrDefault();
			return projectDirectory == null ? new StorageProject(StorageDirectory.CreateSubdirectory(projectId)) : new StorageProject(projectDirectory);
		}

		/// <summary>
		/// Represent a single project in the storage
		/// </summary>
		public class StorageProject
		{
			private readonly DirectoryInfo _projectDirectory;
			private readonly DirectoryInfo _suitesDirectory;
			private readonly DirectoryInfo _stableFilesDirectory;

			public StorageProject(DirectoryInfo projectDirectory)
			{
				_projectDirectory = projectDirectory;
				_suitesDirectory = projectDirectory.CreateSubdirectory("Suites");
				_stableFilesDirectory = projectDirectory.CreateSubdirectory("StableFiles");
			}

			public DirectoryInfo Directory
			{
				get { return _projectDirectory; }
			}

			public List<ProjectSuite> Suites
			{
				get
				{
					return _suitesDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).Select(directory => new ProjectSuite(directory)).ToList();
				}
			}

			public ProjectSuite Suite(string suiteId)
			{
				var suiteDirectory = _suitesDirectory.GetDirectories(suiteId, SearchOption.TopDirectoryOnly).FirstOrDefault() ?? _suitesDirectory.CreateSubdirectory(suiteId);
				return new ProjectSuite(suiteDirectory);
			}

			public DirectoryInfo StableFiles
			{
				get { return _stableFilesDirectory; }
			}

			public DirectoryInfo StableTestDirectory(string testName)
			{
				return _stableFilesDirectory.CreateSubdirectory(testName);
			}

			/// <summary>
			/// Represent a single suite (run) in the current project
			/// </summary>
			public class ProjectSuite
			{
				private readonly DirectoryInfo _suiteDirectory;
				private readonly DirectoryInfo _diffFilesDirectory;
				private readonly DirectoryInfo _testingFilesDirectory;
				private readonly DirectoryInfo _stableFilesDirectory;

				public ProjectSuite(DirectoryInfo suiteDirectory)
				{
					_suiteDirectory = suiteDirectory;

					_stableFilesDirectory = _suiteDirectory.CreateSubdirectory("StableFiles");
					_testingFilesDirectory = _suiteDirectory.CreateSubdirectory("TestingFiles");
					_diffFilesDirectory = _suiteDirectory.CreateSubdirectory("DiffFiles");
				}

				public DirectoryInfo Directory
				{
					get { return _suiteDirectory; }
				}

				public string DateStarted
				{
					get { return _suiteDirectory.CreationTime.ToShortTimeString(); }
				}

				public string DateCompleted
				{
					get
					{
						//TODO can be slow!
						DirectoryInfo lastCreatedFile = TestingFilesDirectory.GetDirectories("*", SearchOption.AllDirectories)
							.OrderBy(d => d.CreationTime)
							.LastOrDefault();

						return lastCreatedFile != null ? lastCreatedFile.CreationTime.ToShortTimeString() : "No Time";
					}
				}

				public DirectoryInfo StableTestDirectory(string testName)
				{
					return _stableFilesDirectory.CreateSubdirectory(testName);
				}

				public DirectoryInfo TestingTestDirectory(string testName)
				{
					return _testingFilesDirectory.CreateSubdirectory(testName);
				}

				public DirectoryInfo DiffTestDirectory(string testName)
				{
					return _diffFilesDirectory.CreateSubdirectory(testName);
				}

				public DirectoryInfo DiffFilesDirectory
				{
					get { return _diffFilesDirectory; }
				}

				public DirectoryInfo TestingFilesDirectory
				{
					get { return _testingFilesDirectory; }
				}

				public DirectoryInfo StableFilesDirectory
				{
					get { return _stableFilesDirectory; }
				}
			}
		}
	}
}
