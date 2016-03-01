using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace VCT.Server
{
	[RoutePrefix("tests")]
	public class TestsController : ApiController
	{
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
			var storage = new Storage();
			return SendFilesToClient(storage.StableTestDirectory(testId));
		}

		[HttpGet]
		[Route("{testId}/testing")]
		public HttpResponseMessage GetTesting(string testId)
		{
			var storage = new Storage();
			return SendFilesToClient(storage.TestingTestDirectory(testId));
		}

		[HttpGet]
		[Route("{testId}/diff")]
		public HttpResponseMessage GetDiff(string testId)
		{
			var storage = new Storage();
			return SendFilesToClient(storage.DiffTestDirectory(testId));
		}

		[HttpPost]
		[Route("{testId}/stable")]
		public Task<IHttpActionResult> PostStable(string testId)
		{
			var storage = new Storage();
			return GetFilesFromClient(storage.StableTestDirectory(testId));
		}

		[HttpPost]
		[Route("{testId}/testing")]
		public Task<IHttpActionResult> PostTesting(string testId)
		{
			var storage = new Storage();
			return GetFilesFromClient(storage.TestingTestDirectory(testId));
		}

		[HttpPost]
		[Route("{testId}/diff")]
		public Task<IHttpActionResult> PostDiff(string testId)
		{
			var storage = new Storage();
			return GetFilesFromClient(storage.DiffTestDirectory(testId));
		}

		[HttpPost]
		[Route("suite/start")]
		public void SuiteStart()
		{
			//TODO move all previous content to history folder with datetime name
			//write started time to txt file
			var storage = new Storage();
			Console.WriteLine("Backup previous data");
			storage.BackUpPreviousRunForHistory();
			Console.WriteLine("Write new info");
			try
			{
				storage.WriteHistoryInfo(string.Format("started|{0}", DateTime.Now.ToString("dd.mm.yyyy_hh.mm.ss")), true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			Console.WriteLine("Suite started at {0}", DateTime.Now.ToString("dd.mm.yyyy_hh.mm.ss"));
		}

		[HttpPost]
		[Route("suite/stop")]
		public void SuiteStop()
		{
			//TODO write end time to txt file
			var storage = new Storage();
			storage.WriteHistoryInfo(string.Format("completed|{0}", DateTime.Now.ToString("dd.mm.yyyy_hh.mm.ss")));
			Console.WriteLine("Suite stopped at {0}", DateTime.Now.ToString("dd.mm.yyyy_hh.mm.ss"));
		}

		[HttpGet]
		[Route("fails")]
		public List<TestResultFiles> GetFails()
		{
			var storage = new Storage();
			var testResults = new List<TestResultFiles>();
			foreach (DirectoryInfo diffDirectory in storage.DiffFilesDirectory.GetDirectories())
			{
				var stableFolderPath = (from stable in storage.StableFilesDirectory.GetDirectories()
										where string.Equals(diffDirectory.Name, stable.Name, StringComparison.InvariantCultureIgnoreCase)
										where stable != null
										select stable.FullName).FirstOrDefault();
				var testingFolderPath = (from testing in storage.TestingFilesDirectory.GetDirectories()
										 where string.Equals(diffDirectory.Name, testing.Name, StringComparison.InvariantCultureIgnoreCase)
										 where testing != null
										 select testing.FullName).FirstOrDefault();

				testResults.Add(new TestResultFiles
				{
					DiffFilesDirectory = diffDirectory.FullName,
					StableFilesDirectory = stableFolderPath,
					TestingFilesDirectory = testingFolderPath
				});
			}
			return testResults;
			//			return (from directory in storage.DiffFilesDirectory.GetDirectories() select directory.FullName).ToArray();
		}

		[HttpGet]
		[Route("passes")]
		public string[] GetPassedTests()
		{
			var storage = new Storage();
			//Get failed tests
			var failedTests = storage.DiffFilesDirectory.GetDirectories();
			//Get total amout of tests
			var testingFiles = storage.TestingFilesDirectory.GetDirectories();
			//find intersect files and return
			//var passedTests = testingFiles.Except(failedTests);
			//var passedTests = testingFiles.Select(t => t.Name).Except(failedTests.Select(f => f.Name));
			var passedTests = testingFiles.Except(failedTests, new DirectoryComparer());
			return (from passed in passedTests select passed.FullName).ToArray();
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