using System.Collections.Generic;

namespace VCT.Server
{
	public class TestResult
	{
		public string TestName { get; set; }
		public List<TestArtifacts> Artifacts { get; set; } 
	}

	public class TestArtifacts
	{
		public List<TestArtifact> DiffImages { get; set; }
		public List<TestArtifact> TestingImages { get; set; }
		public List<TestArtifact> StableImages { get; set; }
	}

	public class TestArtifact
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string RelativePath { get; set; }
	}
}
