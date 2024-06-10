﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VoqooePlanner.DbContexts;

#nullable disable

namespace VoqooePlanner.Migrations
{
    [DbContext(typeof(VoqooeDbContext))]
    partial class VoqooeDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("CommanderVistedSystems", b =>
                {
                    b.Property<int>("CommanderVisitsId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("VoqooeSystemDTOAddress")
                        .HasColumnType("INTEGER");

                    b.HasKey("CommanderVisitsId", "VoqooeSystemDTOAddress");

                    b.HasIndex("VoqooeSystemDTOAddress");

                    b.ToTable("CommanderVistedSystems", (string)null);
                });

            modelBuilder.Entity("VoqooePlanner.DTOs.JournalCommanderDTO", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("JournalDir")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastFile")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("JournalCommanders", (string)null);
                });

            modelBuilder.Entity("VoqooePlanner.DTOs.JournalEntryDTO", b =>
                {
                    b.Property<string>("Filename")
                        .HasColumnType("TEXT");

                    b.Property<long>("Offset")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CommanderID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("EventTypeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Filename", "Offset");

                    b.ToTable("JournalEntries", (string)null);
                });

            modelBuilder.Entity("VoqooePlanner.DTOs.SettingsDTO", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<double?>("DoubleValue")
                        .HasColumnType("REAL");

                    b.Property<int?>("IntValue")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StringValue")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Settings", (string)null);
                });

            modelBuilder.Entity("VoqooePlanner.DTOs.VoqooeSystemDTO", b =>
                {
                    b.Property<long>("Address")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("ContainsELW")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("StarType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Visited")
                        .HasColumnType("INTEGER");

                    b.Property<double>("X")
                        .HasColumnType("REAL");

                    b.Property<double>("Y")
                        .HasColumnType("REAL");

                    b.Property<double>("Z")
                        .HasColumnType("REAL");

                    b.HasKey("Address");

                    b.ToTable("Systems", (string)null);
                });

            modelBuilder.Entity("CommanderVistedSystems", b =>
                {
                    b.HasOne("VoqooePlanner.DTOs.JournalCommanderDTO", null)
                        .WithMany()
                        .HasForeignKey("CommanderVisitsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VoqooePlanner.DTOs.VoqooeSystemDTO", null)
                        .WithMany()
                        .HasForeignKey("VoqooeSystemDTOAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
