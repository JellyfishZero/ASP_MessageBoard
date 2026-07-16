using System.Security.Claims;
using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Services.DTOs;
using ASP_MessageBoard.Services.Interfaces;
using ASP_MessageBoard.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MessageBoard.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            // 避免重新進入註冊頁
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Posts");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            CancellationToken cancellationToken
        )
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var request = new RegisterRequest
                {
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Biography = model.Biography,
                    Password = model.Password,
                    CoverImage = model.CoverImage,
                };

                await _accountService.RegisterAsync(request, cancellationToken);

                TempData["SuccessMessage"] = "註冊成功，請登入。";

                return RedirectToAction(nameof(Login));
            }
            catch (DuplicatePhoneNumberException exception)
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), exception.Message);

                return View(model);
            }
            catch (ImageValidationException exception)
            {
                ModelState.AddModelError(nameof(model.CoverImage), exception.Message);

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Posts");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl,
            CancellationToken cancellationToken
        )
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var request = new LoginRequest
            {
                PhoneNumber = model.PhoneNumber,
                Password = model.Password,
            };

            var user = await _accountService.LoginAsync(request, cancellationToken);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "手機號碼或密碼錯誤。");

                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.MobilePhone, user.PhoneNumber),
                new(ClaimTypes.Email, user.Email),
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
            };

            if (model.RememberMe)
            {
                authenticationProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authenticationProperties
            );

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Posts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Posts");
        }
    }
}
