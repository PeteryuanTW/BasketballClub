using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BasketballClub.EFModels
{
    public partial class BasketballClubDBContext : DbContext
    {
        public BasketballClubDBContext()
        {
        }

        public BasketballClubDBContext(DbContextOptions<BasketballClubDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BulltinContent> BulltinContents { get; set; } = null!;
        public virtual DbSet<Court> Courts { get; set; } = null!;
        public virtual DbSet<Game> Games { get; set; } = null!;
        public virtual DbSet<GameParticipant> GameParticipants { get; set; } = null!;
        public virtual DbSet<UserInfo> UserInfos { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=127.0.0.1;Database=BasketballClubDB;Trusted_Connection=True; trustServerCertificate=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BulltinContent>(entity =>
            {
                entity.ToTable("BulltinContent");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("ID");

                entity.Property(e => e.Author).HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(50);
            });

            modelBuilder.Entity<Court>(entity =>
            {
                entity.HasKey(e => e.CourtName);

                entity.ToTable("Court");

                entity.Property(e => e.CourtName).HasMaxLength(50);

                entity.Property(e => e.NickName).HasMaxLength(50);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Game");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("ID");

                entity.Property(e => e.EndTine).HasColumnType("datetime");

                entity.Property(e => e.Host).HasMaxLength(50);

                entity.Property(e => e.Place).HasMaxLength(50);

                entity.Property(e => e.StartTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<GameParticipant>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ParticipantName });

                entity.ToTable("GameParticipant");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("ID");

                entity.Property(e => e.ParticipantName).HasMaxLength(50);
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);

                entity.ToTable("UserInfo");

                entity.Property(e => e.EmployeeId)
                    .ValueGeneratedNever()
                    .HasColumnName("EmployeeID");

                entity.Property(e => e.SessionKey).HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
