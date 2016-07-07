using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	[RoutePrefix("tests")]
	public class TestsController : ApiController
	{
		public static string LocalStorage = Config.AppSettings["storage"];
		static readonly string DateFormat = Config.AppSettings["dateFormat"];

		//private static DirectoryInfo CurrentSessionDir;
		private Storage Storage = new Storage();

//		[HttpGet]
//		[Route("{projId}/{testId}/stable")]
//		[Obsolete]
//		public HttpResponseMessage GetStable(string projId, string testId)
//		{
//			var projectDir = CurrentFor(projId).projectDirectory;
//			return SendFilesToClient(Storage.GetLatestExistingStable(testId, projectDir));
//		}
//
//		[HttpGet]
//		[Route("{projId}/{testId}/{fileName}/stable")]
//		public HttpResponseMessage GetStableImage(string projId, string testId, string fileName)
//		{
//			var projectDir = CurrentFor(projId).projectDirectory;
//			return SendFilesToClient(Storage.GetLatestExistingStable(testId, projectDir), fileName);
//		}
//
//		[HttpGet]
//		[Route("{projId}/{testId}/{fileName}/stable/hash")]
//		public HttpResponseMessage GetStableImageHash(string projId, string testId, string fileName)
//		{
//			var projectDir = CurrentFor(projId).projectDirectory;
//			return SendHashToClient(Storage.GetLatestExistingStable(testId, projectDir), fileName);
//		}
//
//		[HttpGet]
//		[Route("{projId}/{testId}/testing")]
//		public HttpResponseMessage GetTesting(string projId, string testId)
//		{
//			return SendFilesToClient(CurrentFor(projId).TestingTestDirectory(testId));
//		}
//
//		[HttpGet]
//		[Route("{projId}/{testId}/diff")]
//		public HttpResponseMessage GetDiff(string projId, string testId)
//		{
//			return SendFilesToClient(CurrentFor(projId).DiffTestDirectory(testId));
//		}

//		[HttpPost]
//		[Route("{projId}/{testId}/stable")]
//		public Task<IHttpActionResult> PostStable(string projId, string testId)
//		{
//			if (!Request.Content.IsMimeMultipartContent("form-data"))
//			{
//				//if request doesn't contain any data just copy testing files to stable files (accept fail)
//				CurrentFor(projId).DiffTestDirectory(testId).ClearContent();
//				return UpdateFilesForTest(CurrentFor(projId).TestingTestDirectory(testId), CurrentFor(projId).StableTestDirectory(testId));
//			}
//			return GetFilesFromClient(CurrentFor(projId).StableTestDirectory(testId));
//		}

//		[HttpPost]
//		[Route("{projId}/{testId}/testing")]
//		public Task<IHttpActionResult> PostTesting(string projId, string testId)
//		{
//			return GetFilesFromClient(CurrentFor(projId).TestingTestDirectory(testId));
//		}
//
//		[HttpPost]
//		[Route("{projId}/{testId}/diff")]
//		public Task<IHttpActionResult> PostDiff(string projId, string testId)
//		{
//
//			return GetFilesFromClient(CurrentFor(projId).DiffTestDirectory(testId));
//		}

//		private Storage.Hub CurrentFor(string projId)
//		{
//			if (Storage.Current != null && Storage.Current.projectDirectory.Name.Equals(projId)) return Storage.Current;//ideal case
//			else //may happend if we have two client session in the same time
//			{
//				var freshestInAProj = Storage.StorageDirectory
//					.GetDirectories(projId, SearchOption.TopDirectoryOnly).First()
//					.GetDirectories("*", SearchOption.TopDirectoryOnly)
//					.OrderBy(d => d.CreationTime)
//					.Last();
//				Storage.Current = new Storage.Hub(freshestInAProj);
//				return Storage.Current;
//			}
//		}


		[HttpPost]
		[Route("{projId}/suite/start")]
		public void SuiteStart(string projId)
		{
			string inceptionTime = DateTime.Now.ToString(DateFormat);

			Storage.Allocate(projId, inceptionTime);

			try { Storage.WriteLog(projId, string.Format("Started: {0}\n", inceptionTime), true); }
			catch (Exception e) { Console.WriteLine(e.Message); }

			Console.WriteLine("[{0}] Suite started at {1}", projId, inceptionTime);
		}

		[HttpPost]
		[Route("{projId}/suite/stop")]
		public void SuiteStop(string projId)
		{
			string finishTime = DateTime.Now.ToString(DateFormat);
			Storage.WriteLog(projId, string.Format("Completed: {0}\n", finishTime));
			Console.WriteLine("[{0}] Suite stopped at {0}", projId, finishTime);
		}

//		[HttpGet]
//		[Route("history")]
//		public JsonResult<List<Project>> GetHistory()
//		{
//			var allProjects = new List<Project>();
//
//			int projectId = 1;
//
//			foreach (DirectoryInfo projectDirectory in
//				Storage.StorageDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly))
//			{
//				var suiteResults = new List<Suite>();
//				
//				var suiteDirectories =
//					projectDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly).OrderBy(p => p.CreationTime).Reverse();
//				
//				//start from last one to show newest on the top with correct run id
//				int suiteId = suiteDirectories.Count();
//
//				foreach (var suiteDirectory in suiteDirectories)
//				{
//					//any suite directory which contain diff/test/stable directories in it
//					var currentSuiteDirectory = new Storage.Hub(suiteDirectory);
//					var latestTestingFile =
//						currentSuiteDirectory.testingDirectory.GetDirectories("*", SearchOption.AllDirectories)
//							.OrderBy(p => p.CreationTime)
//							.LastOrDefault();
//
//					var failed = GetFailedResults(currentSuiteDirectory);
//					var passed = GetPassedTests(currentSuiteDirectory).Length;
//					suiteResults.Add(new Suite
//					{
//						DateStarted = currentSuiteDirectory.suiteDirectory.CreationTime.ToShortTimeString(),
//						DateCompleted = latestTestingFile == null ? "No Time" : latestTestingFile.CreationTime.ToShortTimeString(),
//						Failed = failed.Count,
//						Passed = passed,
//						Id = suiteId,
//						Tests = failed
//					});
//					suiteId--;
//				}
//
//				allProjects.Add(new Project
//				{
//					Suites = suiteResults,
//					Id = projectId,
//					Name = projectDirectory.Name
//				});
//
//				projectId++;
//			}
//
//			return Json(allProjects);
//		}
//
//		[HttpGet]
//		[Route("{projId}/fails")]
//		public JsonResult<List<FailedTest>> GetFails(string projId)
//		{
//			return Json(GetFailedResults(CurrentFor(projId)));
//		}
//
//		public List<FailedTest> GetFailedResults(Storage.Hub node)
//		{
//			var testsResults = new List<FailedTest>();
//
//			//Here we consider that diff directory will be created on any fail! 
//			//No fail, no diff directory!
//			foreach (DirectoryInfo diffDirectory in node.diffDirectory.GetDirectories())
//			{
//				//Get stable directory path
//				var stableFolderPath = Storage.GetLatestExistingStable(diffDirectory.Name, node.suiteDirectory);
//
//				//Get testing directory path
//				var testingFolderPath = (from testing in node.testingDirectory.GetDirectories()
//										 where string.Equals(diffDirectory.Name, testing.Name, StringComparison.InvariantCultureIgnoreCase)
//										 where testing != null
//										 select testing).FirstOrDefault();
//				//Get all images from all directories
//				var stableImages = GetResultImages(stableFolderPath);
//				var testingImages = GetResultImages(testingFolderPath);
//				var diffImages = GetResultImages(diffDirectory);
//
//				//find longest list as all directories might have different number of images so we need to find max one
//				//to make sure that we no iterate throught smallest one
//				//how to avoid this? merge three lists into one excluding all repeated images?
//
//				IEnumerable<FileInfo> longestList = FindLongest(stableImages, testingImages, diffImages);
//
//				List<Tuple> artifactsCollection = new List<Tuple>();
//
//				foreach (FileInfo imageFile in longestList)
//				{
//					var stableFile = GetFileByName(stableImages, imageFile.Name);
//					var testingFile = GetFileByName(testingImages, imageFile.Name);
//					var diffFile = GetFileByName(diffImages, imageFile.Name);
//
//					var stableData = GetAtrifactDescription(stableFile);
//					var testingData = GetAtrifactDescription(testingFile);
//					var diffData = GetAtrifactDescription(diffFile);
//
//					artifactsCollection.Add(new Tuple
//					{
//						StableFile = stableData,
//						TestingFile = testingData,
//						DiffFile = diffData
//					});
//				}
//
//				testsResults.Add(new FailedTest
//				{
//					TestName = diffDirectory.Name,
//					Artifacts = artifactsCollection
//				});
//			}
//
//			#region looks bad, probably need to change
////
////			if (testsResults.Count == 0)
////			{
////				testsResults.Add(new Test
////					{
////						TestName = "",
////						Artifacts = new List<Tuple>
////							{
////								new Tuple
////									{
////										DiffFile = EmptyArtifact(),
////										StableFile = EmptyArtifact(),
////										TestingFile = EmptyArtifact()
////									}
////							}
////					});
////			}
//
//			#endregion
//
//
//			return testsResults;
//		}
//
//		[HttpGet]
//		[Route("{projId}/passes")]
//		public string[] GetPassedTests(string projId)
//		{
//			return GetPassedTests(CurrentFor(projId));
//		}

//		private string[] GetPassedTests(Storage.Hub node)
//		{
//			var stableDirectories = node.stableDirectory.GetDirectories();
//			var testingDirectories = node.testingDirectory.GetDirectories();
//			var failedDirectories = node.diffDirectory.GetDirectories();
//
//			var distinctList =
//				stableDirectories.Select(d => d.Name)
//					.ToList()
//					.Concat(testingDirectories.Select(d => d.Name).ToList()).Distinct();
//			var passes = (from dir in distinctList
//						  where !failedDirectories.Select(d => d.Name).ToList().Contains(dir)
//						  select dir).ToArray();
//
//			return passes;
//		}

		private List<FileInfo> GetResultImages(DirectoryInfo directoryToSearch)
		{
			if (directoryToSearch == null) return new List<FileInfo>();
			var imageFiles = directoryToSearch.GetFiles(new[] { "*.png", "*.bmp", "*.jpeg", "*.jpg", "*.gif" },
				SearchOption.TopDirectoryOnly);
			return imageFiles.ToList();
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
				RelativePath = MakeRelativePath(LocalStorage, artifactFile.FullName)
			};
		}

		/// <summary>
		/// Search for element in the list by it's possible name
		/// </summary>
		/// <param name="listToSearchIn">List of files to search in</param>
		/// <param name="fileNameToSearch">Expected file name</param>
		/// <returns><see cref="FileInfo"/> or null if no file exist in the list</returns>
		private FileInfo GetFileByName(List<FileInfo> listToSearchIn, string fileNameToSearch)
		{
			return (from item in listToSearchIn
					where string.Equals(item.Name, fileNameToSearch, StringComparison.InvariantCultureIgnoreCase)
					select item).FirstOrDefault();
		}

		/// <summary>
		/// Gets the longest list from the specified
		/// </summary>
		/// <param name="lists">Lists</param>
		/// <returns>Longest list</returns>
		private IEnumerable<FileInfo> FindLongest(params List<FileInfo>[] lists)
		{
			var longest = lists[0];
			for (var i = 1; i < lists.Length; i++)
			{
				if (lists[i].Count > longest.Count)
					longest = lists[i];
			}
			return longest;
		}
		
		private string GetHexByteArray(byte[] array)
		{
			var builder = new StringBuilder();
			int i;
			for (i = 0; i < array.Length; i++)
			{
				builder.Append(String.Format("{0:X2}", array[i]));
			}
			return builder.ToString();
		}

		public class DirectoryComparer : IEqualityComparer<DirectoryInfo>
		{
			bool IEqualityComparer<DirectoryInfo>.Equals(DirectoryInfo x, DirectoryInfo y)
			{
				return (x.Name.Equals(y.Name));
			}

			int IEqualityComparer<DirectoryInfo>.GetHashCode(DirectoryInfo obj)
			{
				if (Object.ReferenceEquals(obj, null))
					return 0;

				return obj.Name.GetHashCode();
			}
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

		/// <summary>
		/// Send all files from specified directory to client
		/// </summary>
		/// <param name="inputDirectory">Input Directory from which we need send files back</param>
		/// <returns></returns>
		public HttpResponseMessage SendFilesToClient(DirectoryInfo inputDirectory, string fileNameToSend = "")
		{
			if (inputDirectory == null || !inputDirectory.Exists)
				return new HttpResponseMessage(HttpStatusCode.NoContent);

			string searchPattern = string.IsNullOrEmpty(fileNameToSend) ? "*" : fileNameToSend;

			var inputFiles = inputDirectory.GetFiles(searchPattern);

			if (!inputFiles.Any()) return new HttpResponseMessage(HttpStatusCode.NoContent);

			var content = new MultipartContent();
			foreach (FileInfo inputFile in inputFiles)
			{
				//possibly bad way and can cause memory problems i think
				var fs = System.IO.File.Open(inputFile.FullName, FileMode.Open);

				var fileContent = new StreamContent(fs);
				//GET MIME TYPE HERE SOMEHOW
				fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
				fileContent.Headers.Add("FileName", inputFile.Name);
				content.Add(fileContent);
			}

			var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
			return response;
		}

		public HttpResponseMessage SendHashToClient(DirectoryInfo inputDirectory, string fileName)
		{
			if (inputDirectory == null || !inputDirectory.Exists)
				return new HttpResponseMessage(HttpStatusCode.NoContent);

			var inputFiles = inputDirectory.GetFiles(fileName);
			if (!inputFiles.Any()) return new HttpResponseMessage(HttpStatusCode.NoContent);

			SHA256 hash = SHA256Managed.Create();

			var fileStream = inputFiles[0].Open(FileMode.Open);
			var hashValue = hash.ComputeHash(fileStream);
			var result = GetHexByteArray(hashValue);

			fileStream.Close();

			var content = new StringContent(result);
			return new HttpResponseMessage { Content = content };
		}

		public async Task<IHttpActionResult> UpdateFilesForTest(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
		{
			sourceDirectory.CopyTo(targetDirectory);
			return Ok(new { Message = string.Format("All files have been moved from {0} directory to {1}", sourceDirectory.FullName, targetDirectory.FullName) });
		}
	}


	public class UploadMultipartFormProvider : MultipartFormDataStreamProvider
	{
		public UploadMultipartFormProvider(string rootPath) : base(rootPath) { }

		public override string GetLocalFileName(HttpContentHeaders headers)
		{
			if (headers != null &&
				headers.ContentDisposition != null)
			{
				return headers
					.ContentDisposition
					.FileName.TrimEnd('"').TrimStart('"');
			}

			return base.GetLocalFileName(headers);
		}
	}
}