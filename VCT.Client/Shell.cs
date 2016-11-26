using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VCT.Sdk;

namespace VCT.Client
{
	/// <summary>
	/// Do not forget to set ServerAddress and ProjectId BEFORE you start using Shell.Do._any_action_
	/// </summary>
	public class Shell
	{
		private static string _baseServerAddress = "http://localhost:9111/";
		private static string _projectId = "default";
		private static string _suiteId = "";

		public static string ServerAddress
		{
			get { return _baseServerAddress; }
			set
			{
				if (value.StartsWith("http://")) _baseServerAddress = value;
				else throw new InvalidDataException("Use syntax 'http://ip:port' please ");
			}
		}

		public static string ProjectId
		{
			get { return _projectId; }
			set
			{
				if (value.Contains("/") || value.Contains("\\"))
					throw new InvalidDataException("[!] Don't use slash [!]");
				_projectId = value;
			}
		}

		public static string SuiteId
		{
			get { return _suiteId; }
			set { _suiteId = value; }
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
		
		/// <summary>
		/// Inform server that test suite has started
		/// </summary>
		public void SuiteStarted()
		{
			string url = string.Format("{0}/api/{1}/suite/start", ServerAddress, ProjectId);
			SuiteId = PostMessage(url, "Suite started").Replace("\"", "");
		}

		/// <summary>
		/// Inform server that test suite has completed
		/// </summary>
		public void SuiteCompleted()
		{
			string url = string.Format("{0}/api/{1}/{2}/suite/stop", ServerAddress, ProjectId, SuiteId);
			PostMessage(url, "Suite completed");
			SuiteId = "";
		}

		private void Push(DirectoryInfo dir, string nameOfTest, TestTypes type)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/{4}", ServerAddress, ProjectId, SuiteId, nameOfTest, type);
			SendFilesToServer(dir, restUrl);
		}

		private void Push(FileInfo fileToSend, string nameOfTest, TestTypes type)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/{4}", ServerAddress, ProjectId, SuiteId, nameOfTest, type);
			SendFileToServer(fileToSend, restUrl);
		}

		private bool Pull(DirectoryInfo dir, string nameOfTest, TestTypes type)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/{4}", ServerAddress, ProjectId, SuiteId, nameOfTest, type);
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
		/// Saves testing file to file server
		/// </summary>
		/// <param name="testingFile">File which you want to save to the server</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SendTestingFile(FileInfo testingFile, string testName, TestInfo testInfo)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/{4}", ServerAddress, ProjectId, SuiteId, testName, TestTypes.testing);

