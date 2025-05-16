using ManagerSystem.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManagerSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> LoginPost([FromForm] LoginInput input)
        {
            if (!ModelState.IsValid)
                return View("Login", input);

            var user = await _authService.AuthenticateAsync(input.Username, input.Password);
            if (user == null)
            {
                return Redirect("/auth/login?error=用户名或密码错误");
            }

            var principal = await _authService.CreateClaimsPrincipalAsync(user);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            return Redirect("/");
        }

        public class LoginInput
        {
            [Required]
            public string Username { get; set; } = "";
            [Required]
            public string Password { get; set; } = "";
        }
    }
} 