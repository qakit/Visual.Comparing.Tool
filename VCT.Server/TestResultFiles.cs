using System.Collections.Generic;

namespace VCT.Server
{
	public class Project
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int SuitesCount { get; set; }
	}

	public class Suite
	{
		public string DateStarted { get; set; }
		public string DateCompleted { get; set; }
		public int Passed { get; set; }
		public int Failed { get; set; }
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class FailedTest
	{
		public string TestName { get; set; }
		public int EnvironmentId { get; set; }
		public List<Tuple> Artifacts { get; set; } 
	}

	public class Tuple
	{
		public File StableFile { get; set; }
		public File TestingFile { get; set; }
		public File DiffFile { get; set; }
	}

	public class File
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}
}
