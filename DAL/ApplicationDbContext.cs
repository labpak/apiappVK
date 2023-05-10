using apiappVK.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.DAL
{
    public class ApplicationDbContext: DbContext //содержит все компоненты для работы с бд
    {
        public DbSet<User> users { get; set; }
        public DbSet<UserGroup> usergroup { get; set; }
        public DbSet<UserState> userstate { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options)
         {
            //Database.EnsureDeleted();
            Database.EnsureCreated();//создаст бд если ее нет
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=1234;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("users").HasKey(x => x.id);
                builder.Property(x => x.id).ValueGeneratedOnAdd();
                builder.Property(x => x.login).IsRequired();
                builder.Property(x => x.password).IsRequired();
                builder.Property(x => x.created_date);
                builder.Property(x => x.user_group_id).ValueGeneratedOnAdd();
                builder.Property(x => x.user_state_id).ValueGeneratedOnAdd();
            });
        }
    }   
}
