namespace DBTest
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;

	public partial class VCTModelCodeFirst : DbContext
	{
		public VCTModelCodeFirst()
			: base("name=VCTModelCodeFirst")
		{
		}

		public virtual DbSet<Browser> Browsers { get; set; }
		public virtual DbSet<Environment> Environments { get; set; }
		public virtual DbSet<Project> Projects { get; set; }
		public virtual DbSet<Resolution> Resolutions { get; set; }
		public virtual DbSet<RunningOS> RunningOS { get; set; }
		public virtual DbSet<RunningSuite> RunningSuites { get; set; }
		public virtual DbSet<RunningTest> RunningTests { get; set; }
		public virtual DbSet<StableFile> StableFiles { get; set; }
		public virtual DbSet<Suite> Suites { get; set; }
		public virtual DbSet<Test> Tests { get; set; }
		public virtual DbSet<TestResult> TestResults { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Browser>()
				.Property(e => e.Name)
				.IsFixedLength()
				.IsUnicode(false);

			modelBuilder.Entity<Browser>()
				.HasMany(e => e.Environments)
				.WithRequired(e => e.Browser)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Environment>()
				.HasMany(e => e.RunningSuites)
				.WithRequired(e => e.Environment)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Environment>()
				.HasMany(e => e.StableFiles)
				.WithRequired(e => e.Environment)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Project>()
				.Property(e => e.Name)
				.IsFixedLength();

			modelBuilder.Entity<Project>()
				.HasMany(e => e.Suites)
				.WithRequired(e => e.Project)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Resolution>()
				.HasMany(e => e.Environments)
				.WithRequired(e => e.Resolution)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<RunningOS>()
				.Property(e => e.OS)
				.IsFixedLength();

			modelBuilder.Entity<RunningOS>()
				.HasMany(e => e.Environments)
				.WithRequired(e => e.RunningOs)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<RunningSuite>()
				.HasMany(e => e.RunningTests)
				.WithRequired(e => e.RunningSuite)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<RunningTest>()
				.HasMany(e => e.TestResults)
				.WithRequired(e => e.RunningTest)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<StableFile>()
				.HasMany(e => e.TestResults)
				.WithRequired(e => e.StableFile)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Suite>()
				.Property(e => e.Name)
				.IsFixedLength();

			modelBuilder.Entity<Suite>()
				.HasMany(e => e.RunningSuites)
				.WithRequired(e => e.Suite)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Suite>()
				.HasMany(e => e.Tests)
				.WithRequired(e => e.Suite)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Test>()
				.Property(e => e.Name)
				.IsFixedLength();

			modelBuilder.Entity<Test>()
				.HasMany(e => e.RunningTests)
				.WithRequired(e => e.Test)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Test>()
				.HasMany(e => e.StableFiles)
				.WithRequired(e => e.Test)
				.WillCascadeOnDelete(false);
		}
	}
}
