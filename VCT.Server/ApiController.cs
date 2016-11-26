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
		/// <param name="projectName">Project Id</param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectName}/suites")]
		public JsonResult<List<Suite>> GetSuites(string projectName)
		{
			return Json(GetSuitesInformation(projectName));
		}

		/// <summary>
		/// Gets information about all failed tests in specified suite run for specific project
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="suiteName"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("{projectName}/{suiteName}/tests")]
		public JsonResult<List<FailedTest>> GetFailedTests(string projectName, string suiteName)
		{
			return Json(GetFailedTestsInformation(projectName, suiteName));
		}

		/// <summary>
		/// Gets the hash for the stable file with specified name
		/// </summary>
		/// <param name="projectName">project id/project name to which test belong</param>
		/// <param name="testName">test id /name of the test/ in the project</param>
		/// <param name="fileName">name of the file to which hash must be returned</param>
		/// <returns></returns>
		[HttpPost]
		[Route("{projectName}/{testName}/{fileName}/stable/hash")]
		public HttpResponseMessage GetStableTestFileHash(string projectName, string testName, string fileName)
		{
			var testInfo = JsonConvert.DeserializeObject<TestInfo>(Request.Content.ReadAsStringAsync().Result);

			using (var ctx = new StorageContext())
			{
				//find environment first
				var environment = GetEnvironment(testInfo.Browser, testInfo.WindowSize);
				//find project and test
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
				var test = ctx.Tests.FirstOrDefault(t => t.Name == testName && t.ProjectId == project.Id);
				//if no test return no content
				if (test == null) return new HttpResponseMessage(HttpStatusCode.NoContent);

				//find stable file
				var stableFile = ctx.StableFiles.FirstOrDefault(sf => sf.TestId == test.Id && sf.EnvironmentId == environment.Id);
				if (stableFile == null) return new HttpResponseMessage(HttpStatusCode.NoContent);

				var hash = Utils.HashFromBase64(stableFile.File);

				var content = new StringContent(hash);
				return new HttpResponseMessage { Content = content };
			}
		}

		private Entities.Environment GetEnvironment(string browser, Size windowSize)
		{
			using (var ctx = new StorageContext())
			{
				var browserEntity =
					ctx.Browsers.FirstOrDefault(b => b.Name == browser) ??
					ctx.Browsers.Add(new Browser {Name = browser});
				ctx.SaveChanges();

				var resolutionEntity =
					ctx.Resolutions.FirstOrDefault(r => r.Width == windowSize.Width && r.Height == windowSize.Height) ??
					ctx.Resolutions.Add(new Resolution {Height = windowSize.Height, Width = windowSize.Width});
				ctx.SaveChanges();

				var environment =
					ctx.Environments.FirstOrDefault(env => env.BrowserId == browserEntity.Id && env.ResolutionId == resolutionEntity.Id) ??
					ctx.Environments.Add(new Entities.Environment {BrowserId = browserEntity.Id, ResolutionId = resolutionEntity.Id});
				ctx.SaveChanges();
				return environment;
			}
		}

		[HttpPost]
		[Route("{projectName}/{suiteName}/{testName}/status/passed")]
		public void MarkTestAsPassed(string projectName, string suiteName, string testName)
		{
			var testInfo = JsonConvert.DeserializeObject<TestInfo>(Request.Content.ReadAsStringAsync().Result);

			using (var ctx = new StorageContext())
			{
				var environment = GetEnvironment(testInfo.Browser, testInfo.WindowSize);
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
				var suite = ctx.Suites.FirstOrDefault(s => s.Name == suiteName && s.ProjectId == project.Id);
				var test = ctx.Tests.FirstOrDefault(t => t.Name == testName && t.ProjectId == project.Id);
				
				var runningTest = new RunningTest
				{
					EnvironmentId = environment.Id,
					Passed = true,
					SuiteId = suite.Id,
					TestId = test.Id
				};

				if (!ctx.RunningTests.ToList().Contains(runningTest, new RunningTestComparer()))
				{
					ctx.RunningTests.Add(runningTest);
					ctx.SaveChanges();
				}
			}
		}

		[HttpPost]
		[Route("{projectName}/{suiteName}/{testName}/{environmentId}/accept")]
		public HttpResponseMessage AcceptStable(string projectName, string suiteName, string testName, int environmentId)
		{
			using (var ctx = new StorageContext())
			{
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
				var suite = ctx.Suites.FirstOrDefault(s => s.Name == suiteName && s.ProjectId == project.Id);
				var test = ctx.Tests.FirstOrDefault(t => t.Name == testName && t.ProjectId == project.Id);
				var runningTest =
					ctx.RunningTests.FirstOrDefault(
						t => t.SuiteId == suite.Id && t.TestId == test.Id && t.EnvironmentId == environmentId);

				foreach (RunningTestResult runningTestResult in runningTest.TestResults)
				{
					//clear diff file first
					runningTestResult.DiffFile = "";

					//get testing and move it to stable
					var stableFile =
						ctx.StableFiles.FirstOrDefault(
							sf => sf.TestId == test.Id && sf.EnvironmentId == environmentId && sf.Name == runningTestResult.Name);
					if (stableFile == null)
					{
						stableFile = ctx.StableFiles.Add(new StableFile
						{
							EnvironmentId = environmentId,
							Name = runningTestResult.Name,
							TestId = test.Id
						});
						ctx.SaveChanges();
					}
					stableFile.File = runningTestResult.TestingFile;
					ctx.SaveChanges();
				}
			}
			

			return new HttpResponseMessage { Content = new StringContent("All files have been moved from testing to stable directory. And were updated in global storage"), StatusCode = HttpStatusCode.OK };
		}

		/// <summary>
		/// Gets the files from the client and put them to the testing folder for the specified suite/run
		/// </summary>
		/// <param name="projectName">project id/project name to which test belong</param>
		/// <param name="suiteName">suite id/suite name to which test belong</param>
		/// <param name="testName">test id /name of the test/ in the project</param>
		/// <returns></returns>
		[HttpPost]
		[Route("{projectName}/{suiteName}/{testName}/testing")]
		public async Task<IHttpActionResult> PostTesting(string projectName, string suiteName, string testName)
		{
			//1. Get files
			var outputDirectory = Storage.Project(projectName).Suite(suiteName).TestingTestDirectory(testName);

			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				return BadRequest("Unsupported media type");
			}
			if (!outputDirectory.Exists) outputDirectory.Create();
			var multiPartFormDataStreamProvider = new UploadMultipartFormProvider(outputDirectory.FullName);
			await Request.Content.ReadAsMultipartAsync(multiPartFormDataStreamProvider);

			var stringContent = Request.Headers.GetValues("TestInfo").FirstOrDefault();
			var testInfo = JsonConvert.DeserializeObject<TestInfo>(stringContent);

			//2. Compare each file to stable
			var files = outputDirectory.GetImageFiles();
			using (var ctx = new StorageContext())
			{
				//you could have old test here for previous suite
				//this must be handled in future
				var environment = GetEnvironment(testInfo.Browser, testInfo.WindowSize);
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
				var suite = ctx.Suites.FirstOrDefault(s => s.Name == suiteName);

				//find current test
				var test = ctx.Tests.FirstOrDefault(t => t.Name == testName && t.ProjectId == project.Id);
				RunningTest runningTest = null;

				//if there is no unique test description let's create it
				if (test == null)
				{
					test = ctx.Tests.Add(new Test
					{
						Name = testName , 
						ProjectId = project.Id
					});
					ctx.SaveChanges();
					//if there were not such test let's create and corresponding running test in suite collection
					
				}

				runningTest = suite.Tests.FirstOrDefault(t => t.TestId == test.Id && t.EnvironmentId == environment.Id);
				if (runningTest == null)
				{
					runningTest = ctx.RunningTests.Add(new RunningTest
					{
						Passed = false,
						TestId = test.Id,
						SuiteId = suite.Id,
						EnvironmentId = environment.Id,
					});
					ctx.SaveChanges();
				}
				

				foreach (FileInfo testingFile in files)
				{
					//get stable file from 
					var stableFile =
						ctx.StableFiles.FirstOrDefault(
							sf => sf.Name == testingFile.Name && sf.TestId == test.Id && sf.EnvironmentId == environment.Id);

					//create artifact and verify its existance in running test
					var runningTestResult = new RunningTestResult
					{
						TestingFile = Utils.ImageToBase64(testingFile),
						Name = testingFile.Name,
						DiffFile = "",
						RunningTestId = runningTest.Id
					};

					//if file not exists add it
					if (!runningTest.TestResults.Contains(runningTestResult, new RunningTestResultComparer()))
					{
						runningTest.TestResults.Add(runningTestResult);
						ctx.SaveChanges();
					}

					//if no stable file when just put testing file to current test
					if (stableFile != null)
					{
						//now check diff file
						var diffDirectory = new DirectoryInfo(@"C:\projects\Visual.Comparing.Tool\VCT.Server\Output");
						if(diffDirectory.Exists) diffDirectory.ClearContent();

						var diffFile = new FileInfo(Path.Combine(
							diffDirectory.FullName,
							testingFile.Name));

						//first compare
						var diff = ImageFileComparer.GenerateDiffFile(Utils.Base64ToImage(stableFile.File), Image.FromFile(testingFile.FullName), diffFile);

						//add diff to test result
						runningTest.TestResults.Where(tr => tr.Name == testingFile.Name).FirstOrDefault().DiffFile =
							Utils.ImageToBase64(diff, ImageFormat.Png);
						runningTest.Passed = false;
						//save changes
						ctx.SaveChanges();
					}
				}
			}
			return Ok(new { Message = string.Format("All files have been uploaded successfully to {0} directory", outputDirectory.FullName) });
		}

		[HttpPost]
		[Route("{projectName}/suite/start")]
		public string SuiteStart(string projectName)
		{
			var dateTime = DateTime.Now;
			string inceptionTime = dateTime.ToString(DateFormat);

			Storage.Project(projectName).Suite(inceptionTime);
			using (var context = new StorageContext())
			{
				var project = context.Projects.FirstOrDefault(p => p.Name == projectName);
				if (project == null)
				{
					project = context.Projects.Add(new Entities.Project { Name = projectName });
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
		[Route("{projectName}/{suiteName}/suite/stop")]
		public void SuiteStop(string projectName, string suiteName)
		{
			var finishTime = DateTime.Now;
			using (var ctx = new StorageContext())
			{
				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
				var suite = ctx.Suites.FirstOrDefault(s => s.Name == suiteName && s.ProjectId == project.Id);
				suite.EndTime = finishTime;
				ctx.SaveChanges();
			}
			Console.WriteLine("[{0}] Suite stopped at {1}", suiteName, finishTime.ToString(DateFormat));
		}

		/// <summary>
		/// Back up all stable files from previous runs and put them to the latest stable folder.
		/// Necessary when we want to remove redundant files from system but have to save stable files
		/// </summary>
		[HttpGet]
		[Route("{projectName}/stable/backup")]
		public HttpResponseMessage BackupStableFilesForLatestBuild(string projectName)
		{
			//TODO migratio nhere from file storage to db
			try
			{
				using (var ctx = new StorageContext())
				{
					//find project if any. If not, create
					var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
					var browser = ctx.Browsers.First(b => b.Name == "chrome");
					var resolution = ctx.Resolutions.Add(new Resolution {Height = 1024, Width = 1000});
					ctx.SaveChanges();
					var environment =
						ctx.Environments.Add(new Entities.Environment {BrowserId = browser.Id, ResolutionId = resolution.Id});

					if (project == null)
					{
						project = ctx.Projects.Add(new Entities.Project {Name = projectName});
						ctx.SaveChanges();
					}
					//create list of tests
					foreach (DirectoryInfo stableFileDirectory in Storage.Project(projectName).StableFiles.GetDirectories())
					{
						var test = ctx.Tests.Add(new Test {Name = stableFileDirectory.Name, ProjectId = project.Id});
						ctx.SaveChanges();
						foreach (FileInfo imageFile in stableFileDirectory.GetImageFiles())
						{
							ctx.StableFiles.Add(new StableFile
							{
								EnvironmentId = environment.Id,
								File = Utils.ImageToBase64(imageFile),
								Name = imageFile.Name,
								TestId = test.Id
							});
							ctx.SaveChanges();
						}
					}
					//save changes

					//get suites and sort them in asc order
					var fileStorageSuites = Storage.Project(projectName).Suites.OrderBy(s => s.DateStarted);
					foreach (Storage.StorageProject.ProjectSuite fileStorageSuite in fileStorageSuites)
					{
						//add suite first
						var suite = ctx.Suites.Add(new Entities.Suite
						{
							Name = fileStorageSuite.Directory.Name,
							ProjectId = project.Id,
							StartTime = fileStorageSuite.DateStarted
						});
						ctx.SaveChanges();
						foreach (var testDirectory in fileStorageSuite.TestingFilesDirectory.GetDirectories())
						{
							var runningTest = ctx.RunningTests.Add(new RunningTest
							{
								SuiteId = suite.Id,
								EnvironmentId = environment.Id,
								Passed = false, //how to count??
								TestId = ctx.Tests.FirstOrDefault(t => t.Name == testDirectory.Name).Id
							});
							ctx.SaveChanges();
							foreach (FileInfo imageFile in testDirectory.GetImageFiles())
							{
								var diffFile = fileStorageSuite.DiffFilesDirectory.GetImageFiles().FirstOrDefault(f => f.Name == imageFile.Name);

								var result = ctx.RunningTestResults.Add(new RunningTestResult
								{
									TestingFile = Utils.ImageToBase64(imageFile),
									DiffFile = diffFile == null ? "" : Utils.ImageToBase64(diffFile),
									Name = imageFile.Name,
									RunningTestId = runningTest.Id
								});
								ctx.SaveChanges();
								runningTest.TestResults.Add(result);
								ctx.SaveChanges();
							}
						}
					}
				}
//			try
//			{
//				//get suites and sort them in asc order
//				var suites = Storage.Project(projectName).Suites.OrderBy(s => s.DateStarted);
//
//				foreach (Storage.StorageProject.ProjectSuite suite in suites)
//				{
//					var tests = suite.StableFilesDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly);
//					foreach (DirectoryInfo test in tests)
//					{
//						var copyToDir = new DirectoryInfo(Path.Combine(Storage.Project(projectName).StableFiles.FullName, test.Name));
//						test.CopyTo(copyToDir);
//					}
//				}
//			}
			}
			catch (Exception e)
			{
				return new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent(string.Format("Backup for stable files falied: {0}", e.Message))
				};
			}

			return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("Backup for stable files completed successfully") };
		}

		/// <summary>
		/// Removes suite from project
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="suiteName"></param>
		/// <returns></returns>
		[HttpDelete]
		[Route("{projectName}/{suiteName}/delete")]
		public HttpResponseMessage DeleteSuite(string projectName, string suiteName)
		{
//			using (var ctx = new StorageContext())
//			{
//				var project = ctx.Projects.FirstOrDefault(p => p.Name == projectName);
//			}
			Storage.Project(projectName).Suite(suiteName).Delete();
			return new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent(string.Format("Suite {0} has been removed from {1} project.", suiteName, projectName))
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
					var failed = suite.Tests.Count(t => !t.Passed);
					var passed = suite.Tests.Count(t => t.Passed);

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
				var failedTests = ctx.RunningTests.Where(t => t.SuiteId == suite.Id && !t.Passed).ToList();

				
				foreach (RunningTest failedTest in failedTests)
				{
					var artifactsCollection = new List<Tuple>();
					var testDescription = ctx.Tests.FirstOrDefault(t => t.Id == failedTest.TestId);

					var stableFiles = ctx.StableFiles.Where(sf => sf.TestId == failedTest.TestId && sf.EnvironmentId == failedTest.EnvironmentId).ToList();

					var listOfArtifacts = failedTest.TestResults.Select(df => df.Name)
							.Union(stableFiles.Select(sf => sf.Name));
					var environment = ctx.Environments.FirstOrDefault(e => e.Id == failedTest.EnvironmentId);

					foreach (string singleArtifact in listOfArtifacts)
					{
						var testResult = failedTest.TestResults.Where(t => t.Name == singleArtifact);

						var stableFile = stableFiles.FirstOrDefault(sf => sf.Name == singleArtifact);
						var testingFile = testResult.Select(tf => tf.TestingFile).FirstOrDefault() ?? stableFile.File;
						var diffFile = testResult.Select(tf => tf.DiffFile).FirstOrDefault() ?? "";

						var stableFileDescription = stableFile == null
							? EmptyArtifact()
							: new File
							{
								Name = stableFile.Name,
								Value = stableFile.File,
							};

						artifactsCollection.Add(new Tuple
						{
							DiffFile = GetAtrifactDescription(singleArtifact, diffFile),
							StableFile = stableFileDescription,
							TestingFile = GetAtrifactDescription(singleArtifact, testingFile),
						});
					}
					var browser = ctx.Browsers.FirstOrDefault(b => b.Id == environment.BrowserId);
					var resolution = ctx.Resolutions.FirstOrDefault(r => r.Id == environment.ResolutionId);

					tests.Add(new FailedTest
					{
						TestName = testDescription.Name,
						Artifacts = artifactsCollection,
						Environment = new Environment
						{
							Browser = browser.Name,
							WindowSize = string.Format("{0} x {1}", resolution.Width, resolution.Height),
							Id = (int) environment.Id
						}
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
		private File GetAtrifactDescription(string fileName, string base64Value)
		{
			return new File
			{
				Name = fileName,
				Value = base64Value
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
	}
}
