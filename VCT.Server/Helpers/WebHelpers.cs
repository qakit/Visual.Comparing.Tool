using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace VCT.Server.Helpers
{
	public class WebHelpers
	{
		/// <summary>
		/// Send all files from specified directory to client
		/// </summary>
		/// <param name="inputDirectory">Input Directory from which we need send files back</param>
		/// <param name="fileNameToSend"></param>
		/// <returns></returns>
		public static HttpResponseMessage SendFilesToClient(DirectoryInfo inputDirectory, string fileNameToSend = "*")
		{
			if (inputDirectory == null || !inputDirectory.Exists)
				return new HttpResponseMessage(HttpStatusCode.NoContent);

			var inputFiles = inputDirectory.GetFiles(fileNameToSend);

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

		/// <summary>
		/// Create response message with hash (SHA256) of the specified file to the client
		/// </summary>
		/// <param name="inputDirectory"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static HttpResponseMessage SendHashToClient(DirectoryInfo inputDirectory, string fileName)
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

		private static string GetHexByteArray(byte[] array)
		{
			var builder = new StringBuilder();
			int i;
			for (i = 0; i < array.Length; i++)
			{
				builder.Append(String.Format("{0:X2}", array[i]));
			}
			return builder.ToString();
		}
	}
}
