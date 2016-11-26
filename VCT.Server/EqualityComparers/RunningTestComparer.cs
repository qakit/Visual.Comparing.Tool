using System.Collections.Generic;
using VCT.Server.Entities;

namespace VCT.Server.EqualityComparers
{
	public class RunningTestComparer : IEqualityComparer<RunningTest>
	{
		public bool Equals(RunningTest x, RunningTest y)
		{
			if (x == null && y == null) return true;
			if (x == null || y == null) return false;

			return x.SuiteId == y.SuiteId && x.TestId == y.TestId && x.EnvironmentId == y.EnvironmentId;
		}

		public int GetHashCode(RunningTest obj)
		{
			return obj.GetHashCode();
		}
	}

	public class RunningTestResultComparer : IEqualityComparer<RunningTestResult>
	{
		public bool Equals(RunningTestResult x, RunningTestResult y)
		{
			if (x == null && y == null) return true;
			if (x == null || y == null) return false;

			return x.Name == y.Name;
		}

		public int GetHashCode(RunningTestResult obj)
		{
			return obj.GetHashCode();
		}
	}
}
