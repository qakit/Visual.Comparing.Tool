using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using VCT.Server.Helpers;

namespace VCT.Server
{
	[RoutePrefix("")]
	public class ApiController : System.Web.Http.ApiController
	{
		private readonly Storage Storage = new Storage();

		static readonly string DateFormat = ConfigurationManager.AppSettings["dateFormat"];

		/// <summary>
		/// Gets information about all projects in the storage
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[Route("projects")]
		public JsonResult<List<Project>> GetProjects()
		{
			return Json(GetProjectsInformation());
		}

		/// <summary>
		/// Gets wide information about all suites in the current project
		/// </summary>
		/// <param name="projectId">Project Id</param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectId}/suites")]
		public JsonResult<List<Suite>> GetSuites(string projectId)
		{
			return Json(GetSuitesInformation(projectId));
		}

		/// <summary>
		/// Gets information about all failed tests in specified suite run for specific project
		/// </summary>
		/// <param name="projectId"></param>
		/// <param name="suiteId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectId}/{suiteId}/tests")]
		public JsonResult<List<FailedTest>> GetFailedTests(string projectId, string suiteId)
		{
			return Json(GetFailedTestsInformation(projectId, suiteId));
		}

		/// <summary>
		/// Get all stable files for specified test
		/// </summary>
		/// <param name="projectId">project id/project name to whcih test belong</param>
		/// <param name="testId">test id /name of the test/ in the project</param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectId}/{testId}/stable")]
		public HttpResponseMessage GetStableTestFiles(string projectId, string testId)
		{
			return WebHelpers.SendFilesToClient(Storage.Project(projectId).StableFiles.GetDirectories(testId).FirstOrDefault());
		}

		/// <summary>
		/// Get the sable version of the specified file
		/// </summary>
		/// <param name="projectId">project id/project name to whcih test belong</param>
		/// <param name="testId">test id /name of the test/ in the project</param>
		/// <param name="fileName">name of the file to which stable file must be returned</param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectId}/{testId}/{fileName}/stable")]
		public HttpResponseMessage GetStableTestFile(string projectId, string testId, string fileName)
		{
			return WebHelpers.SendFilesToClient(Storage.Project(projectId).StableFiles.GetDirectories(testId).FirstOrDefault(), fileName);
		}

		/// <summary>
		/// Gets the hash for the stable file with specified name
		/// </summary>
		/// <param name="projectId">project id/project name to which test belong</param>
		/// <param name="testId">test id /name of the test/ in the project</param>
		/// <param name="fileName">name of the file to which hash must be returned</param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectId}/{testId}/{fileName}/stable/hash")]
		public HttpResponseMessage GetStableTestFileHash(string projectId, string testId, string fileName)
		{
			return WebHelpers.SendHashToClient(Storage.Project(projectId).StableFiles.GetDirectories(testId).FirstOrDefault(), fileName);
		}

		/// <summary>
		/// Post stable file for specified test to the current suite run
		/// </summary>
		/// <param name="projectId">project id/project name to which test belong</param>
		/// <param name="suiteId">suite id/suite name to which test belong</param>
		/// <param name="testId">test id /name of the test/ in the project</param>
		/// <returns></returns>
		[HttpPost]
		[Route("{projectId}/{suiteId}/{testId}/stable")]
		public Task<IHttpActionResult> PostStable(string projectId, string suiteId, string testId)
		{
			return GetFilesFromClient(Storage.Project(projectId).Suite(suiteId).StableTestDirectory(testId));
		}

		[HttpPost]
		[Route("{projectId}/{suiteId}/{testId}/accept")]
		public HttpResponseMessage AcceptStable(string projectId, string suiteId, string testId)
		{
			var suiteDirectory = Storage.Project(projectId).Suite(suiteId);
			//clear diff files (as they are not necessary any more)
			suiteDirectory.DiffTestDirectory(testId).ClearContent();

			//copy content of the testing files for sepcified test to stable dirrectory
			var testingDirectory = suiteDirectory.TestingTestDirectory(testId);
			var stableFilesDirectory = suiteDirectory.StableTestDirectory(testId);

			//update stable files in current test run
			testingDirectory.CopyTo(stableFilesDirectory);
			//copy and replace stable files in global storage for 'Stable Files'
			testingDirectory.CopyTo(Storage.Project(projectId).StableTestDirectory(testId));

			return new HttpResponseMessage { Content = new StringContent("All files have been moved from testing to stable directory. And were updated in global storage"), StatusCode = HttpStatusCode.OK };
		}

