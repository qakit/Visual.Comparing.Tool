using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;
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

		[HttpGet]
		[Route("")]
		public string GetAllTests()
		{
			return "Returned ALL Previously runned tests (list of folders in Testing directory i think (or DIFF)";
		}

		[HttpGet]
		[Route("{testId}/stable")]
		public HttpResponseMessage GetStable(string testId)
		{
			return SendFilesToClient(Storage.Current.StableTestDirectory(testId));
		}

		[HttpGet]
		[Route("{testId}/testing")]
		public HttpResponseMessage GetTesting(string testId)
		{
			return SendFilesToClient(Storage.Current.TestingTestDirectory(testId));
		}

		[HttpGet]
		[Route("{testId}/diff")]
		public HttpResponseMessage GetDiff(string testId)
		{
			return SendFilesToClient(Storage.Current.DiffTestDirectory(testId));
		}

		[HttpPost]
		[Route("{testId}/stable")]
		public Task<IHttpActionResult> PostStable(string testId)
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				//if request doesn't contain any data just copy testing files to stable files (accept fail)
				Storage.Current.DiffTestDirectory(testId).ClearContent();
				return UpdateFilesForTest(Storage.Current.TestingTestDirectory(testId), Storage.Current.StableTestDirectory(testId));
			}
			return GetFilesFromClient(Storage.Current.StableTestDirectory(testId));
		}

		[HttpPost]
		[Route("{testId}/testing")]
		public Task<IHttpActionResult> PostTesting(string testId)
		{
			return GetFilesFromClient(Storage.Current.TestingTestDirectory(testId));
		}

		[HttpPost]
		[Route("{testId}/diff")]
		public Task<IHttpActionResult> PostDiff(string testId)
		{
			return GetFilesFromClient(Storage.Current.DiffTestDirectory(testId));
		}


		[HttpPost]
		[Route("suite/start")]
		public void SuiteStart()
		{
			string inceptionTime = DateTime.Now.ToString(DateFormat);

			Storage.Allocate(inceptionTime);

			try { Storage.WriteHistoryInfo(string.Format("started|{0}", inceptionTime), true); }
			catch (Exception e) { Console.WriteLine(e.Message); }

			Console.WriteLine("Suite started at {0}", inceptionTime);
		}

		[HttpPost]
		[Route("suite/stop")]
		public void SuiteStop()
		{
			string finishTime = DateTime.Now.ToString(DateFormat);
			Storage.WriteHistoryInfo(string.Format("completed|{0}", finishTime));
			Console.WriteLine("Suite stopped at {0}", finishTime);
		}

		[HttpGet]
		[Route("history")]
		public JsonResult<List<HistoryResult>> GetHistory()
		{
			List<HistoryResult> historyResults = new List<HistoryResult>();

			int id = 0;

			foreach (DirectoryInfo session in 
				Storage.Root.GetDirectories().OrderBy(p => p.CreationTime).Reverse())
			{
				var hub = new Storage.Hub(session);
				
				var resultsWhatewerItMeans = GetTestResults(hub.stable, hub.testing, hub.diff);
				var passed = GetPassedTests(hub.stable, hub.testing, hub.diff).Length;
				var failed = resultsWhatewerItMeans.Count;

				historyResults.Add(new HistoryResult
				{
					DateStarted = hub.root.CreationTime.ToShortTimeString(),
					DateCompleted = "[ TODO ]",
					Failed = failed,
					Passed = passed,
					Id = id,
					Tests = resultsWhatewerItMeans
				});

				id++;
			}

			return Json(historyResults);
		}

		[HttpGet]
		[Route("fails")]
		public JsonResult<List<TestResult>> GetFails()
		{
			//What da Hell! test is fails? fails is tests? naming sucks
			var testsResults = GetTestResults(Storage.Current.stable, Storage.Current.testing, Storage.Current.diff);

			return Json(testsResults);
		}

		private List<TestResult> GetTestResults(DirectoryInfo stableFilesDirectory, DirectoryInfo testingFilesDirectory, DirectoryInfo diffFilesDirectory)
		{
			var testsResults = new List<TestResult>();

			//Here we consider that diff directory will be created on any fail! 
			//No fail, no diff directory!
			foreach (DirectoryInfo diffDirectory in diffFilesDirectory.GetDirectories())
			{
				//Get stable directory path
				var stableFolderPath = (from stable in stableFilesDirectory.GetDirectories()
										where string.Equals(diffDirectory.Name, stable.Name, StringComparison.InvariantCultureIgnoreCase)
										where stable != null
										select stable).FirstOrDefault();
				//Get testing directory path
				var testingFolderPath = (from testing in testingFilesDirectory.GetDirectories()
										 where string.Equals(diffDirectory.Name, testing.Name, StringComparison.InvariantCultureIgnoreCase)
										 where testing != null
										 select testing).FirstOrDefault();
				//Get all images from all directories
				var stableImages = GetResultImages(stableFolderPath);
				var testingImages = GetResultImages(testingFolderPath);
				var diffImages = GetResultImages(diffDirectory);

				//find longest list as all directories might have different number of images so we need to find max one
				//to make sure that we no iterate throught smallest one
				//how to avoid this? merge three lists into one excluding all repeated images?

				var longestList = FindLongest(stableImages, testingImages, diffImages);

				var artifactsCollection = new List<TestArtifacts>();

				foreach (FileInfo imageFile in longestList)
				{
					var stableFile = GetFileByName(stableImages, imageFile.Name);
					var testingFile = GetFileByName(testingImages, imageFile.Name);
					var diffFile = GetFileByName(diffImages, imageFile.Name);

					var stableData = GetAtrifactDescription(stableFile);
					var testingData = GetAtrifactDescription(testingFile);
					var diffData = GetAtrifactDescription(diffFile);

					artifactsCollection.Add(new TestArtifacts
					{
						StableFile = stableData,
						TestingFile = testingData,
						DiffFile = diffData
					});
				}

				testsResults.Add(new TestResult
				{
					TestName = diffDirectory.Name,
					Artifacts = artifactsCollection
				});
			}

			if (testsResults.Count == 0)
			{
				testsResults.Add(new TestResult
				   {
					   TestName = "",
					   Artifacts = new List<TestArtifacts>
					{
						new TestArtifacts
						{
							DiffFile = EmptyArtifact(),
							StableFile = EmptyArtifact(),
							TestingFile = EmptyArtifact()
						}
					}
				   });
			}

			return testsResults;
		}

		[HttpGet]
		[Route("passes")]
		public string[] GetPassedTests()
		{
			var passedTests = GetPassedTests(Storage.Current.stable, Storage.Current.testing, Storage.Current.diff);
			return passedTests;
		}

		private string[] GetPassedTests(DirectoryInfo stableFilesDirectory, DirectoryInfo testingFilesDirectory,
			DirectoryInfo failedTestsDirectory)
		{
			var stableDirectories = stableFilesDirectory.GetDirectories();
			var testingDirectories = testingFilesDirectory.GetDirectories();
			var failedDirectories = failedTestsDirectory.GetDirectories();

			var distinctList =
				stableDirectories.Select(d => d.Name)
					.ToList()
					.Concat(testingDirectories.Select(d => d.Name).ToList()).Distinct();
			var passes = (from dir in distinctList
						  where !failedDirectories.Select(d => d.Name).ToList().Contains(dir)
						  select dir).ToArray();

			return passes;
		}

		private List<FileInfo> GetResultImages(DirectoryInfo directoryToSearch)
		{
			if (directoryToSearch == null) return new List<FileInfo>();
			var imageFiles = directoryToSearch.GetFiles(new[] { "*.png", "*.bmp", "*.jpeg", "*.jpg", "*.gif" },
				SearchOption.TopDirectoryOnly);
			return imageFiles.ToList();
		}


		private TestArtifact EmptyArtifact()
		{
			return new TestArtifact
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
		/// <returns><see cref="TestArtifact"/> with Name (empty if null), Path (empty if null)</returns>
		private TestArtifact GetAtrifactDescription(FileInfo artifactFile)
		{
			if (artifactFile == null)
				return EmptyArtifact();

			return new TestArtifact
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
		public HttpResponseMessage SendFilesToClient(DirectoryInfo inputDirectory)
		{
			if (inputDirectory == null || !inputDirectory.Exists) return new HttpResponseMessage(HttpStatusCode.NoContent);

			var inputFiles = inputDirectory.GetFiles();

			if (!inputFiles.Any()) return new HttpResponseMessage(HttpStatusCode.NoContent);

			var content = new MultipartContent();
			foreach (FileInfo inputFile in inputFiles)
			{
				//possibly bad way and can cause memory problems i think
				var fs = File.Open(inputFile.FullName, FileMode.Open);

				var fileContent = new StreamContent(fs);
				//GET MIME TYPE HERE SOMEHOW
				fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
				fileContent.Headers.Add("FileName", inputFile.Name);
				content.Add(fileContent);
			}

			var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
			return response;
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