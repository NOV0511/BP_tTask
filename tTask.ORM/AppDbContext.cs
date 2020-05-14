using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using tTask.ORM.DTO;

namespace tTask.ORM
{
    public partial class AppDbContext : IdentityDbContext<User, Role, int>
    {
        private readonly HttpContext _httpContext;
        private IConfiguration Configuration { get; set; }


        public AppDbContext(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpContext = httpContextAccessor.HttpContext;
            Configuration = config;
        }


        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config, IHttpContextAccessor httpContextAccessor = null)
            : base(options)
        {
            _httpContext = httpContextAccessor?.HttpContext;
            Configuration = config;
        }

        public virtual DbSet<GlobalUser> GlobalUser { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Service> Service { get; set; }
        public virtual DbSet<ServiceOrder> ServiceOrder { get; set; }
        public virtual DbSet<Task> Task { get; set; }
        public virtual DbSet<TaskUserComment> TaskUserComment { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserProject> UserProject { get; set; }
        public virtual DbSet<UserSettings> UserSettings { get; set; }
        public virtual DbSet<UserNotification> UserNotification { get; set; }
        public virtual DbSet<UserTask> UserTask { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string domain = _httpContext.Items["domain"] as string;
                var connectionString = Configuration.GetConnectionString("TenantConnection").Replace(":domainPass", Domain.GetDomainHash(domain)).Replace(":domain", domain);
                optionsBuilder.UseSqlServer(connectionString);
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GlobalUser>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.IdPayment)
                    .HasName("payment_pk");

                entity.Property(e => e.IdPayment).ValueGeneratedNever();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.IdProject)
                    .HasName("project_pk");

                entity.Property(e => e.IdProject).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);


                entity.Property(e => e.Description)
                    .HasMaxLength(300)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("Role");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.IdService)
                    .HasName("service_pk");

                entity.Property(e => e.IdService).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("numeric(28, 0)");
            });

            modelBuilder.Entity<ServiceOrder>(entity =>
            {
                entity.HasKey(e => new { e.IdTenant, e.OrderDate, e.IdService })
                    .HasName("serviceorder_pk");

                entity.HasIndex(e => e.IdPayment)
                    .HasName("serviceorder__idx")
                    .IsUnique();


                entity.HasOne(d => d.IdPaymentNavigation)
                    .WithOne(p => p.ServiceOrder)
                    .HasForeignKey<ServiceOrder>(d => d.IdPayment)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("serviceorder_payment_fk");

                entity.HasOne(d => d.IdServiceNavigation)
                    .WithMany(p => p.ServiceOrder)
                    .HasForeignKey(d => d.IdService)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("serviceorder_service_fk");

                entity.HasOne(d => d.IdTenantNavigation)
                    .WithMany(p => p.ServiceOrder)
                    .HasForeignKey(d => d.IdTenant)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("serviceorder_tenant_fk");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasKey(e => e.IdTask)
                    .HasName("task_pk");

                entity.Property(e => e.IdTask).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasIndex(b => b.Completed);

                entity.HasOne(d => d.IdProjectNavigation)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.IdProject)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("task_project_fk");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("task_user_fk");
            });

            modelBuilder.Entity<TaskUserComment>(entity =>
            {
                entity.HasKey(e => e.IdComment)
                    .HasName("taskusercomment_pk");

                entity.Property(e => e.IdComment).ValueGeneratedNever();

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);


                entity.HasOne(d => d.IdTaskNavigation)
                    .WithMany(p => p.TaskUserComment)
                    .HasForeignKey(d => d.IdTask)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("taskusercomment_task_fk");



                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.TaskUserComment)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("taskusercomment_user_fk");
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.IdTenant)
                    .HasName("tenant_pk");

                entity.Property(e => e.IdTenant).ValueGeneratedNever();

                entity.Property(e => e.Domain)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.ToTable("User");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);


                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(13)
                    .IsUnicode(false);

                entity.Property(e => e.Photopath)
                    .HasMaxLength(240)
                    .IsUnicode(false);

                entity.Property(e => e.Surname)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);


                entity.HasOne(d => d.IdTenantNavigation)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.IdTenant)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_tenant_fk");
            });

            modelBuilder.Entity<UserProject>(entity =>
            {
                entity.HasKey(e => new { e.IdUser, e.IdProject })
                    .HasName("userproject_pk");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.IdProjectNavigation)
                    .WithMany(p => p.UserProject)
                    .HasForeignKey(d => d.IdProject)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("userproject_project_fk");

                entity.HasOne(d => d.IdRoleNavigation)
                    .WithMany(p => p.UserProject)
                    .HasForeignKey(d => d.IdRole)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("userproject_role_fk");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.UserProject)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("userproject_user_fk");
            });

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.IdUser)
                    .HasName("usersettings_pk");

                entity.Property(e => e.IdUser).ValueGeneratedNever();

                entity.Property(e => e.Coloring)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();


                entity.Property(e => e.Notifications )
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();


                entity.Property(e => e.CustomizeView)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.IdUserNavigation)
                    .WithOne(p => p.UserSettings)
                    .HasForeignKey<UserSettings>(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usersettings_user_fk");
            });

            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(e => e.IdNotification)
                    .HasName("usernotification_pk");

                entity.Property(e => e.IdNotification).ValueGeneratedNever();

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);


                entity.Property(e => e.Read)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.UserNotification)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_usernotification_fk");
            });

            modelBuilder.Entity<UserTask>(entity =>
            {
                entity.HasKey(e => new { e.IdUser, e.IdTask })
                    .HasName("usertask_pk");

                entity.Ignore(u => u.IdTaskNavigation);

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.UserTask)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usertask_user_fk");
            });



            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
