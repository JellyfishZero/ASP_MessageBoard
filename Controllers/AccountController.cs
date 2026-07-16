using ASP_MessageBoard.Services.Interfaces;
using ASP_MessageBoard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ASP_MessageBoard.Common.Exceptions;
using ASP_MessageBoard.Services.Dtos;

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
                    Password = model.Password
                };

                await _accountService.RegisterAsync(request, cancellationToken);

                TempData["SuccessMessage"] = "註冊成功，請登入。";

                // 登入功能尚未完成，暫時回到註冊頁。
                // return RedirectToAction(nameof(Login));
                return RedirectToAction(nameof(Register));
            }
            catch (DuplicatePhoneNumberException exception)
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), exception.Message);

                return View(model);
            }
        }
    }
}
