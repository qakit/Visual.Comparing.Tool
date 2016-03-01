using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VCT.Client
{
	public class Core
	{
		private const string BaseServerAddress = "http://localhost:9111/";

		/// <summary>
		/// Saves all testing files from specified directory to file server
		/// </summary>
		/// <param name="testingFilesDirectory">Directory in which testing files are placed</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SaveTestingFiles(DirectoryInfo testingFilesDirectory, string testName)
		{
			string url = string.Format("{0}/tests/{1}/testing", BaseServerAddress, testName);

			SaveFilesToServer(url, testingFilesDirectory);
		}

		/// <summary>
		/// Download all testing files from server to specified directory
		/// </summary>
		/// <param name="outputTestingFilesDirectory">Directory to put testing files</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public bool GetTestingFiles(DirectoryInfo outputTestingFilesDirectory, string testName)
		{
			//TODO download and put all testing files for specified test to folder;
			string url = string.Format("{0}/tests/{1}/testing", BaseServerAddress, testName);
			
			return GetFilesFromServer(url, outputTestingFilesDirectory).Result;
		}

		/// <summary>
		/// Saves all stable files from specified directory to file server
		/// </summary>
		/// <param name="stableFilesDirectory">Directory in which stable files are placed</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SaveStableFiles(DirectoryInfo stableFilesDirectory, string testName)
		{
			string url = string.Format("{0}/tests/{1}/stable", BaseServerAddress, testName);

			SaveFilesToServer(url, stableFilesDirectory);
			//TODO save all stable files from stable directory to server (with removing old stable files);
		}

		/// <summary>
		/// Download all stable files from server to specified directory
		/// </summary>
		/// <param name="outputStableFilesDirectory">Directory to put stable files</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public bool GetStableFiles(DirectoryInfo outputStableFilesDirectory, string testName)
		{
			//TODO download and put all stable files for specified test to folder;
			string url = string.Format("{0}/tests/{1}/stable", BaseServerAddress, testName);

			return GetFilesFromServer(url, outputStableFilesDirectory).Result;
		}

		/// <summary>
		/// Saves all diff files from specified directory to file server
		/// </summary>
		/// <param name="diffFilesDirectory">Directory in which diff files are placed</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SaveDiffFiles(DirectoryInfo diffFilesDirectory, string testName)
		{
			string url = string.Format("{0}/tests/{1}/diff", BaseServerAddress, testName);

			SaveFilesToServer(url, diffFilesDirectory);
			//TODO save all diff files from diff files folder to server (with removing old diff files first)
		}

		/// <summary>
		/// Download all diff files from server to specified directory
		/// </summary>
		/// <param name="outputDiffFilesDirectory">Directory to put diff files</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public bool GetDiffFiles(DirectoryInfo outputDiffFilesDirectory, string testName)
		{
			//TODO download and put all diff files for specified test to folder;
			string url = string.Format("{0}/tests/{1}/diff", BaseServerAddress, testName);

			return GetFilesFromServer(url, outputDiffFilesDirectory).Result;
		}

		/// <summary>
		/// Inform server that test suite has started
		/// </summary>
		public void SuiteStarted()
		{
			string url = string.Format("{0}/tests/suite/start", BaseServerAddress);
			PostMessage(url, "Suite started");
		}

		/// <summary>
		/// Inform server that test suite has completed
		/// </summary>
		public void SuiteCompleted()
		{
			string url = string.Format("{0}/tests/suite/stop", BaseServerAddress);
			PostMessage(url, "Suite completed");
		}

		/// <summary>
		/// Saves files from specified directory to server. Send POST request to specified url with MultipartFormDataContent
		/// </summary>
		/// <param name="url">RestAPI url</param>
		/// <param name="inputDirectory">Basic input directory</param>
		private void SaveFilesToServer(string url, DirectoryInfo inputDirectory)
		{
			using (var httpClient = new HttpClient())
			{
				var inputFiles = inputDirectory.GetFiles();
//				if (!inputFiles.Any()) throw new Exception("No files to upload");

				var content = new MultipartFormDataContent();

				foreach (FileInfo stableFile in inputFiles)
				{
					var fs = File.Open(stableFile.FullName, FileMode.Open);
					var fileContent = new StreamContent(fs);
					content.Add(fileContent, "file", stableFile.Name);
				}

				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		/// <summary>
		/// Download files from server and put them to specified directory
		/// </summary>
		/// <param name="url">RestAPI url</param>
		/// <param name="outputDirectory">Directory to put downloaded files</param>
		private async Task<bool> GetFilesFromServer(string url, DirectoryInfo outputDirectory)
		{
			using (var httpClient = new HttpClient())
			{
				var result = httpClient.GetAsync(url).Result;
				if (result.StatusCode == HttpStatusCode.NoContent)
				{
					return false;
				}

				if (!outputDirectory.Exists) outputDirectory.Create();

				var stream = await result.Content.ReadAsMultipartAsync();
				foreach (var content in stream.Contents)
				{
					var fName = content.Headers.GetValues("FileName").First();
					using (var fileStream = File.Open(Path.Combine(outputDirectory.FullName, fName), FileMode.Create))
					{
						await content.CopyToAsync(fileStream);
					}
				}
				Console.WriteLine(result);
				return true;
			}
		}

		/// <summary>
		/// Post message to specified url
		/// </summary>
		/// <param name="url">Url to send message</param>
		/// <param name="message">Message</param>
		private void PostMessage(string url, string message)
		{
			Console.WriteLine("Posting message {0}", message);
			using (var httpClient = new HttpClient())
			{
				var content = new StringContent(message);
				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}
	}
}
