using BlogMVC.Models;
using System.Data.Entity;

namespace BlogMVC.DAL
{
    public class BlogContext: DbContext
    {
        public BlogContext():base("BlogDB")
        {

        }

        
        public DbSet<BlogPostModel> BlogPostTable { get; set; }
        public DbSet<Image> ImagesTable { get; set; }


    }
}