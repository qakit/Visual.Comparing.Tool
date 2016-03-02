using System.Collections.Generic;

namespace VCT.Server
{
	public class TestResultFiles
	{
		public List<string> DiffImages { get; set; }
		public List<string> TestingImages  { get; set; }
		public List<string> StableImages  { get; set; }
	}
}
