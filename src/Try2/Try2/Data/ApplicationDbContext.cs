using Microsoft.EntityFrameworkCore;
using Try2.Models;

namespace Try2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostLike> Likes { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            // StudyGroup отношения
            modelBuilder.Entity<StudyGroup>()
                .HasOne(sg => sg.Notebook)
                .WithMany(n => n.StudyGroups)
                .HasForeignKey(sg => sg.NotebookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudyGroup>()
                .HasOne(sg => sg.User)
                .WithMany(u => u.StudyGroups)
                .HasForeignKey(sg => sg.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post -> User (1:M)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment отношения
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like -> User (1:M)
            modelBuilder.Entity<PostLike>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subscription отношения
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Follower)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.TargetUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<PostLike>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique();

            modelBuilder.Entity<CommentLike>()
                .HasIndex(l => new { l.UserId, l.CommentId })
                .IsUnique();

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => new { s.FollowerId, s.TargetUserId })
                .IsUnique();
        }
    }
}
