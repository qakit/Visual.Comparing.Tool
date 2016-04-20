using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace VCT.Client
{

	public class Shell
	{
		private static string _baseServerAddress = "http://localhost:9111/";

		public static string ServerAddress
		{
			get { return _baseServerAddress; }
			set
			{
				if (value.StartsWith("http://")) _baseServerAddress = value;
				else throw new InvalidDataException("Use syntax 'http://ip:port' please ");
			}
		}

		#region singleton

		private static Shell _singleton;

		private Shell()
		{
			SuiteStarted();
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
		}

		private void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
		{
			SuiteCompleted();
		}

		public static Shell Do
		{
			get { return _singleton ?? (_singleton = new Shell()); }
		}

		#endregion

		public void Push(DirectoryInfo dir, string nameOfTest, TestTypes type)
		{
			var restUrl = string.Format("{0}/tests/{1}/{2}", ServerAddress, nameOfTest, type);
			SendFilesToServer(dir, restUrl);
		}

		public bool Pull(DirectoryInfo dir, string nameOfTest, TestTypes type)
		{
			var restUrl = string.Format("{0}/tests/{1}/{2}", ServerAddress, nameOfTest, type);
			return GetFilesFromServer(dir, restUrl).Result;
		}

		#region old methods, not shure that we need it

		/// <summary>
		/// Saves all testing files from specified directory to file server
		/// </summary>
		/// <param name="testingFilesDirectory">Directory in which testing files are placed</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SendTestingFiles(DirectoryInfo testingFilesDirectory, string testName)
		{
			Push(testingFilesDirectory, testName, TestTypes.testing);
		}

		/// <summary>
		/// Download all testing files from server to specified directory
		/// </summary>
		/// <param name="outputTestingFilesDirectory">Directory to put testing files</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		/// <returns></returns>
		public bool GetTestingFiles(DirectoryInfo outputTestingFilesDirectory, string testName)
		{
			return Pull(outputTestingFilesDirectory, testName, TestTypes.testing);
		}

		/// <summary>
		/// Saves all stable files from specified directory to file server
		/// </summary>
		/// <param name="stableFilesDirectory">Directory in which stable files are placed</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SendStableFiles(DirectoryInfo stableFilesDirectory, string testName)
		{
			Push(stableFilesDirectory, testName, TestTypes.stable);
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
			var hasStable = Pull(outputStableFilesDirectory, testName, TestTypes.stable);

			if (!hasStable) Push(outputStableFilesDirectory, testName, TestTypes.diff);
			//we push null content for notify server; crutch but works

			return hasStable;
		}

		/// <summary>
		/// Saves all diff files from specified directory to file server
		/// </summary>
		/// <param name="diffFilesDirectory">Directory in which diff files are placed</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SendDiffFiles(DirectoryInfo diffFilesDirectory, string testName)
		{
			Push(diffFilesDirectory, testName, TestTypes.diff);
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
			return Pull(outputDiffFilesDirectory, testName, TestTypes.diff);
		}

		#endregion

		#region logging methods

		/// <summary>
		/// Inform server that test suite has started
		/// </summary>
		public void SuiteStarted()
		{
			string url = string.Format("{0}/tests/suite/start", ServerAddress);
			PostMessage(url, "Suite started");
		}

		/// <summary>
		/// Inform server that test suite has completed
		/// </summary>
		public void SuiteCompleted()
		{
			string url = string.Format("{0}/tests/suite/stop", ServerAddress);
			PostMessage(url, "Suite completed");
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

		#endregion

		#region core

		/// <summary>
		/// Saves files from specified directory to server. Send POST request to specified url with MultipartFormDataContent
		/// </summary>
		/// <param name="dir">Basic input directory</param>
		/// <param name="url">RestAPI url</param>
		private static void SendFilesToServer(DirectoryInfo dir, string url)
		{
			using (var httpClient = new HttpClient())
			{
				var inputFiles = dir.GetFiles();
				//				if (!inputFiles.Any()) throw new Exception("No files to upload");

				var content = new MultipartFormDataContent();

				foreach (FileInfo file in inputFiles)
				{
					var fs = File.Open(file.FullName, FileMode.Open);
					var fileContent = new StreamContent(fs);
					content.Add(fileContent, "file", file.Name);
				}

				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		/// <summary>
		/// Download files from server and put them to specified directory
		/// </summary>
		/// <param name="dir">Directory to put downloaded files</param>
		/// <param name="url">RestAPI url</param>
		private static async Task<bool> GetFilesFromServer(DirectoryInfo dir, string url)
		{
			using (var httpClient = new HttpClient())
			{
				var result = httpClient.GetAsync(url).Result;
				if (result.StatusCode == HttpStatusCode.NoContent)
				{
					return false;
				}

				if (!dir.Exists) dir.Create();

				var stream = await result.Content.ReadAsMultipartAsync();
				foreach (var content in stream.Contents)
				{
					var fName = content.Headers.GetValues("FileName").First();
					using (var fileStream = File.Open(Path.Combine(dir.FullName, fName), FileMode.Create))
					{
						await content.CopyToAsync(fileStream);
					}
				}
				Console.WriteLine(result);
				return true;
			}
		}

		#endregion


		public enum TestTypes
		{
			stable,
			testing,
			diff,
		}
	}

}
