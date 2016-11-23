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
		public virtual DbSet<Test> Tests { get; set; }
		public virtual DbSet<Resolution> Resolutions { get; set; }
		public virtual DbSet<Browser> Browsers { get; set; }
		public virtual DbSet<Entities.Environment> Environments { get; set; }
		public virtual DbSet<TestRunStatus> TestRunStatuses { get; set; }
		public virtual DbSet<ArtifactFile> ArtifactFiles { get; set; }
		public virtual DbSet<ArtifactFileType> ArtifactFileTypes { get; set; }
		public virtual DbSet<StableTestFile> StableTestFiles { get; set; }
	}
}
