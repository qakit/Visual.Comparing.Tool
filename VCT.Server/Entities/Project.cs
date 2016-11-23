using System;
using System.Collections;
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

		public Int64 ProjectId { get; set; }
	}

	public class Test
	{
		public Int64 Id { get; set; }
		public string Name { get; set; }
	}

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

	public class TestRunStatus
	{
		public Int64 Id { get; set; }
		public bool Passed { get; set; }

		public Int64 TestId { get; set; }
		public Int64 SuiteId { get; set; }
		public Int64 EnvironmentId { get; set; }

		public ICollection<ArtifactFile> Artifacts { get; set; }
	}

	public class ArtifactFile
	{
		public Int64 Id { get; set; }
		public string Name { get; set; }
		public string FullPath { get; set; }
		public string RelativePath { get; set; }

		public Int64 ArtifactFileTypeId { get; set; }
	}

	public class ArtifactFileType
	{
		public Int64 Id { get; set; }
		public string Type { get; set; }
	}
}
