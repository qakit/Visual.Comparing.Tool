using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Remote;

namespace VCT.Test
{
	public sealed class Core
	{
		private static readonly Core instance = new Core();
		private const string BaseAddress = "http://localhost:9111/";
		
		private Core()
		{
			//necessary to identify time when all tests are finished;
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
		}

		public async void MakeScreenshot(RemoteWebDriver driver, FileInfo outputFile, string testName)
		{
			var c = new VCTCore(driver);
			c.MakeFullScreenshot(outputFile);
			var stableOutputDir = new DirectoryInfo(Path.Combine(outputFile.Directory.FullName, "STABLE"));

			var stableFiles = await GetStableFiles(stableOutputDir, testName);
			if (!stableFiles)
			{
				PostTestingFiles(outputFile.Directory, testName);
			}
			else
			{
				var equal = CompareScreenshots(outputFile, stableOutputDir);
				if (!equal)
				{
					//POST DIFF HERE;
					Console.WriteLine("FILE IS DIFFERENT");
					DirectoryInfo outputDir = new DirectoryInfo(Path.Combine(outputFile.Directory.FullName, "DIFF"));
					PostDiffFiles(outputDir, testName);
					//POST TESTING IMAGE
					PostTestingFiles(outputFile.Directory, testName);
					Console.WriteLine("Posting images is done");
				}
				else
				{
					Console.WriteLine("ALL IS FINE");
					//TODO delete testing images if any and diff images on the server
				}
				Console.WriteLine("COMPARE AND PUT DIFF IF NECESSARY TO SERVER");
			}
		}

		private bool CompareScreenshots(FileInfo outputTestingFile, DirectoryInfo stableFilesFolder)
		{
			var stableImageFile = new FileInfo(Path.Combine(stableFilesFolder.FullName, outputTestingFile.Name));

			return ImageFileComparer.ComparingFilesAreEqual(stableImageFile, outputTestingFile);
		}

		/// <summary>
		/// Get stable files for specific test and put them to specified outputdirectory
		/// </summary>
		/// <param name="outputDirectory"></param>
		/// <param name="testName"></param>
		/// <returns></returns>
		public async Task<bool> GetStableFiles(DirectoryInfo outputDirectory, string testName)
		{
			string url = string.Format("{0}/tests/{1}/stable", BaseAddress, testName);
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
		/// Post all files from specified folder as testing files to file server
		/// </summary>
		/// <param name="inputDirectory"></param>
		/// <param name="testName"></param>
		public static void PostTestingFiles(DirectoryInfo inputDirectory, string testName)
		{
			string url = string.Format("{0}/tests/{1}/testing", BaseAddress, testName);

			using (var httpClient = new HttpClient())
			{
				var stableFiles = inputDirectory.GetFiles();
				var content = new MultipartFormDataContent();

				foreach (FileInfo stableFile in stableFiles)
				{
					var fs = File.Open(stableFile.FullName, FileMode.Open);
					var fileContent = new StreamContent(fs);
					content.Add(fileContent, "\"file\"", string.Format("\"{0}\"", stableFile.Name));
				}

				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		/// <summary>
		/// Post all files from specified folder as diff files to file server
		/// </summary>
		/// <param name="inputDirectory"></param>
		/// <param name="testName"></param>
		public static void PostDiffFiles(DirectoryInfo inputDirectory, string testName)
		{
			string url = string.Format("{0}/tests/{1}/diff", BaseAddress, testName);

			using (var httpClient = new HttpClient())
			{
				var stableFiles = inputDirectory.GetFiles();
				var content = new MultipartFormDataContent();

				foreach (FileInfo stableFile in stableFiles)
				{
					var fs = File.Open(stableFile.FullName, FileMode.Open);
					var fileContent = new StreamContent(fs);
					content.Add(fileContent, "\"file\"", string.Format("\"{0}\"", stableFile.Name));
				}

				var result = httpClient.PostAsync(url, content).Result;
				Console.WriteLine(result);
			}
		}

		private void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
		{
			//Pass end time of group test to server
			MessageBox.Show("All is done");
		}

		public static Core Instance
		{
			get { return instance; }
		}
	}
}
