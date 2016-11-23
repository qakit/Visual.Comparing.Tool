using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.CodeFirst;

namespace VCT.Server.Entities
{
//	public class Blog
//	{
//		public Int64 BlogId { get; set; }
//		public string Name { get; set; }
//
//		public virtual List<Post> Posts { get; set; }
//	}
//
//	public class Post
//	{
//		public Int64 PostId { get; set; }
//		public string Title { get; set; }
//		public string Content { get; set; }
//
//		public Int64 BlogId { get; set; }
//		public virtual Blog Blog { get; set; }
//	}
//
//	public class BloggingContext : DbContext
//	{
//		public BloggingContext() : base("name=Storage") { }
//
//		protected override void OnModelCreating(DbModelBuilder modelBuilder)
//		{
//			var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<BloggingContext>(modelBuilder);
//			Database.SetInitializer(sqliteConnectionInitializer);
//		}
//
//		public DbSet<Blog> Blogs { get; set; }
//		public DbSet<Post> Posts { get; set; }
//	}
}
