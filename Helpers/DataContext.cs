using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Helpers
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }
        public DbSet<PostWithoutUserId> PostsMin { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder
        //     .Entity<User>(
        //         builder =>
        //             {
        //                 builder
        //                 .HasMany(p => p.Posts)
        //                 .WithOne(e => e.User);
                        
        //             });

            //         modelBuilder
            // .Entity<Post>(
            //     builder =>
            //         {
            //             builder
            //             .HasOne(p => p.User)
            //             .WithMany(e => e.Posts);
                        
            //         });
    }
    }
}