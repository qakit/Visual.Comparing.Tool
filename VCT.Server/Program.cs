using System;
using Microsoft.Owin.Hosting;
using VCT.Server.Entities;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	class Program
	{
		static readonly string BaseAddress = Config.AppSettings["rootUrl"];

		static void Main(string[] args)
		{
			using (var db = new StorageContext())
			{
				//				//Add project
				db.Projects.Add(new Entities.Project {Name = "Designer"});
				db.SaveChanges();
//
//				//Add suite
//				var project = (from p in db.Projects
//					where p.Name == "Designer"
//					select p).FirstOrDefault();
//				var suite = new Entities.Suite {StartTime = DateTime.Now, ProjectId = project.Id};
//				db.Suites.Add(suite);
//				db.SaveChanges();
//
//				//Add test
//				var test = new Entities.Test {Name = "Sample Test"};
//				db.Tests.Add(test);
//				db.SaveChanges();
//				//get environment
//				var browser = new Browser {Name = "FF"};
//				db.Browsers.Add(browser);
//				db.SaveChanges();
//				var resolution = new Resolution {Height = 1024, Width = 600};
//				db.Resolutions.Add(resolution);
//				db.SaveChanges();
//				var environment = new Entities.Environment {BrowserId = browser.Id, ResolutionId = resolution.Id};
//				db.Environments.Add(environment);
//				db.SaveChanges();
//
//				//add files for test
//				var resultType = new ArtifactFileType {Type = "Passed"};
//				db.ArtifactFileTypes.Add(resultType);
//				db.SaveChanges();
//				var result = new ArtifactFile
//				{
//					FullPath = "Full Pasdfsth",
//					RelativePath = "Relsdfsdative",
//					Name = "Nsdfsdfame",
//					ArtifactFileTypeId = resultType.Id
//				};
//				var result2 = new ArtifactFile
//				{
//					FullPath = "Full Path2",
//					RelativePath = "Relative2",
//					Name = "Name2",
//					ArtifactFileTypeId = resultType.Id
//				};
//				
//				db.ArtifactFiles.Add(result);
//				db.ArtifactFiles.Add(result2);
//				db.SaveChanges();
//
//				//add test result
//				var testrunresult = new TestRunStatus
//				{
//					Artifacts = new List<ArtifactFile> { result, result2 },
//					EnvironmentId = environment.Id,
//					Passed = false,
//					SuiteId = suite.Id,
//					TestId = test.Id
//				};
//
//				db.TestRunStatuses.Add(testrunresult);
//				db.SaveChanges();
			}
//			using (var db = new BloggingContext())
//			{
//				// Create and save a new Blog 
//				Console.Write("Enter a name for a new Blog: ");
//				var name = Console.ReadLine();
//
//				var blog = new Blog { Name = name };
//				db.Blogs.Add(blog);
//				db.SaveChanges();
//
//				// Display all Blogs from the database 
//				var query = from b in db.Blogs
//							orderby b.Name
//							select b;
//
//				Console.WriteLine("All blogs in the database:");
//				foreach (var item in query)
//				{
//					Console.WriteLine(item.Name);
//				}
//
//				Console.WriteLine("Press any key to exit...");
//				Console.ReadKey();
//			} 


			WebApp.Start<Startup>(BaseAddress);
			Console.WriteLine("Server started and waiting");
			Console.ReadLine();
		}
	}
}
