using System;
using System.Collections.Generic;

namespace VCT.Server.Entities
{
	/// <summary>
	/// Represent a project
	/// </summary>
	public class Project
	{
		public Int64 Id { get; set; }
		/// <summary>
		/// Name of the project
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Project's description (if any)
		/// </summary>
		public string Description { get; set; }
	}

	public class Suite
	{
		public Int64 Id { get; set; }
		/// <summary>
		/// Start time
		/// </summary>
		public DateTime StartTime { get; set; }
		/// <summary>
		/// End time
		/// </summary>
		public DateTime? EndTime { get; set; }
		/// <summary>
		/// Name of the test run (suite)
		/// </summary>
		public string Name { get; set; }

		public Int64 ProjectId { get; set; }
		/// <summary>
		/// Collection of the tests related to the current tests run (suite)
		/// </summary>
		public virtual ICollection<RunningTest> Tests { get; set; }

		public Suite()
		{
			Tests = new List<RunningTest>();
		}
	}

	/// <summary>
	/// Represent an unique test which can exist in current project
	/// </summary>
	public class Test
	{
		public Int64 Id { get; set; }
		/// <summary>
		/// Name of the test
		/// </summary>
		public string Name { get; set; }

		public Int64 ProjectId { get; set; }
	}

	/// <summary>
	/// Test which is currently runned (executed)
	/// </summary>
	public class RunningTest
	{
		public Int64 Id { get; set; }
		public bool Passed { get; set; }

		/// <summary>
		/// Unique Test id which can get it's name
		/// </summary>
		public Int64 TestId { get; set; }
		public Int64 SuiteId { get; set; }
		public Int64 EnvironmentId { get; set; }

		/// <summary>
		/// Collection of the current running test results
		/// </summary>
		public virtual ICollection<RunningTestResult> TestResults { get; set; }

		public RunningTest()
		{
			TestResults = new List<RunningTestResult>();
		}
	}

	/// <summary>
	/// Represent a single test result including diff/testing file
	/// </summary>
	public class RunningTestResult
	{
		public Int64 Id { get; set; }

		/// <summary>
		/// Name of the test result (usually file name). Diff file and testing file should have same names
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Base64 representation of the testing file
		/// </summary>
		public string TestingFile { get; set; }

		/// <summary>
		/// Base64 representation of the testing file
		/// </summary>
		public string DiffFile { get; set; }

		public Int64 RunningTestId { get; set; }
	}

	public class StableFile
	{
		public Int64 Id { get; set; }
		public Int64 TestId { get; set; }
		public Int64 EnvironmentId { get; set; }

		/// <summary>
		/// Base64 representation of the stable file
		/// </summary>
		public string File { get; set; }
		public string Name { get; set; }
	}

	/// <summary>
	/// Represent a mask area which must be excluded from comparison
	/// </summary>
	public class Mask
	{
		public Int64 Id { get; set; }
		/// <summary>
		/// X offset from left top corner
		/// </summary>
		public Int64 XOffset { get; set; }
		/// <summary>
		/// Y offset from left top corner
		/// </summary>
		public Int64 YOffset { get; set; }
		/// <summary>
		/// Width of the rectangle to be exluded
		/// </summary>
		public Int64 Width { get; set; }
		/// <summary>
		/// Height of the rectangle to be exluded
		/// </summary>
		public Int64 Height { get; set; }

		public Int64 StableFileId { get; set; }
	}

	#region environment tables
	public class Browser
	{
		public Int64 Id { get; set;} 
		public string Name { get; set; }
	}

	public class Resolution
	{
		public Int64 Id { get; set; }
		public Int64 Width { get; set; }
		public Int64 Height { get; set; }
	}

	public class Environment
	{
		public Int64 Id { get; set; }
		public Int64 BrowserId { get; set; }
		public Int64 ResolutionId { get; set; }
	}
	#endregion
	
}