		/// <summary>
		/// Gets the files from the client and put them to the testing folder for the specified suite/run
		/// </summary>
		/// <param name="projectId">project id/project name to which test belong</param>
		/// <param name="suiteId">suite id/suite name to which test belong</param>
		/// <param name="testId">test id /name of the test/ in the project</param>
		/// <returns></returns>
		[HttpPost]
		[Route("{projectId}/{suiteId}/{testId}/testing")]
		public Task<IHttpActionResult> PostTesting(string projectId, string suiteId, string testId)
		{
			return GetFilesFromClient(Storage.Project(projectId).Suite(suiteId).TestingTestDirectory(testId));
		}

		/// <summary>
		/// Gets the files from the client and put them to the diff folder for the specified suite/run
		/// </summary>
		/// <param name="projectId">project id/project name to which test belong</param>
		/// <param name="suiteId">suite id/suite name to which test belong</param>
		/// <param name="testId">test id /name of the test/ in the project</param>
		/// <returns></returns>
		[HttpPost]
		[Route("{projectId}/{suiteId}/{testId}/diff")]
		public Task<IHttpActionResult> PostDiff(string projectId, string suiteId, string testId)
		{
			return GetFilesFromClient(Storage.Project(projectId).Suite(suiteId).DiffTestDirectory(testId));
		}

		[HttpPost]
		[Route("{projId}/suite/start")]
		public string SuiteStart(string projId)
		{
			string inceptionTime = DateTime.Now.ToString(DateFormat);

			Storage.Project(projId).Suite(inceptionTime);

			return inceptionTime;
		}

		[HttpPost]
		[Route("{projId}/suite/stop")]
		public void SuiteStop(string projId)
		{
			string finishTime = DateTime.Now.ToString(DateFormat);
			Console.WriteLine("[{0}] Suite stopped at {1}", projId, finishTime);
		}

		/// <summary>
		/// Back up all stable files from previous runs and put them to the latest stable folder.
		/// Necessary when we want to remove redundant files from system but have to save stable files
		/// </summary>
		[HttpGet]
		[Route("{projectId}/stable/backup")]
		public HttpResponseMessage BackupStableFilesForLatestBuild(string projectId)
		{
			try
			{
				//get suites and sort them in asc order
				var suites = Storage.Project(projectId).Suites;
				foreach (Storage.StorageProject.ProjectSuite suite in suites)
				{
					var tests = suite.StableFilesDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly);
					foreach (DirectoryInfo test in tests)
					{
						var copyToDir = new DirectoryInfo(Path.Combine(Storage.Project(projectId).StableFiles.FullName, test.Name));
						test.CopyTo(copyToDir);
					}
				}
			}
			catch (Exception e)
			{
				return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError, Content = new StringContent(string.Format("Backup for stable files falied: {0}", e.Message)) };
			}

