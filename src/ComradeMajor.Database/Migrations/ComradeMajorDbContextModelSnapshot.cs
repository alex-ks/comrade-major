﻿// <auto-generated />
using ComradeMajor.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ComradeMajor.Database.Migrations
{
    [DbContext(typeof(ComradeMajorDbContext))]
    partial class ComradeMajorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.5");

            modelBuilder.Entity("ComradeMajor.Database.User", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ComradeMajor.Database.UserAction", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActionType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Activity")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Timestamp")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserActions");
                });

            modelBuilder.Entity("ComradeMajor.Database.UserAction", b =>
                {
                    b.HasOne("ComradeMajor.Database.User", "User")
                        .WithMany("Actions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ComradeMajor.Database.User", b =>
                {
                    b.Navigation("Actions");
                });
#pragma warning restore 612, 618
        }
    }
}
