using System.Collections.Generic;

namespace VCT.Server
{
	public class HistoryResult
	{
		public string DateStarted { get; set; }
		public string DateCompleted { get; set; }
		public int Passed { get; set; }
		public int Failed { get; set; }
		public int Id { get; set; }
		public List<TestResult> Tests { get; set; } 
	}

	public class TestResult
	{
		public string TestName { get; set; }
		public List<TestArtifacts> Artifacts { get; set; } 
	}

	public class TestArtifacts
	{
		public TestArtifact StableFile { get; set; }
		public TestArtifact TestingFile { get; set; }
		public TestArtifact DiffFile { get; set; }
	}

	public class TestArtifact
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string RelativePath { get; set; }
	}
}
