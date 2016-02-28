using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public string Get()
		{
			return "Returned ALL TESTS";
		}

		[HttpGet]
		[Route("{testId}/stable")]
		public HttpResponseMessage GetStable(string testId)
		{
			var inputfile = @"F:\Projects\VCT.Test\T.txt";
			var inputfile2 = @"F:\Projects\VCT.Test\Bender.jpeg";
			var fileStream = File.Open(inputfile, FileMode.Open);
			var fileStream2 = File.Open(inputfile2, FileMode.Open);

			var content = new MultipartContent();


			var file1Content = new StreamContent(fileStream);
			file1Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
			file1Content.Headers.Add("fileName", "T1.txt");
			content.Add(file1Content);

			var file2Content = new StreamContent(fileStream2);
			file2Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
			file2Content.Headers.Add("fileName", "Bender1.jpeg");
			content.Add(file2Content);


			var response = new HttpResponseMessage { Content = content };

			return response;
		}

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


			//			var multipartFormDataStreamProvider = new UploadMultipartFormProvider(@"F:\Projects\VCT.Test\Output");
			//			await Request.Content.ReadAsMultipartAsync(multipartFormDataStreamProvider);
			//			List<string> files = new List<string>();
			//			foreach (MultipartFileData file in multipartFormDataStreamProvider.FileData)
			//			{
			//				files.Add(file.LocalFileName);
			//			}
			//			return Ok(new { Message = "Uploaded successfully" });
		}

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