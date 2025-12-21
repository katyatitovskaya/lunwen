using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyLunWen.Models;
using System.Reflection.Emit;

namespace MyLunWen.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet для ваших моделей
        public DbSet<Notebook> Notebooks { get; set; }
        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация отношений

            // User -> Notebook (1:M)
            modelBuilder.Entity<Notebook>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notebooks)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reply отношения
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Replies)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Author)
                .WithMany(u => u.Replies)
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like -> User (1:M)
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Subscription отношения
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Subscriber)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.SubscriberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.TargetUser)
                .WithMany()
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.ObjectType, l.ObjectId })
                .IsUnique();

            modelBuilder.Entity<Subscription>()
                .HasIndex(s => new { s.SubscriberId, s.TargetUserId })
                .IsUnique();

            // Конфигурация Identity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.Nickname).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.About).HasMaxLength(500);
            });
        }
    }
}



