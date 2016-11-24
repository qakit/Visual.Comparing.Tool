using System.Collections.Generic;
using VCT.Server.Entities;

namespace VCT.Server.EqualityComparers
{
	public class ArtifactFileComparer : IEqualityComparer<ArtifactFile>
	{
		public bool Equals(ArtifactFile x, ArtifactFile y)
		{
			if (x == null && y == null) return true;
			if (x == null || y == null) return false;

			return x.FileName == y.FileName && x.Value == y.Value && x.Type == y.Type;
		}

		public int GetHashCode(ArtifactFile obj)
		{
			return obj.GetHashCode();
		}
	}
}
