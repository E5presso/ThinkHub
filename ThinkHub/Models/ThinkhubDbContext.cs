using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ThinkHub.Models
{
    public partial class ThinkhubDbContext : DbContext
    {
        private readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public virtual DbSet<Access> Access { get; set; }
        public virtual DbSet<Authority> Authority { get; set; }
        public virtual DbSet<Permission> Permission { get; set; }
        public virtual DbSet<Profile> Profile { get; set; }
        public virtual DbSet<Resource> Resource { get; set; }
        public virtual DbSet<ResourceMap> ResourceMap { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Shared> Shared { get; set; }
        public virtual DbSet<User> User { get; set; }

        public ThinkhubDbContext() { }
        public ThinkhubDbContext(DbContextOptions<ThinkhubDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseSqlServer(configuration.GetConnectionString("ThinkhubDb"));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
            modelBuilder.Entity<Access>(entity =>
            {
                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasColumnType("text");

                entity.HasOne(d => d.Shared)
                    .WithMany(p => p.Access)
                    .HasForeignKey(d => d.SharedId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Access__SharedId__4E88ABD4");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Access)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Access__UserId__4D94879B");
            });
            modelBuilder.Entity<Authority>(entity =>
            {
                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Authority)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Authority__RoleI__3E52440B");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Authority)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Authority__UserI__3D5E1FD2");
            });
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasOne(d => d.Shared)
                    .WithMany(p => p.Permission)
                    .HasForeignKey(d => d.SharedId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Permissio__Share__4AB81AF0");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Permission)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Permissio__UserI__49C3F6B7");
            });
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Birthday).HasColumnType("datetime");

                entity.Property(e => e.Image).HasMaxLength(1);

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Profile)
                    .HasForeignKey<Profile>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Profile__Id__38996AB5");
            });
            modelBuilder.Entity<Resource>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Target)
                    .IsRequired()
                    .HasColumnType("text");
            });
            modelBuilder.Entity<ResourceMap>(entity =>
            {
                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ResourceMap)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ResourceM__Resou__440B1D61");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.ResourceMap)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ResourceM__RoleI__4316F928");
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });
            modelBuilder.Entity<Shared>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasColumnType("text");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Shared)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Shared__UserId__46E78A0C");
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.HashSalt)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.RegistrationDate).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });
        }
    }
}