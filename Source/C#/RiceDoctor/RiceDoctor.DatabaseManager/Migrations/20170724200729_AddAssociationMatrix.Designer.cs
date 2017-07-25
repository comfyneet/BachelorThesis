using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using RiceDoctor.DatabaseManager;

namespace RiceDoctor.DatabaseManager.Migrations
{
    [DbContext(typeof(RiceContext))]
    [Migration("20170724200729_AddAssociationMatrix")]
    partial class AddAssociationMatrix
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("RiceDoctor.DatabaseManager.Article", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<string>("Image");

                    b.Property<DateTime>("RetrievedDate");

                    b.Property<string>("Title");

                    b.Property<string>("Url");

                    b.Property<int>("WebsiteId");

                    b.HasKey("Id");

                    b.HasIndex("WebsiteId");

                    b.ToTable("Articles");
                });

            modelBuilder.Entity("RiceDoctor.DatabaseManager.Association", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AssociationScore");

                    b.Property<string>("Term1");

                    b.Property<string>("Term2");

                    b.HasKey("Id");

                    b.ToTable("Associations");
                });

            modelBuilder.Entity("RiceDoctor.DatabaseManager.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ArticleXPath");

                    b.Property<string>("ContentXPath");

                    b.Property<string>("TitleXPath");

                    b.Property<string>("Url");

                    b.Property<int>("WebsiteId");

                    b.HasKey("Id");

                    b.HasIndex("WebsiteId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("RiceDoctor.DatabaseManager.Website", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Websites");
                });

            modelBuilder.Entity("RiceDoctor.DatabaseManager.Article", b =>
                {
                    b.HasOne("RiceDoctor.DatabaseManager.Website", "Website")
                        .WithMany("Articles")
                        .HasForeignKey("WebsiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("RiceDoctor.DatabaseManager.Category", b =>
                {
                    b.HasOne("RiceDoctor.DatabaseManager.Website", "Website")
                        .WithMany("Categories")
                        .HasForeignKey("WebsiteId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
