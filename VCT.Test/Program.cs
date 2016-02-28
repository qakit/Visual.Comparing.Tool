using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace VCT.Test
{
	class Program
	{
		static string baseAddress = "http://localhost:9111/";
		static string url = string.Format("{0}/tests/{1}/stable", baseAddress, "BBBD");
		static FileInfo file = new FileInfo(@"F:\Projects\VCT.Test\T.txt");
		static FileInfo file2 = new FileInfo(@"F:\Projects\VCT.Test\Bender.jpeg");
		static void Main(string[] args)
		{
			//			HttpUploadFile(url, file.FullName, "file", "text/plain");
			//UpData();
			//			UploadData();
//			GetMultiData();
			
			Console.ReadLine();
		}

		//stable
		public async static void GetData()
		{
			using (var httpClient = new HttpClient())
			{
				var result = httpClient.GetAsync(url).Result;
				var stream = await result.Content.ReadAsStreamAsync();
				using (var file = File.Open(@"F:\Projects\VCT.Test\Output\TTTT.txt", FileMode.CreateNew))
				{
					await stream.CopyToAsync(file);
				}
				Console.WriteLine(result);
			}
		}

		public async static void GetMultiData()
		{
			using (var httpClient = new HttpClient())
			{
				var result = httpClient.GetAsync(url).Result;
				var stream = await result.Content.ReadAsMultipartAsync();
				foreach (var content in stream.Contents)
				{
					var fName = content.Headers.GetValues("fileName").First();
					using (var fileStream = File.Open(Path.Combine(@"F:\Projects\VCT.Test\Output", fName), FileMode.Create))
					{
						await content.CopyToAsync(fileStream);
					}
				}
//				using (var file = File.Open(@"F:\Projects\VCT.Test\Output\TTTT.txt", FileMode.CreateNew))
//				{
//					await stream.CopyToAsync(file);
//				}
				Console.WriteLine(result);
			}
		}



		public static void UploadData()
		{
			using (var httpClient = new HttpClient())
			{
				var fileStream = File.Open(file.FullName, FileMode.Open);
				StreamContent content = new StreamContent(fileStream);
				content.Headers.Add("fileName", file.Name);

				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		private struct ImageData
		{
			public Stream ImageStream { get; set; }
			public string ImageName { get; set; }
		}

		/// <summary>
		/// THIS WORKS PERFECTLY!!!
		/// </summary>
		public static void UpData()
		{
			using (var httpClient = new HttpClient())
			{
				var fileStream = File.Open(file.FullName, FileMode.Open);
				var fileStream2 = File.Open(file2.FullName, FileMode.Open);
				var content = new MultipartFormDataContent
				{
					{new StreamContent(fileStream), "\"file\"", string.Format("\"{0}\"", file.Name)},
					{new StreamContent(fileStream2), "\"file\"", string.Format("\"{0}\"", file2.Name)}
				};

				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		public static void HttpUploadFile(string url, string file, string paramName, string contentType)
		{

			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
			wr.ContentType = "multipart/form-data; boundary=" + boundary;
			wr.Method = "POST";
			wr.KeepAlive = true;
			wr.Credentials = CredentialCache.DefaultCredentials;

			Stream rs = wr.GetRequestStream();

			rs.Write(boundarybytes, 0, boundarybytes.Length);

			string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			string header = string.Format(headerTemplate, paramName, file, contentType);
			byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
			rs.Write(headerbytes, 0, headerbytes.Length);

			FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[4096];
			int bytesRead = 0;
			while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				rs.Write(buffer, 0, bytesRead);
			}
			fileStream.Close();

			byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			rs.Write(trailer, 0, trailer.Length);
			rs.Close();

			WebResponse wresp = null;
			try
			{
				wresp = wr.GetResponse();
				Stream stream2 = wresp.GetResponseStream();
				StreamReader reader2 = new StreamReader(stream2);
				Console.WriteLine("File uploaded, server response is: {0}", reader2.ReadToEnd());
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error uploading file", ex);
				if (wresp != null)
				{
					wresp.Close();
					wresp = null;
				}
			}
			finally
			{
				wr = null;
			}
		}
	}

	public class FileUploadResult
	{
		public string LocalFilePath { get; set; }
		public string FileName { get; set; }
		public long FileLength { get; set; }
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
