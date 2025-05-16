using ManagerSystem.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace ManagerSystem.Data
{
    /// <summary>
    /// 权限系统数据库上下文
    /// </summary>
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置关系
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // 添加种子数据
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 添加默认管理员角色
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "管理员", Description = "系统管理员，拥有所有权限" },
                new Role { Id = 2, Name = "普通用户", Description = "普通用户，拥有基本权限" }
            );

            // 添加默认权限
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "首页访问", ResourceUri = "/", Description = "访问系统首页", Type = PermissionType.Page },
                new Permission { Id = 2, Name = "用户管理", ResourceUri = "/users", Description = "管理系统用户", Type = PermissionType.Page },
                new Permission { Id = 3, Name = "角色管理", ResourceUri = "/admin/roles", Description = "管理系统角色", Type = PermissionType.Page },
                new Permission { Id = 4, Name = "权限管理", ResourceUri = "/permissions", Description = "管理系统权限", Type = PermissionType.Page }
            );

            // 为管理员角色分配所有权限
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = 1, RoleId = 1, PermissionId = 1 },
                new RolePermission { Id = 2, RoleId = 1, PermissionId = 2 },
                new RolePermission { Id = 3, RoleId = 1, PermissionId = 3 },
                new RolePermission { Id = 4, RoleId = 1, PermissionId = 4 }
            );

            // 为普通用户分配首页访问权限
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = 5, RoleId = 1, PermissionId = 3 }
            );
        }
    }
}