			return new HttpResponseMessage {StatusCode = HttpStatusCode.OK, Content = new StringContent("Backup for stable files completed successfully")};
		}

		/// <summary>
		/// Get files from client and save them to specified directory
		/// </summary>
		/// <param name="outputDirectory">Directory to save files from client</param>
		/// <returns></returns>
		public async Task<IHttpActionResult> GetFilesFromClient(DirectoryInfo outputDirectory)
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				return BadRequest("Unsupported media type");
			}
			if (!outputDirectory.Exists) outputDirectory.Create();
			var multiPartFormDataStreamProvider = new UploadMultipartFormProvider(outputDirectory.FullName);
			await Request.Content.ReadAsMultipartAsync(multiPartFormDataStreamProvider);
			return Ok(new { Message = string.Format("All files have been uploaded successfully to {0} directory", outputDirectory.FullName) });
		}

		private List<Project> GetProjectsInformation()
		{
			var projects = new List<Project>();
			var projectDirectories = Storage.Projects;
			//TODO remove id after all changes
			int id = 1;
			foreach (Storage.StorageProject projectDirectory in projectDirectories)
			{
				var suitesCount = projectDirectory.Suites.Count;
				projects.Add(new Project { Id = id, Name = projectDirectory.Directory.Name, SuitesCount = suitesCount });
				id++;
			}
			return projects;
		}

		private List<Suite> GetSuitesInformation(string projectId)
		{
			var suites = new List<Suite>();
			var suiteDirectories =
				Storage.Project(projectId).Suites.OrderBy(d => d.Directory.Name).ToList();

			int suiteId = suiteDirectories.Count;

			foreach (Storage.StorageProject.ProjectSuite suiteDirectory in suiteDirectories)
			{
				var failed = GetFailedTests(suiteDirectory);
				var passed = GetPassedTests(suiteDirectory);

				suites.Add(new Suite
				{
					DateCompleted = suiteDirectory.DateCompleted,
					DateStarted = suiteDirectory.DateStarted,
					Failed = failed.Length,
					Passed = passed.Length,
					Id = suiteId,
					Name = suiteDirectory.Directory.Name
				});

				suiteId--;
			}

			return suites;
		}

		private List<FailedTest> GetFailedTestsInformation(string projectId, string suiteId)
		{
			var tests = new List<FailedTest>();
			var suiteDirectory = Storage.Project(projectId).Suite(suiteId);

			if (suiteDirectory == null) return tests;

			foreach (DirectoryInfo diffDirectory in suiteDirectory.DiffFilesDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly))
			{
				//Get stable directory path
				var stableFolderPath = suiteDirectory.StableFilesDirectory.GetDirectories(diffDirectory.Name,
					SearchOption.TopDirectoryOnly).FirstOrDefault();

				//Get testing directory path
				var testingFolderPath =
					suiteDirectory.TestingFilesDirectory.GetDirectories(diffDirectory.Name, SearchOption.TopDirectoryOnly)
						.FirstOrDefault();

				//Get all images from all directories
				var stableImages = Utils.GetResultImages(stableFolderPath);
				var testingImages = Utils.GetResultImages(testingFolderPath);
				var diffImages = Utils.GetResultImages(diffDirectory);

				//find longest list as all directories might have different number of images so we need to find max one
				//to make sure that we no iterate throught smallest one
				//how to avoid this? merge three lists into one excluding all repeated images?

				IEnumerable<FileInfo> longestList = Utils.FindLongest(stableImages, testingImages, diffImages);

				var artifactsCollection = new List<Tuple>();

				foreach (FileInfo imageFile in longestList)
				{
					var stableFile = Utils.GetFileByName(stableImages, imageFile.Name);
					var testingFile = Utils.GetFileByName(testingImages, imageFile.Name);
					var diffFile = Utils.GetFileByName(diffImages, imageFile.Name);

					var stableData = GetAtrifactDescription(stableFile);
					var testingData = GetAtrifactDescription(testingFile);
					var diffData = GetAtrifactDescription(diffFile);

					artifactsCollection.Add(new Tuple
					{
						StableFile = stableData,
						TestingFile = testingData,
						DiffFile = diffData
					});
				}

				tests.Add(new FailedTest
				{
					TestName = diffDirectory.Name,
					Artifacts = artifactsCollection
				});
			}

			return tests;
		}

		/// <summary>
		/// gets the desctiption of the current artifact file
		/// </summary>
		/// <param name="artifactFile">Artifact as <see cref="FileInfo"/></param>
		/// <returns><see cref="File"/> with Name (empty if null), Path (empty if null)</returns>
		private File GetAtrifactDescription(FileInfo artifactFile)
		{
			if (artifactFile == null)
				return EmptyArtifact();

			return new File
			{
				Name = artifactFile.Name,
				Path = artifactFile.FullName,
				RelativePath = "..\\" + MakeRelativePath(Storage.StorageDirectory.FullName, artifactFile.FullName)
			};
		}

		public static String MakeRelativePath(String fromPath, String toPath)
		{
			if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
			if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

			Uri fromUri = new Uri(fromPath);
			Uri toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

		private File EmptyArtifact()
		{
			return new File
			{
				Name = "",
				Path = "",
				RelativePath = ""
			};
		}

		private string[] GetFailedTests(Storage.StorageProject.ProjectSuite suite)
		{
			return suite.DiffFilesDirectory.GetDirectories().Select(d => d.Name).ToArray();
		}

		private string[] GetPassedTests(Storage.StorageProject.ProjectSuite suite)
		{
			var stableDirectories = suite.StableFilesDirectory.GetDirectories();
			var testingDirectories = suite.TestingFilesDirectory.GetDirectories();
			var failedDirectories = suite.DiffFilesDirectory.GetDirectories();

			var distinctList =
				stableDirectories.Select(d => d.Name)
					.ToList()
					.Concat(testingDirectories.Select(d => d.Name).ToList()).Distinct();
			var passes = (from dir in distinctList
						  where !failedDirectories.Select(d => d.Name).ToList().Contains(dir)
						  select dir).ToArray();

			return passes;
		}	
	}
}
