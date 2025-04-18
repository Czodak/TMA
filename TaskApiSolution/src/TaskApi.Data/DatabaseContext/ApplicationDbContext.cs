﻿using Microsoft.EntityFrameworkCore;
using TaskApi.Data.Entities;

namespace TaskApi.Data.DatabaseContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tasks>(entity =>
            {
                entity.ToTable("Tasks");
                entity.Property(t => t.Id) 
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();
                entity.Property(t => t.Status)
                    .HasConversion<int>(); // converts enum to int
            });
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Tasks> Tasks { get; set; }
    }
}
