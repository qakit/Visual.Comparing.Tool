using System.Collections.Generic;
using System.Data.Entity;
using SQLite.CodeFirst;
using VCT.Server.Entities;

namespace VCT.Server
{
	public class StorageDbInitializer : SqliteCreateDatabaseIfNotExists<StorageContext>
	{
		public StorageDbInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
		{
		}

		protected override void Seed(StorageContext context)
		{
			//Initialize predefined browsers
			context.Set<Browser>().
				AddRange(new List<Browser>
				{
					new Browser {Name = "chrome"}, 
					new Browser {Name = "ff"}, 
					new Browser {Name = "ie"}
				});

			//File types
//			context.Set<ArtifactFileType>()
//				.AddRange(new List<ArtifactFileType>
//				{
//					new ArtifactFileType {Type = "Stable"},
//					new ArtifactFileType {Type = "Testing"},
//					new ArtifactFileType {Type = "Diff"}
//				});
		}
	}
}