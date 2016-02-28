using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using NUnit.Framework;

namespace VCT.Test
{
	[TestFixture]
	public class SecondFakeTestFixture
	{
//		string baseAddress = "http://localhost:9111/";
//
//		[Test]
//		public void ConnectionTest()
//		{
//			HttpClient client = new HttpClient();
//
//			var response = client.GetAsync(baseAddress + "api/values").Result;
//
//			Console.WriteLine(response);
//			Console.WriteLine(response.Content.ReadAsStringAsync().Result);
//		}
		string baseAddress = "http://localhost:9111/";
		[Test]
		public void FakeTest()
		{
			string url = string.Format("{0}/tests/{1}/stable", baseAddress, "BBBD");
			var file = new FileInfo(@"F:\Projects\VCT.Test\T.txt");
			
//			using (var client = new HttpClient())
//			{
//				var request = new HttpRequest();
//				
//				var form = new MultipartFormDataContent();
//				form.Add(new StreamContent(file.OpenRead()), "\"file\"", "\"" + file.Name + "\"");
//				client.PostAsync(url, form);
//			}
//			var core = Core.Instance;
//
//			core.MakeScreenshot(TestContext.CurrentContext.Test.FullName);


		}

		
	}
}
