using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Data.Entity.Infrastructure.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using XuguClient;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class LibraryDbContext : DbContext
    {
        //public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
        //public LibraryDbContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseXG(
                "IP=127.0.0.1;DB=TESTLMSEFCore50;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=off;CHAR_SET=GBK;"
            );
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<StockAlert> StockAlerts { get; set; }
        public DbSet<Configuration> Configurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置主键和表名
            modelBuilder.Entity<User>().ToTable("Users").HasKey(u => u.UserId);

            modelBuilder.Entity<Book>().ToTable("Books")
                .HasKey(b => b.BookId);

            modelBuilder.Entity<Category>()
                .ToTable("Categories").HasKey(c => c.CategoryId);
                

            modelBuilder.Entity<BorrowRecord>()
                .ToTable("BorrowRecords").HasKey(br => br.BorrowRecordId);
                

            modelBuilder.Entity<Fine>().ToTable("Fines")
                .HasKey(f => f.FineId);

            modelBuilder.Entity<Log>().ToTable("Logs")
                .HasKey(l => l.LogId);

            modelBuilder.Entity<StockAlert>().ToTable("StockAlerts")
                .HasKey(sa => sa.StockAlertId);

            modelBuilder.Entity<Configuration>().ToTable("Configurations")
                .HasKey(c => c.ConfigurationId);

            // 配置一对多关系：User 和 BorrowRecord
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.User)
                .WithMany(u => u.BorrowRecords)
                .HasForeignKey(br => br.UserId);

            // 配置一对多关系：Book 和 BorrowRecord
            modelBuilder.Entity<BorrowRecord>()
                .HasOne(br => br.Book)
                .WithMany(b => b.BorrowRecords)
                .HasForeignKey(br => br.BookId);

            
            modelBuilder.Entity<Fine>()
                .HasOne(br => br.BorrowRecord)
                .WithMany(br => br.Fines)
                .HasForeignKey(i=>i.BorrowRecordId);

            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId });

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId);

            // 配置默认值和非空约束
            modelBuilder.Entity<User>()
                .Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Book>()
                .Property(b => b.Author)
                .HasMaxLength(255);

            modelBuilder.Entity<Book>()
                .Property(b => b.ISBN)/*.HasColumnType("varchar")*/
                .IsRequired()
                .HasMaxLength(13);

            //modelBuilder.Entity<Book>().Property(b => b.BookId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<BorrowRecord>()
                .Property(br => br.BorrowDate)
                .IsRequired();

            modelBuilder.Entity<BorrowRecord>()
                .Property(br => br.DueDate)
                .IsRequired();

            modelBuilder.Entity<Fine>()
                .Property(f => f.Amount)
                .IsRequired();

            modelBuilder.Entity<Log>()
                .Property(l => l.Action)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<StockAlert>()
                .Property(sa => sa.AlertDate)
                .IsRequired();

        }
        public static string GetEFConnectionString<T>(string database = null) where T : DbContext
        {
            XGConnectionStringBuilder sb = new XGConnectionStringBuilder();
            //这是新数据库的连接字符串，可以配置在Web.Config中然后读取，也可以像现在这样在这里动态拼接
            sb.ConnectionString = $"IP=127.0.0.1;DB={database ?? typeof(T).Name};User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=off;CHAR_SET=GBK";

            return sb.ToString();
        }
    }

}
