﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shuttle.Recall.EFCore.SqlServer.EventProcessing;

#nullable disable

namespace Shuttle.Recall.EFCore.SqlServer.EventProcessing.Migrations
{
    [DbContext(typeof(EventProcessingDbContext))]
    partial class EventProcessingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("EventStore")
                .HasAnnotation("ProductVersion", "7.0.20")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Shuttle.Recall.EFCore.SqlServer.EventProcessing.Models.Projection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(650)
                        .HasColumnType("nvarchar(650)");

                    b.Property<long>("SequenceNumber")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "IX_Projection")
                        .IsUnique();

                    b.ToTable("Projection", "EventStore");
                });

            modelBuilder.Entity("Shuttle.Recall.EFCore.SqlServer.EventProcessing.Models.ProjectionJournal", b =>
                {
                    b.Property<Guid>("ProjectionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("SequenceNumber")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CorrelationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ManagedThreadId")
                        .HasColumnType("int");

                    b.HasKey("ProjectionId", "SequenceNumber");

                    b.ToTable("ProjectionJournal", "EventStore");
                });
#pragma warning restore 612, 618
        }
    }
}