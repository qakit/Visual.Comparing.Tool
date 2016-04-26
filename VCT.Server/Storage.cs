﻿using System;
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
			private DirectoryInfo _proj;
			private DirectoryInfo _stable;
			private DirectoryInfo _testing;
			private DirectoryInfo _diff;

			public Hub(DirectoryInfo branch)
			{
				_root =		branch;
				_proj =		branch.Parent;
				_stable =	_root.CreateSubdirectory("StableFiles");
				_testing =	_root.CreateSubdirectory("TestingFiles");
				_diff =		_root.CreateSubdirectory("DiffFiles");
			}

			public DirectoryInfo root		{ get { return _root; } }
			public DirectoryInfo proj		{ get { return _proj; } }
			public DirectoryInfo stable		{ get { return _stable; } }
			public DirectoryInfo testing	{ get { return _testing; } }
			public DirectoryInfo diff		{ get { return _diff; } }


			#region old wrapers

			public DirectoryInfo StableTestDirectory(string testName)
			{
				return stable.CreateSubdirectory(testName);
			}

			public DirectoryInfo TestingTestDirectory(string testName)
			{
				return testing.CreateSubdirectory(testName);
			}

			public DirectoryInfo DiffTestDirectory(string testName)
			{
				return diff.CreateSubdirectory(testName);
			}

			#endregion

		}




		public DirectoryInfo GetLatestExistingStable(string testIdentifyer)
		{
			//TODO: dont search in all, search just in proj dir

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
		/// <param name="projId"></param>
		/// <param name="infoText">text</param>
		/// <param name="removeIfExists">do we need to remove previous file if it exists (optional. Default - false)</param>
		public void WriteLog(string projId, string infoText, bool removeIfExists = false)
		{
			DirectoryInfo projDir = Root.CreateSubdirectory(projId);

			var historyFile = new FileInfo(Path.Combine(projDir.FullName, HistoryFileName));

			using (var writer = new StreamWriter(historyFile.FullName, true))
			{
				writer.WriteLine(infoText);
			}
		}


		public void Allocate(string projId, string inceptionTime)
		{
			DirectoryInfo branch = Root.CreateSubdirectory(projId).CreateSubdirectory(inceptionTime);
			Current = new Hub(branch);
		}
	}
}
