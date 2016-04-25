using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Config = System.Configuration.ConfigurationManager;

namespace VCT.Server
{
	public class Storage
	{
		public readonly DirectoryInfo Root;
		public static Hub Current;

		static readonly string HistoryFileName = Config.AppSettings["history"];

		public Storage()
		{
			Root = Directory.CreateDirectory(Config.AppSettings["storage"]);
		}

		public class Hub //nested bcoz it shouldn't be used without Storage context
		{
			public string DebugInfo()
			{
				return string.Join(" |\r", root.FullName, stable.FullName, testing.FullName, diff.FullName);
			}

			private DirectoryInfo _root;
			private DirectoryInfo _stable;
			private DirectoryInfo _testing;
			private DirectoryInfo _diff;

			public Hub(DirectoryInfo branch)
			{
				_root = branch;
				_stable = Directory.CreateDirectory(_root.FullName + @"/StableFiles");
				_testing = Directory.CreateDirectory(_root.FullName + @"/TestingFiles");
				_diff = Directory.CreateDirectory(_root.FullName + @"/DiffFiles");
			}

			public DirectoryInfo root { get { return _root; } }
			public DirectoryInfo stable { get { return _stable; } }
			public DirectoryInfo testing { get { return _testing; } }
			public DirectoryInfo diff { get { return _diff; } }


			#region old wrapers

			public DirectoryInfo StableTestDirectory(string testName)
			{
				return Directory.CreateDirectory(stable.FullName + @"/" + testName);
			}

			public DirectoryInfo TestingTestDirectory(string testName)
			{
				return Directory.CreateDirectory(testing.FullName + @"/" + testName);
			}

			public DirectoryInfo DiffTestDirectory(string testName)
			{
				return Directory.CreateDirectory(diff.FullName + @"/" + testName);
			}

			#endregion

		}




		public DirectoryInfo GetLatestExistingStable(string testIdentifyer)
		{
			var search = Root.GetDirectories("*", SearchOption.AllDirectories)
							 .Where(d => d.Parent.Name.Equals("StableFiles") &&
										 d.Name.Equals(testIdentifyer) &&
										 d.GetFiles().Any())
							 .OrderBy(d => d.CreationTime);
			return search.LastOrDefault();
		}




		/// <summary>
		/// Writes info text to history file
		/// </summary>
		/// <param name="infoText">text</param>
		/// <param name="removeIfExists">do we need to remove previous file if it exists (optional. Default - false)</param>
		public void WriteHistoryInfo(string infoText, bool removeIfExists = false)
		{
			//TODO: separate logs for dif suits
			var historyFile = new FileInfo(Path.Combine(Root.FullName, HistoryFileName));

			using (var writer = new StreamWriter(historyFile.FullName, true))
			{
				writer.WriteLine(infoText);
			}
		}


		public void Allocate(string inceptionTime)
		{
			DirectoryInfo branch = Directory.CreateDirectory(Root.FullName + @"/" + inceptionTime);
			Current = new Hub(branch);
		}
	}
}
