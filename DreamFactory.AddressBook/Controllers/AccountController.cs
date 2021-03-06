﻿namespace DreamFactory.AddressBook.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using DreamFactory.AddressBook.Models;
    using DreamFactory.Api;
    using DreamFactory.Model.User;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;

    [Authorize]
    [HandleError]
    public class AccountController : Controller
    {
        private readonly IUserApi userApi;
        private readonly ISystemAdminApi adminApi;

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }


        public AccountController(IUserApi userApi, ISystemAdminApi adminApi)
        {
            this.userApi = userApi;
            this.adminApi = adminApi;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Session session = new Session();

            try
            {
                session = await userApi.LoginAsync(model.Email, model.Password);
            }
            catch (DreamFactoryException)
            {
                try
                {
                    session = await adminApi.LoginAdminAsync(model.Email, model.Password);
                }
                catch
                {; }
            }

            if (string.IsNullOrEmpty(session.SessionId))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            SignIn(session, model.RememberMe);
            return RedirectToLocal(returnUrl);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Register register = new Register
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Name = model.Name,
                    NewPassword = model.Password
                };

                bool result;
                try
                {
                    result = await userApi.RegisterAsync(register);
                }
                catch (DreamFactoryException)
                {
                    result = false;
                }

                if (result)
                {
                    Session session = new Session();
                    try
                    {
                        session = await userApi.LoginAsync(model.Email, model.Password);
                    }
                    catch (DreamFactoryException) {;}

                    if (string.IsNullOrEmpty(session.SessionId))
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    SignIn(session, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "There has been an error registering your account.");
                }
            }
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            ClaimsIdentity identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;

            if (claims.Any(x => x.Type == ClaimTypes.Role && x.Value == DreamFactoryContext.Roles.SysAdmin))
            {
                await adminApi.LogoutAdminAsync();
            }
            else
            {
                await userApi.LogoutAsync();
            }

            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        #region Helpers

        private void SignIn(Session session, bool rememberMe)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, session.Name),
                new Claim(ClaimTypes.NameIdentifier, session.Id),
            };

            if (session.IsSysAdmin ?? false)
            {
                claims.Add(new Claim(ClaimTypes.Role, DreamFactoryContext.Roles.SysAdmin));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = rememberMe }, identity);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("List", "ContactGroup");
        }

        #endregion
    }
}