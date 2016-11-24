using System;
using System.Collections.Generic;

namespace VCT.Server.Entities
{
	public class Project
	{
		public Int64 Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class Suite
	{
		public Int64 Id { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public string Name { get; set; }

		public Int64 ProjectId { get; set; }
		public ICollection<Test> Tests { get; set; }
	}

	public class Test
	{
		public Int64 Id { get; set; }
		//primary key
		public string Name { get; set; }

		public virtual ICollection<ArtifactFile> TestingFiles { get; set; }
		public virtual ICollection<ArtifactFile> DiffFiles { get; set; }

		public bool Passed { get; set; }

		public Int64 SuiteId { get; set; }

		public Test()
		{
			TestingFiles = new List<ArtifactFile>();
			DiffFiles = new List<ArtifactFile>();
		}
	}

	public class StableFile
	{
		public Int64 Id { get; set; }

		public string FileName { get; set; }
		public string Value { get; set; }

		public string TestName { get; set; }
		public string ProjectName { get; set; }
	}

	public class ArtifactFile
	{
		public Int64 Id { get; set; }
		
		public string Value { get; set; }
		public string FileName { get; set; }
		public ArtifactType Type { get; set; }

		public Int64 TestId { get; set; }
		public Test Test { get; set; }

	}

	public enum ArtifactType
	{
		Stable,
		Testing,
		Diff
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
