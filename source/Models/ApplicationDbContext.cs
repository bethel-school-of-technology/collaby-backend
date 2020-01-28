﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace collaby_backend.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=C:/repos/collaby-backend/source/collabyDB.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.Id);

                entity.Property(e => e.IsDraft).HasDefaultValueSql("0");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnType("VARCHAR(1000)");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.Id);

                entity.Property(e => e.DateCreated)
                    .HasColumnType("DATETIME")
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(e => e.DateModified).HasColumnType("DATETIME");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnType("VARCHAR(12500)");

                entity.Property(e => e.RatingValue).HasColumnType("DOUBLE");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID");

                entity.Property(e => e.Ratings).HasColumnType("NUMERIC(1,1)");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("DATETIME")
                    .HasDefaultValueSql("datetime('now')");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnType("VARCHAR(12500)");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("VARCHAR(40)");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnType("VARCHAR(40)");

                entity.Property(e => e.Followings).HasColumnType("VARCHAR(1400)");

                entity.Property(e => e.Img).HasColumnType("CHAR(10000)");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnType("VARCHAR(40)");

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasColumnType("VARCHAR(40)");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("VARCHAR(80)");

                entity.Property(e => e.RatedPosts).HasDefaultValueSql("0");

                entity.Property(e => e.TotalPosts).HasDefaultValueSql("0");

                entity.Property(e => e.TotalRating).HasColumnType("DOUBLE");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnType("VARCHAR(40)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
