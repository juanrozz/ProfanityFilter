using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProfanityFilter.Entities;

public partial class InternsDbContext : DbContext
{
    public InternsDbContext()
    {
    }

    public InternsDbContext(DbContextOptions<InternsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BannedWordsJdr> BannedWordsJdrs { get; set; }

    public virtual DbSet<ExceptionWordsJdr> ExceptionWordsJdrs { get; set; }

    public virtual DbSet<LanguagesJdr> LanguagesJdrs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BannedWordsJdr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BannedWo__3214EC0737E43982");

            entity.ToTable("BannedWords_JDR");

            entity.HasIndex(e => new { e.Word, e.LanguageId }, "UQ_Word_Language_JDR").IsUnique();

            entity.Property(e => e.Word).HasMaxLength(100);

            entity.HasOne(d => d.Language).WithMany(p => p.BannedWordsJdrs)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK_BannedWords_Languages_JDR");
        });

        modelBuilder.Entity<ExceptionWordsJdr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exceptio__3214EC0779855738");

            entity.ToTable("ExceptionWords_JDR");

            entity.Property(e => e.Word)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Language).WithMany(p => p.ExceptionWordsJdrs)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExceptionWords_Languages");
        });

        modelBuilder.Entity<LanguagesJdr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Language__3214EC079A93129F");

            entity.ToTable("Languages_JDR");

            entity.HasIndex(e => e.Name, "UQ__Language__737584F603B8FD8F").IsUnique();

            entity.HasIndex(e => e.Code, "UQ__Language__A25C5AA73CAADF94").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
