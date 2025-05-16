using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagerSystem.Models.Auth
{
    /// <summary>
    /// 用户实体
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLogin { get; set; }
        
        // 导航属性
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    /// <summary>
    /// 角色实体
    /// </summary>
    public class Role
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        // 导航属性
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// 权限实体
    /// </summary>
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required, StringLength(200)]
        public string ResourceUri { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        // 权限类型：页面访问、数据访问等
        public PermissionType Type { get; set; } = PermissionType.Page;
        
        // 导航属性
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    /// <summary>
    /// 用户-角色关联表
    /// </summary>
    public class UserRole
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public int RoleId { get; set; }
        
        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }

    /// <summary>
    /// 角色-权限关联表
    /// </summary>
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        
        public int PermissionId { get; set; }
        
        // 导航属性
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
        
        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;
    }

    /// <summary>
    /// 权限类型枚举
    /// </summary>
    public enum PermissionType
    {
        Page,       // 页面访问权限
        Data,       // 数据访问权限
        Function,   // 功能操作权限
        Api         // API访问权限
    }
}