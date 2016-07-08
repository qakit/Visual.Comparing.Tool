using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
			string url = string.Format("{0}/tests/{1}/suite/stop", ServerAddress, ProjectId);
			PostMessage(url, "Suite completed");
			SuiteId = "";
		}

		public void Push(DirectoryInfo dir, string nameOfTest, TestTypes type)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/{4}", ServerAddress, ProjectId, SuiteId, nameOfTest, type);
			SendFilesToServer(dir, restUrl);
		}

		public bool Pull(DirectoryInfo dir, string nameOfTest, TestTypes type)
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
			var pullUrl = string.Format("{0}/api/{1}/{2}/{3}", ServerAddress, ProjectId, testName, TestTypes.stable);
			var hasStable = GetFilesFromServer(outputStableFilesDirectory, pullUrl).Result;

			if (!hasStable) Push(outputStableFilesDirectory, testName, TestTypes.diff);
			//we push null content for notify server; crutch but works

			return hasStable;
		}

		public string GetStableFileHash(string testName, string fileName)
		{
			var restUrl = string.Format("{0}/api/{1}/{2}/{3}/stable/hash", ServerAddress, ProjectId, testName, fileName);
			return GetResultFromServer(restUrl).Result;
		}

		/// <summary>
		/// Computes hash for current file
		/// </summary>
		/// <param name="testingFile"></param>
		/// <returns></returns>
		public string ComputeFileHash(FileInfo testingFile)
		{
			SHA256 hash = SHA256Managed.Create();

			var fileStream = testingFile.Open(FileMode.Open);
			var hashValue = hash.ComputeHash(fileStream);
			var result = GetHexByteArray(hashValue);

			fileStream.Close();
			return result;
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
