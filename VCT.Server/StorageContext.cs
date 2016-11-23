using System.Data.Entity;
using SQLite.CodeFirst;

namespace VCT.Server
{
	public class StorageContext : DbContext
	{
		public StorageContext() : base("name=Storage") { }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<StorageContext>(modelBuilder);
			Database.SetInitializer(sqliteConnectionInitializer);
			//TODO fill browsers, artifact file type tables here
		}

		public virtual DbSet<Entities.Project> Projects { get; set; }
		public virtual DbSet<Entities.Suite> Suites { get; set; }
		public virtual DbSet<Entities.Test> Tests { get; set; }
		public virtual DbSet<Entities.Resolution> Resolutions { get; set; }
		public virtual DbSet<Entities.Browser> Browsers { get; set; }
		public virtual DbSet<Entities.Environment> Environments { get; set; }
		public virtual DbSet<Entities.TestRunStatus> TestRunStatuses { get; set; }
		public virtual DbSet<Entities.ArtifactFile> ArtifactFiles { get; set; }
		public virtual DbSet<Entities.ArtifactFileType> ArtifactFileTypes { get; set; }
	}
}
