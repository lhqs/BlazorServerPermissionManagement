using ManagerSystem.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ManagerSystem.Services.Auth
{
    /// <summary>
    /// 认证状态提供者，用于管理用户的认证状态（基于 Cookie）
    /// </summary>
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthStateProvider> _logger;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public AuthStateProvider(IAuthService authService, IHttpContextAccessor httpContextAccessor, ILogger<AuthStateProvider> logger)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前认证状态
        /// </summary>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User ?? _anonymous;
            return Task.FromResult(new AuthenticationState(user));
        }

        /// <summary>
        /// 用户登录（写入 Cookie）
        /// </summary>
        public async Task LoginAsync(User user)
        {
            var principal = await _authService.CreateClaimsPrincipalAsync(user);
            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        /// <summary>
        /// 用户登出（清除 Cookie）
        /// </summary>
        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}