using System;
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

		//Get stable images for specified test
		[HttpGet]
		[Route("{testId}/stable")]
		public HttpResponseMessage GetStable(string testId)
		{
			//TODO
			//1. Get Stable images
			//2. If there are not images return some response to identify it (in thi case test must fail)
			var storage = new Storage();
			var stableFiles = storage.StableTestDirectory(testId).GetFiles();
			if (!stableFiles.Any()) return new HttpResponseMessage(HttpStatusCode.NoContent);

			var content = new MultipartContent();
			foreach (FileInfo stableFile in stableFiles)
			{
				//possibly bad way and can cause memory problems i think
				var fs = File.Open(stableFile.FullName, FileMode.Open);

				var fileContent = new StreamContent(fs);
				fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
				fileContent.Headers.Add("FileName", stableFile.Name);
				content.Add(fileContent);

			}

			var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
			return response;
		}

		//Save stable output files to specific folder
		[HttpPost]
		[Route("{testId}/stable")]
		public async Task<IHttpActionResult> PostStable(string testId)
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				return BadRequest("Unsupported media type");
			}

			var storage = new Storage();
			var multiPartFormDataStreamProvider = new UploadMultipartFormProvider(storage.StableTestDirectory(testId).FullName);
			await Request.Content.ReadAsMultipartAsync(multiPartFormDataStreamProvider);
			return Ok(new { Message = "All files have been uploaded successfully" });
		}

		//Save testing output files in case of fails to specific folder
		[HttpPost]
		[Route("{testId}/testing")]
		public async Task<IHttpActionResult> PostTesting(string testId)
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				return BadRequest("Unsupported media type");
			}

			var storage = new Storage();
			var multiPartFormDataStreamProvider = new UploadMultipartFormProvider(storage.TestingTestDirectory(testId).FullName);
			await Request.Content.ReadAsMultipartAsync(multiPartFormDataStreamProvider);
			return Ok(new { Message = "All files have been uploaded successfully" });
		}

		//Save diff output files in case of fails to specific folder
		[HttpPost]
		[Route("{testId}/diff")]
		public async Task<IHttpActionResult> PostDiff(string testId)
		{
			if (!Request.Content.IsMimeMultipartContent("form-data"))
			{
				return BadRequest("Unsupported media type");
			}

			var storage = new Storage();
			var multiPartFormDataStreamProvider = new UploadMultipartFormProvider(storage.DiffTestDirectory(testId).FullName);
			await Request.Content.ReadAsMultipartAsync(multiPartFormDataStreamProvider);
			return Ok(new { Message = "All files have been uploaded successfully" });
		}

		//STABLE
		//		[HttpGet]
		//		[Route("{testId}/stable")]
		//		public HttpResponseMessage GetStable(string testId)
		//		{
		//			var inputfile = @"F:\Projects\VCT.Test\T.txt";
		//			var stream = File.Open(inputfile, FileMode.Open);
		//			var response = new HttpResponseMessage
		//			{
		//				Content = new StreamContent(stream)
		//			};
		//			
		//			response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpg");
		//			response.Content.Headers.ContentLength = stream.Length;
		//			return response;
		//		}

		private string outputFIle = @"F:\Projects\VCT.Test\Output\T2.txt";
		//		[HttpPost]
		//		[Route("{testId}/stable")]
		//		public async void PostStable(string testId)
		//		{
		//			var x = 0;
		//			var httpContent = Request.Content;
		//			var contentStream = await Request.Content.ReadAsStreamAsync();
		//			var bytes = ToByteArray(contentStream);
		////			File.Delete(outputFIle);
		//			using (FileStream fileStream = File.Create(outputFIle))
		//			{
		//				fileStream.Write(bytes, 0, bytes.Length);
		//			}
		//		}



		//		[HttpPost]
		//		[Route("{testId}/stable")]
		//		public async Task<IHttpActionResult> PostStable(string testId)
		//		{
		//			var stream = await Request.Content.ReadAsStreamAsync();
		//			var outputFile = Path.Combine(@"F:\Projects\VCT.Test\Output", Request.Headers.GetValues("fileName").First());
		//
		//			using (FileStream fS = File.Create(outputFile))
		//			{
		//				await stream.CopyToAsync(fS);
		//			}
		//			
		//			return Ok(new { Message = "TADAAAM" });
		//		}

		[HttpGet]
		[Route("{testId}/testing")]
		public string GetTesting(string testId)
		{
			return "testing images";
		}

		[HttpGet]
		[Route("{testId}/diff")]
		public string GetDiff(string testId)
		{
			return "diff images";
		}

		[HttpGet]
		[Route("{testId}/testoutput")]
		public string GetTestOutput(string testId)
		{
			return "Test Output";
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