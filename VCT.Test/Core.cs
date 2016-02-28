using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace VCT.Test
{
	public sealed class Core
	{
		private static readonly Core instance = new Core();
		string baseAddress = "http://localhost:9111/";
		HttpClient client;
		private Core()
		{
			client = new HttpClient();
			AppDomain.CurrentDomain.DomainUnload += CurrentDomainOnDomainUnload;
			Console.WriteLine("Group started");
		}

		public void SaveStableOutput()
		{
			var response = client.GetAsync(baseAddress + "api/values").Result;
		}

		public async void MakeScreenshot(string testName)
		{
			PostData();
//			string url = string.Format("{0}/tests/{1}/stable", baseAddress, testName);
//			var response = await client.GetAsync(url).Result.Content.ReadAsStringAsync();
//
//			Console.WriteLine(response);
		}

		public async void PostData()
		{
			var file = @"F:\Projects\VCT.Test\T.txt";
			var image = File.ReadAllBytes(file);
			string url = string.Format("{0}/tests/{1}/stable", baseAddress, "BBBD");
			HttpClient httpClient = new HttpClient();
			httpClient.BaseAddress = new Uri(url);
			MultipartFormDataContent form = new MultipartFormDataContent();
			HttpContent content = new StringContent(file);
			form.Add(content, file);
			var stream = new MemoryStream(File.ReadAllBytes(file));
			content = new StreamContent(stream);
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
			{
				Name = file,
				FileName = "T.txt"
			};
			form.Add(content);
			var response = await httpClient.PostAsync(url, form);
			var x = 0;
		}

		private void CurrentDomainOnDomainUnload(object sender, EventArgs eventArgs)
		{
			MessageBox.Show("All is done");
		}

		public static Core Instance
		{
			get { return instance; }
		}
	}
}
