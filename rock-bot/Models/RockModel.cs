namespace rock_bot.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class RockModel : DbContext
    {
        public RockModel()
            : base("name=RockModel")
        {
        }

        public virtual DbSet<meme> memes { get; set; }
        public virtual DbSet<permission> permissions { get; set; }
        public virtual DbSet<restricted_keywords> restricted_keywords { get; set; }
        public virtual DbSet<user> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<meme>()
                .Property(e => e.keyword)
                .IsUnicode(false);

            modelBuilder.Entity<meme>()
                .Property(e => e.response)
                .IsUnicode(false);

            modelBuilder.Entity<permission>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<permission>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<permission>()
                .HasMany(e => e.users)
                .WithMany(e => e.permissions)
                .Map(m => m.ToTable("user_permissions", "rock-bot").MapLeftKey("permissionid").MapRightKey("userid"));

            modelBuilder.Entity<restricted_keywords>()
                .Property(e => e.keyword)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.details)
                .IsUnicode(false);
        }
    }
}
