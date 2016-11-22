using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace VCT.Sdk
{
	public class TestInfo
	{
		public string TestName { get; set; }
		public string Browser { get; set; }
		public Size WindowSize { get; set; }
		//todo artifacts??
		public List<FileInfo> Artifacts { get; set; } 
	}
}
