using ManagerSystem.Data;
using ManagerSystem.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ManagerSystem.Services.Auth
{
    /// <summary>
    /// 权限服务接口
    /// </summary>
    public interface IAuthService
    {
        // 用户认证相关
        Task<User?> AuthenticateAsync(string username, string password);
        Task<bool> RegisterUserAsync(User user, string password, int[] roleIds);
        Task<AuthResult> RegisterAsync(string username, string password);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        
        // 用户管理相关
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        
        // 角色管理相关
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task<bool> CreateRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(int id);
        Task<List<Role>> GetUserRolesAsync(int userId);
        Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);
        
        // 权限管理相关
        Task<List<Permission>> GetAllPermissionsAsync();
        Task<Permission?> GetPermissionByIdAsync(int id);
        Task<bool> CreatePermissionAsync(Permission permission);
        Task<bool> UpdatePermissionAsync(Permission permission);
        Task<bool> DeletePermissionAsync(int id);
        Task<List<Permission>> GetRolePermissionsAsync(int roleId);
        Task<List<Permission>> GetUserPermissionsAsync(int userId);
        
        // 权限验证相关
        Task<bool> HasPermissionAsync(int userId, string resourceUri, PermissionType type = PermissionType.Page);
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user);
    }

    /// <summary>
    /// 权限服务实现
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<AuthDbContext> _dbFactory;

        public AuthService(IDbContextFactory<AuthDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #region 用户认证相关

        /// <summary>
        /// 用户注册
        /// </summary>
        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext();
                // 检查用户名是否已存在
                if (await context.Users.AnyAsync(u => u.Username == username))
                {
                    return new AuthResult { Success = false, ErrorMessage = "用户名已存在" };
                }

                // 创建新用户
                var user = new User
                {
                    Username = username,
                    PasswordHash = CreatePasswordHash(password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // 添加用户
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                return new AuthResult { Success = true };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, ErrorMessage = "注册过程中发生错误" };
            }
        }

        /// <summary>
        /// 用户认证
        /// </summary>
        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return null;

            // 验证密码
            if (!VerifyPasswordHash(password, user.PasswordHash))
                return null;

            // 更新最后登录时间
            user.LastLogin = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// 注册用户
        /// </summary>
        public async Task<bool> RegisterUserAsync(User user, string password, int[] roleIds)
        {
            using var context = _dbFactory.CreateDbContext();
            // 检查用户名是否已存在
            if (await context.Users.AnyAsync(u => u.Username == user.Username))
                return false;

            // 创建密码哈希
            user.PasswordHash = CreatePasswordHash(password);

            // 添加用户
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // 分配角色
            foreach (var roleId in roleIds.Distinct())
            {
                await context.UserRoles.AddAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // 验证旧密码
            if (!VerifyPasswordHash(oldPassword, user.PasswordHash))
                return false;

            // 更新密码
            user.PasswordHash = CreatePasswordHash(newPassword);
            await context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region 用户管理相关

        /// <summary>
        /// 获取所有用户
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            using var context = _dbFactory.CreateDbContext();
            var existingUser = await context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return false;

            // 更新用户信息，但不更新密码
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.IsActive = user.IsActive;

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.Users.FindAsync(id);
            if (user == null)
                return false;

            // 删除用户角色关联
            var userRoles = await context.UserRoles.Where(ur => ur.UserId == id).ToListAsync();
            context.UserRoles.RemoveRange(userRoles);

            // 删除用户
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region 角色管理相关

        /// <summary>
        /// 获取所有角色
        /// </summary>
        public async Task<List<Role>> GetAllRolesAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Roles.ToListAsync();
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        public async Task<bool> CreateRoleAsync(Role role)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        public async Task<bool> UpdateRoleAsync(Role role)
        {
            using var context = _dbFactory.CreateDbContext();
            var existingRole = await context.Roles.FindAsync(role.Id);
            if (existingRole == null)
                return false;

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        public async Task<bool> DeleteRoleAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var role = await context.Roles.FindAsync(id);
            if (role == null)
                return false;

            // 删除角色权限关联
            var rolePermissions = await context.RolePermissions.Where(rp => rp.RoleId == id).ToListAsync();
            context.RolePermissions.RemoveRange(rolePermissions);

            // 删除用户角色关联
            var userRoles = await context.UserRoles.Where(ur => ur.RoleId == id).ToListAsync();
            context.UserRoles.RemoveRange(userRoles);

            // 删除角色
            context.Roles.Remove(role);
            await context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        public async Task<List<Role>> GetUserRolesAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        /// <summary>
        /// 更新角色权限
        /// </summary>
        public async Task<bool> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            using var context = _dbFactory.CreateDbContext();
            // 检查角色是否存在
            var role = await context.Roles.FindAsync(roleId);
            if (role == null)
                return false;

            // 获取当前角色的所有权限关联
            var currentRolePermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            // 删除不在新权限列表中的权限关联
            var toRemove = currentRolePermissions
                .Where(rp => !permissionIds.Contains(rp.PermissionId))
                .ToList();
            context.RolePermissions.RemoveRange(toRemove);

            // 添加新的权限关联
            var currentPermissionIds = currentRolePermissions.Select(rp => rp.PermissionId).ToList();
            var toAdd = permissionIds
                .Where(pid => !currentPermissionIds.Contains(pid))
                .Select(pid => new RolePermission { RoleId = roleId, PermissionId = pid })
                .ToList();
            await context.RolePermissions.AddRangeAsync(toAdd);

            await context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region 权限管理相关

        /// <summary>
        /// 获取所有权限
        /// </summary>
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Permissions.ToListAsync();
        }

        /// <summary>
        /// 根据ID获取权限
        /// </summary>
        public async Task<Permission?> GetPermissionByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Permissions.FindAsync(id);
        }

        /// <summary>
        /// 创建权限
        /// </summary>
        public async Task<bool> CreatePermissionAsync(Permission permission)
        {
            using var context = _dbFactory.CreateDbContext();
            await context.Permissions.AddAsync(permission);
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 更新权限
        /// </summary>
        public async Task<bool> UpdatePermissionAsync(Permission permission)
        {
            using var context = _dbFactory.CreateDbContext();
            var existingPermission = await context.Permissions.FindAsync(permission.Id);
            if (existingPermission == null)
                return false;

            existingPermission.Name = permission.Name;
            existingPermission.ResourceUri = permission.ResourceUri;
            existingPermission.Description = permission.Description;
            existingPermission.Type = permission.Type;

            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        public async Task<bool> DeletePermissionAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var permission = await context.Permissions.FindAsync(id);
            if (permission == null)
                return false;

            // 删除角色权限关联
            var rolePermissions = await context.RolePermissions.Where(rp => rp.PermissionId == id).ToListAsync();
            context.RolePermissions.RemoveRange(rolePermissions);

            // 删除权限
            context.Permissions.Remove(permission);
            await context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// 获取角色的权限
        /// </summary>
        public async Task<List<Permission>> GetRolePermissionsAsync(int roleId)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        /// <summary>
        /// 获取用户的所有权限
        /// </summary>
        public async Task<List<Permission>> GetUserPermissionsAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext();
            // 获取用户的所有角色ID
            var roleIds = await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // 获取这些角色的所有权限
            return await context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();
        }

        #endregion

        #region 权限验证相关

        /// <summary>
        /// 检查用户是否拥有指定资源的权限
        /// </summary>
        public async Task<bool> HasPermissionAsync(int userId, string resourceUri, PermissionType type = PermissionType.Page)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Any(p => 
                p.ResourceUri.Equals(resourceUri, StringComparison.OrdinalIgnoreCase) && 
                p.Type == type);
        }

        /// <summary>
        /// 创建用户的ClaimsPrincipal
        /// </summary>
        public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            var roles = await GetUserRolesAsync(user.Id);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var permissions = await GetUserPermissionsAsync(user.Id);
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission.ResourceUri));
            }

            var identity = new ClaimsIdentity(claims, "ManagerSystemAuth");
            return new ClaimsPrincipal(identity);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建密码哈希
        /// </summary>
        private string CreatePasswordHash(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = Convert.ToBase64String(hashedBytes);
            return hash == storedHash;
        }

        #endregion
    }
}