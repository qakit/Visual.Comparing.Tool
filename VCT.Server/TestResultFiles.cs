using System.Collections.Generic;

namespace VCT.Server
{
	public class History
	{
		public string DateStarted { get; set; }
		public string DateCompleted { get; set; }
		public int Passed { get; set; }
		public int Failed { get; set; }
		public int Id { get; set; }
		public List<Test> Tests { get; set; } 
	}

	public class Test
	{
		public string TestName { get; set; }
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
		public string Path { get; set; }
		public string RelativePath { get; set; }
	}
}
