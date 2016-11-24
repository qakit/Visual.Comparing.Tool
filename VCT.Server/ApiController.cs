using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
using VCT.Sdk;
using VCT.Sdk.Extensions;
using VCT.Server.Entities;
using VCT.Server.EqualityComparers;
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
		[HttpPost]
		[Route("{projectId}/{testId}/{fileName}/stable/hash")]
		public HttpResponseMessage GetStableTestFileHash(string projectId, string testId, string fileName)
		{
			using (var storage = new StorageContext())
			{
				var test = storage.Tests.FirstOrDefault(t => t.Name == testId);
				if (test == null) return new HttpResponseMessage(HttpStatusCode.NoContent);

				var stableFile =
					storage.StableFiles.FirstOrDefault(
						sf => sf.TestName == testId && sf.ProjectName == projectId && sf.FileName == fileName);

				if (stableFile == null) return new HttpResponseMessage(HttpStatusCode.NoContent);

				var hash = Utils.HashFromBase64(stableFile.Value);

				var content = new StringContent(hash);
				return new HttpResponseMessage { Content = content };
			}
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
//			//file server changes
//			var suiteDirectory = Storage.Project(projectId).Suite(suiteId);
//			//clear diff files (as they are not necessary any more)
//			suiteDirectory.DiffTestDirectory(testId).ClearContent();
//
//			//copy content of the testing files for sepcified test to stable dirrectory
//			var testingDirectory = suiteDirectory.TestingTestDirectory(testId);
//			var stableFilesDirectory = suiteDirectory.StableTestDirectory(testId);
//
//			//update stable files in current test run
//			testingDirectory.CopyTo(stableFilesDirectory);
//			//copy and replace stable files in global storage for 'Stable Files'
//			testingDirectory.CopyTo(Storage.Project(projectId).StableTestDirectory(testId));
//
			using (var ctx = new StorageContext())
			{
				var suite = ctx.Suites.FirstOrDefault(s => s.Name == suiteId);
				var test = ctx.Tests.FirstOrDefault(t => t.Name == testId && t.SuiteId == suite.Id);
				var stableFiles = ctx.StableFiles.Where(sf => sf.TestName == test.Name && sf.ProjectName == projectId);

				//clear all diff files
				test.DiffFiles.Clear();
				//save changes
				ctx.SaveChanges();

				foreach (ArtifactFile artifactFile in test.TestingFiles)
				{
					var stableFile =
						ctx.StableFiles.FirstOrDefault(
							sf => sf.FileName == artifactFile.FileName && sf.TestName == test.Name && sf.ProjectName == projectId);
					if (stableFile == null)
					{
						ctx.StableFiles.Add(new StableFile
						{
							FileName = artifactFile.FileName,
							ProjectName = projectId,
							TestName = test.Name,
							Value = artifactFile.Value
						});
						ctx.SaveChanges();
					}
					else
					{
						stableFile.Value = artifactFile.Value;
						ctx.SaveChanges();
					}
				}

//				//now add stable files for current test and project
//
//				foreach (FileInfo imageFile in images)
//				{
//					ctx.StableFiles.Add(new StableFile
//					{
//						FullPath = imageFile.FullName,
//						Name = imageFile.Name,
//						ProjectName = projectId,
//						//TODO move it to separate method
//						RelativePath = "..\\" + MakeRelativePath(Storage.StorageDirectory.FullName, imageFile.FullName) + "?date=" +
//						               DateTime.Now,
//						TestName = test.Name
//					});
//					ctx.SaveChanges();
//				}
			}
			

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
		public async Task<IHttpActionResult> PostTesting(string projectId, string suiteId, string testId)
		{
			//TODO
			//take files from client
			//find out stable
			//if no stable put testing to folder and made related records in db
			//if stable => compare
			//add diff/testing/stable

			//1. Get files
			var outputDirectory = Storage.Project(projectId).Suite(suiteId).TestingTestDirectory(testId);

			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				return BadRequest("Unsupported media type");
			}
			if (!outputDirectory.Exists) outputDirectory.Create();
			var multiPartFormDataStreamProvider = new UploadMultipartFormProvider(outputDirectory.FullName);
			await Request.Content.ReadAsMultipartAsync(multiPartFormDataStreamProvider);

			//2. Compare each file to stable
			var files = outputDirectory.GetImageFiles();
			using (var ctx = new StorageContext())
			{
				//you could have old test here for previous suite
				//this must be handled in future
				var suite = ctx.Suites.FirstOrDefault(s => s.Name == suiteId);
				//find current test
				var test = ctx.Tests.FirstOrDefault(t => t.Name == testId && t.SuiteId == suite.Id);
				//if not current test create
				if (test == null)
				{
					ctx.Tests.Add(new Test
					{
						Name = testId , 
						SuiteId = suite.Id,
						Passed = false
					});
					ctx.SaveChanges();
					test = ctx.Tests.FirstOrDefault(t => t.Name == testId && t.SuiteId == suite.Id);
				}

				foreach (FileInfo testingFile in files)
				{
					//get stable file from 
					var stableFile = ctx.StableFiles.FirstOrDefault(sf => sf.TestName == test.Name && sf.FileName == testingFile.Name && sf.ProjectName == projectId);
					//if no stable file when just put testing file to current test
					if (stableFile == null)
					{
						var testingFileArtifact = new ArtifactFile
						{
							FileName = testingFile.Name,
							Value = Utils.ImageToBase64(testingFile),
							Test = test,
							Type = ArtifactType.Testing
						};

						//if artifact not exist insert as testing file for current test
						if (!test.TestingFiles.ToList().Contains(testingFileArtifact, new ArtifactFileComparer()))
						{
							//insert
							ctx.ArtifactFiles.Add(testingFileArtifact);
							test.TestingFiles.Add(testingFileArtifact);

							//SAVE
							ctx.SaveChanges();
						}
						//ignore if artifact exists
					}
					else
					{
						//there is a stable file

						//first make sure that there is no any artifact already
						//first check testing
						var testingFileArtifact = new ArtifactFile
						{
							FileName = testingFile.Name,
							Value = Utils.ImageToBase64(testingFile),
							Test = test,
							Type = ArtifactType.Testing
						};

						//if artifact not exist insert as testing file for current test
						if (!test.TestingFiles.AsEnumerable().Contains(testingFileArtifact, new ArtifactFileComparer()))
						{
							//insert
							ctx.ArtifactFiles.Add(testingFileArtifact);
							test.TestingFiles.Add(testingFileArtifact);
							//SAVE
							ctx.SaveChanges();
						}
						//now check diff file
						var diffDirectory = new DirectoryInfo(@"C:\projects\Visual.Comparing.Tool\Output");
						var diffFile = new FileInfo(Path.Combine(
							diffDirectory.FullName,
							testingFile.Name));

						//first compare
						var diff = ImageFileComparer.GenerateDiffFile(Utils.Base64ToImage(stableFile.Value), Image.FromFile(testingFile.FullName), diffFile);

						var diffFileArtifact = new ArtifactFile
						{
							FileName = diffFile.Name,
							Value = Utils.ImageToBase64(diff, ImageFormat.Png),
							Test = test,
							Type = ArtifactType.Diff
						};

						//insert
						ctx.ArtifactFiles.Add(diffFileArtifact);
						test.DiffFiles.Add(diffFileArtifact);
						//SAVE
						ctx.SaveChanges();

					}
				}
			}
			return Ok(new { Message = string.Format("All files have been uploaded successfully to {0} directory", outputDirectory.FullName) });
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
			var dateTime = DateTime.Now;
			string inceptionTime = dateTime.ToString(DateFormat);

			Storage.Project(projId).Suite(inceptionTime);
			using (var context = new StorageContext())
			{
				var project = context.Projects.FirstOrDefault(p => p.Name == projId);
				if (project == null)
				{
					project = context.Projects.Add(new Entities.Project { Name = projId });
					context.SaveChanges();
				}

				var suite = context.Suites.FirstOrDefault(s => s.Name == inceptionTime);
				if (suite == null)
				{
					suite = context.Suites.Add(new Entities.Suite { Name = inceptionTime, StartTime = dateTime, ProjectId = project.Id });
					context.SaveChanges();
				}
				return suite.Name;
			}
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
				var suites = Storage.Project(projectId).Suites.OrderBy(s => s.DateStarted);

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

			return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Backup for stable files completed successfully") };
		}

		/// <summary>
		/// Removes suite from project
		/// </summary>
		/// <param name="projectId"></param>
		/// <param name="suiteId"></param>
		/// <returns></returns>
		[HttpDelete]
		[Route("{projectId}/{suiteId}/delete")]
		public HttpResponseMessage DeleteSuite(string projectId, string suiteId)
		{
			Storage.Project(projectId).Suite(suiteId).Delete();
			return new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(string.Format("Suite {0} has been removed from {1} project.", suiteId, projectId))
			};
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
			using (var context = new StorageContext())
			{
				foreach (Entities.Project project in context.Projects)
				{
					var suitesCount = (from suite in context.Suites
									   where suite.ProjectId == project.Id
									   select suite).Count();
					projects.Add(new Project { Id = (int)project.Id, Name = project.Name, SuitesCount = suitesCount });
				}
			}
			return projects;


			//			var projectDirectories = Storage.Projects;
			//			//TODO remove id after all changes
			//			int id = 1;
			//			foreach (Storage.StorageProject projectDirectory in projectDirectories)
			//			{
			//				var suitesCount = projectDirectory.Suites.Count;
			//				projects.Add(new Project { Id = id, Name = projectDirectory.Directory.Name, SuitesCount = suitesCount });
			//				id++;
			//			}
			//			return projects;
		}

		private List<Suite> GetSuitesInformation(string projectId)
		{
			var suitesInfo = new List<Suite>();
			using (var ctx = new StorageContext())
			{
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectId);
				var suites = ctx.Suites.Where(s => s.ProjectId == project.Id).OrderByDescending(s => s.StartTime).ToList();

				int suiteId = suites.Count;

				foreach (Entities.Suite suite in suites)
				{
					var tests = ctx.Tests.Where(t => t.SuiteId == suite.Id).ToList();
					var failed = tests.Count(t => !t.Passed);
					var passed = tests.Count(t => t.Passed);

					suitesInfo.Add(new Suite
					{
						DateCompleted = suite.EndTime.ToString(),
						DateStarted = suite.StartTime.ToString(),
						Failed = failed,
						Passed = passed,
						Id = suiteId,
						Name = suite.Name
					});
					suiteId--;
				}
			}
			return suitesInfo;
		}

		private List<FailedTest> GetFailedTestsInformation(string projectId, string suiteId)
		{
			var tests = new List<FailedTest>();
			using (var ctx = new StorageContext())
			{
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectId);
				var suite = ctx.Suites.FirstOrDefault(s => s.ProjectId == project.Id && s.Name == suiteId);
				var failedTests = ctx.Tests.Where(t => t.SuiteId == suite.Id && !t.Passed).ToList();

				
				foreach (Test failedTest in failedTests)
				{
					var artifactsCollection = new List<Tuple>();
					var stableFiles = ctx.StableFiles.Where(sf => sf.TestName == failedTest.Name && sf.ProjectName == project.Name).ToList();

					var listOfArtifacts =
						failedTest.DiffFiles.Select(df => df.FileName)
							.Union(failedTest.TestingFiles.Select(tf => tf.FileName)).Union(stableFiles.Select(sf => sf.FileName));

					foreach (string singleArtifact in listOfArtifacts)
					{
						var stableFile = stableFiles.FirstOrDefault(sf => sf.FileName == singleArtifact);
						var testingFile = failedTest.TestingFiles.FirstOrDefault(tf => tf.FileName == singleArtifact);
						var diffFile = failedTest.DiffFiles.FirstOrDefault(df => df.FileName == singleArtifact);

						var stableFileDescription = stableFile == null
							? EmptyArtifact()
							: new File
							{
								Name = stableFile.FileName,
								Value = stableFile.Value,
							};

						artifactsCollection.Add(new Tuple
						{
							DiffFile = GetAtrifactDescription(diffFile),
							StableFile = stableFileDescription,
							TestingFile = GetAtrifactDescription(testingFile),
						});
					}
					tests.Add(new FailedTest
					{
						TestName = failedTest.Name,
						Artifacts = artifactsCollection
					});
				}
			}
			return tests;
		}

		/// <summary>
		/// gets the desctiption of the current artifact file
		/// </summary>
		/// <param name="artifactFile">Artifact as <see cref="FileInfo"/></param>
		/// <returns><see cref="File"/> with Name (empty if null), Path (empty if null)</returns>
		private File GetAtrifactDescription(ArtifactFile artifact)
		{
			if (artifact == null) return EmptyArtifact();
			return new File
			{
				Name = artifact.FileName,
				Value = artifact.Value
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
				Value = ""
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
