using System.Collections.Generic;

namespace VCT.Server
{
	public class TestResultFiles
	{
		public List<TestResult> DiffImages { get; set; }
		public List<TestResult> TestingImages  { get; set; }
		public List<TestResult> StableImages  { get; set; }
	}

	public class TestResult
	{
		public string Name { get; set; }
		public string Path { get; set; }
	}
}