			using (var httpClient = new HttpClient())
			{
				HttpResponseMessage result;
				var content = new MultipartContent("form-data", Guid.NewGuid().ToString());

								testingFile.Refresh();
				if (!testingFile.Exists)
				{
					result = httpClient.PostAsync(restUrl, content).Result;
					Console.WriteLine(result);
					return;
				}

				content.Headers.Add("TestInfo", JsonConvert.SerializeObject(testInfo));
				var fs = File.Open(testingFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				
				StreamContent filePart = new StreamContent(fs);
				
				filePart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
				filePart.Headers.ContentType = new MediaTypeHeaderValue("image/png");
				filePart.Headers.ContentDisposition.FileName = testingFile.Name;
				
//				var jsongPart = new StringContent(JsonConvert.SerializeObject(testInfo), Encoding.UTF8, "application/json");
//				jsongPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
//				jsongPart.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				content.Add(filePart);
//				content.Add(jsongPart);
				

				result = httpClient.PostAsync(restUrl, content).Result;
				Console.WriteLine(result);
//				var content = new MultipartFormDataContent();
//				HttpResponseMessage result;
//				testingFile.Refresh();
//				if (!testingFile.Exists)
//				{
//					result = httpClient.PostAsync(restUrl, content).Result;
//					Console.WriteLine(result);
//					return;
//				}
//
//				var fs = File.Open(testingFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
//				var fileContent = new StreamContent(fs);
//				content.Add(fileContent, "file", testingFile.Name);
//				content.Add(new StringContent(JsonConvert.SerializeObject(testInfo), Encoding.UTF8, "application/json"));
//
//				result = httpClient.PostAsync(restUrl, content).Result;
//				Console.WriteLine(result);
			}
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
		/// Saves all stable file to file server
		/// </summary>
		/// <param name="stableFile">File to send</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SendStableFile(FileInfo stableFile, string testName)
		{
			Push(stableFile, testName, TestTypes.stable);
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
			var pullUrl = string.Format("{0}/api/{1}/{2}/{3}", ServerAddress, ProjectId, testName, TestTypes.stable);
			var hasStable = GetFilesFromServer(outputStableFilesDirectory, pullUrl).Result;

			if (!hasStable) Push(outputStableFilesDirectory, testName, TestTypes.diff);
			//we push null content for notify server; crutch but works

			return hasStable;
		}

		/// <summary>
		/// Download stable file from server to the specified folder
		/// </summary>
		/// <param name="outputStableFilesDirectory">Directory to put stable file</param>
		/// <param name="fileName">Name of the stable file</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		/// <returns></returns>
		public bool GetStableFile(DirectoryInfo outputStableFilesDirectory, string fileName, string testName)
		{
			var pullUrl = string.Format("{0}/api/{1}/{2}/{3}/{4}", ServerAddress, ProjectId, testName, fileName, TestTypes.stable);
			var hasStable = GetFilesFromServer(outputStableFilesDirectory, pullUrl).Result;

			if (!hasStable) Push(outputStableFilesDirectory, testName, TestTypes.diff);
			//we push null content for notify server; crutch but works

			return hasStable;
		}

		public async Task<string> GetStableFileHash(string testName, string fileName, TestInfo testInfo)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/stable/hash", ServerAddress, ProjectId, testName, fileName);

			using (var httpClient = new HttpClient())
			{
				var stringContent = new StringContent(JsonConvert.SerializeObject(testInfo), Encoding.UTF8, "application/json");
				var result = httpClient.PostAsync(restUrl, stringContent).Result;
				if (result.StatusCode == HttpStatusCode.NoContent)
				{
					return string.Empty;
				}

				return await result.Content.ReadAsStringAsync();
			}

//			return GetResultFromServer(restUrl).Result;
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
		/// Mark test as passed on server
		/// </summary>
		/// <param name="testName"></param>
		public void SayTestOkToServer(string testName, TestInfo testInfo)
		{
			string url = string.Format("{0}/api/{1}/{2}/{3}/status/passed", ServerAddress, ProjectId, SuiteId, testName);

			using (var httpClient = new HttpClient())
			{
				var stringContent = new StringContent(JsonConvert.SerializeObject(testInfo), Encoding.UTF8, "application/json");
				var result = httpClient.PostAsync(url, stringContent).Result;
			}
		}

		/// <summary>
		/// Saves specified diff file to the file server
		/// </summary>
		/// <param name="diffFile">File to send to the server</param>
		/// <param name="testName">Unique test name (will be used to search for files/folders on server)</param>
		public void SendDiffFile(FileInfo diffFile, string testName)
		{
			Push(diffFile, testName, TestTypes.diff);
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

		/// <summary>
		/// Post message to specified url
		/// </summary>
		/// <param name="url">Url to send message</param>
		/// <param name="message">Message</param>
		private string PostMessage(string url, string message)
		{
			Console.WriteLine("Posting message {0}", message);
			using (var httpClient = new HttpClient())
			{
				var content = new StringContent(message);
				var result = httpClient.PostAsync(url, content).Result;
				return result.Content.ReadAsStringAsync().Result;
			}
		}

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
				var content = new MultipartFormDataContent();
				HttpResponseMessage result;
				if (dir == null)
				{
					result = httpClient.PostAsync(url, content).Result;
					Console.WriteLine(result);
					return;
				}

				var inputFiles = dir.GetFiles();

				foreach (FileInfo file in inputFiles)
				{
					var fs = File.Open(file.FullName, FileMode.Open);
					var fileContent = new StreamContent(fs);
					content.Add(fileContent, "file", file.Name);
				}

				result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		/// <summary>
		/// Saves file to server. Send POST request to specified url with MultipartFormDataContent
		/// </summary>
		/// <param name="fileToSend">File which you want to send to the server</param>
		/// <param name="url">RestAPI url</param>
		private static void SendFileToServer(FileInfo fileToSend, string url)
		{
			using (var httpClient = new HttpClient())
			{
				var content = new MultipartFormDataContent();
				HttpResponseMessage result;
				fileToSend.Refresh();
				if (!fileToSend.Exists)
				{
					result = httpClient.PostAsync(url, content).Result;
					Console.WriteLine(result);
					return;
				}

				var fs = File.Open(fileToSend.FullName, FileMode.Open);
				var fileContent = new StreamContent(fs);
				content.Add(fileContent, "file", fileToSend.Name);

				result = httpClient.PostAsync(url, content).Result;
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

		private static async Task<string> GetResultFromServer(string url)
		{
			using (var httpClient = new HttpClient())
			{
				var result = httpClient.GetAsync(url).Result;
				if (result.StatusCode == HttpStatusCode.NoContent)
				{
					return string.Empty;
				}

				return await result.Content.ReadAsStringAsync();
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
