﻿namespace NetCoreTemplate.Web.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using NetCoreTemplate.Authentication;
    using NetCoreTemplate.Authentication.Exceptions;
    using NetCoreTemplate.SharedKernel.Interfaces.Managers;
    using NetCoreTemplate.SharedKernel.ServiceContainer;
    using NetCoreTemplate.SharedKernel.Validation;
    using NetCoreTemplate.ViewModels.Controllers.Admin;
    using NetCoreTemplate.Web.Controllers.Base;
    using NetCoreTemplate.Web.Extensions;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using NetCoreTemplate.DAL.Models.General;
    using NetCoreTemplate.Providers.Interfaces;
    using NetCoreTemplate.SharedKernel.Extensions;

    public class AdminController : BaseController
    {
        private readonly IAuthenticationClient authentication;
        private readonly ITranslationManager translationManager;
        private readonly IBaseProvider<Language> languageProvider;

        public AdminController(IServiceContainer serviceContainer)
            : base(serviceContainer)
        {
            this.authentication = serviceContainer.GetService<IAuthenticationClient>();
            this.translationManager = serviceContainer.GetService<ITranslationManager>();
            this.languageProvider = serviceContainer.GetService<IBaseProvider<Language>>();
        }

        [AllowAnonymous]
        [HttpGet("signin")]
        public IActionResult SignIn()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public IActionResult SignIn(SignInViewModel viewModel)
        {
            var result = ProcessViewModel(
                viewModel,
                async vm =>
                {
                    var validationResult = new ValidationResult<SignInViewModel>();

                    try
                    {
                        var principal = authentication.SignIn(vm.Username, vm.Password);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    }
                    catch (DeactivatedException)
                    {
                        validationResult.AddError(
                            x => x.Username,
                            translationManager.GetTranslationLabel(LanguageId, "Dashboard:SignIn:Deactivated"));

                        ModelState.AddValidationResult(validationResult);

                        throw;
                    }
                    catch (Exception)
                    {
                        validationResult.AddError(
                            x => x.Username,
                            translationManager.GetTranslationLabel(LanguageId, "Dashboard:SignIn:WrongLogin"));

                        validationResult.AddError(
                            x => x.Password,
                            translationManager.GetTranslationLabel(LanguageId, "Dashboard:SignIn:WrongLogin"));

                        ModelState.AddValidationResult(validationResult);

                        throw;
                    }
                },
                () => RedirectToAction("Index", "Dashboard"),
                () =>
                {
                    var reloader = GetReloader<SignInViewModel>();
                    return View("SignIn", reloader.Reload(viewModel));
                });

            return result.Result;
        }

        [HttpGet("signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            foreach (var cookie in Request.Cookies)
            {
                Response.Cookies.Delete(cookie.Key);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        [HttpGet("language/{languageCode}/{returnPath}")]
        public IActionResult Language(string languageCode, string returnPath)
        {
            var language = languageProvider
                .GetEntity(x => x.Code.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

            if (language.IsNullOrDefault())
            {
                language = new Language { Id = 1, Code = "NL" };
            }

            Response.Cookies.Append("language", language.Id.ToString());

            var url = WebUtility.UrlDecode(returnPath);

            return Redirect(url);
        }
    }
}
