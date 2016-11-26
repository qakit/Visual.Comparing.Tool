using System;
using System.Data.Entity;
using VCT.Server.Entities;

namespace VCT.Server
{
	public class StorageContext : DbContext
	{
		public StorageContext() : base("name=Storage")
		{
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var initializer = new StorageDbInitializer(modelBuilder);
			Database.SetInitializer(initializer);
		}

		public virtual DbSet<Entities.Project> Projects { get; set; }
		public virtual DbSet<Entities.Suite> Suites { get; set; }
		public virtual DbSet<Entities.Test> Tests { get; set; }
		public virtual DbSet<Entities.RunningTest> RunningTests { get; set; }
		public virtual DbSet<Entities.RunningTestResult> RunningTestResults { get; set; }
		public virtual DbSet<Entities.StableFile> StableFiles { get; set; }

		public virtual DbSet<Resolution> Resolutions { get; set; }
		public virtual DbSet<Browser> Browsers { get; set; }
		public virtual DbSet<Entities.Environment> Environments { get; set; }
	}
}
