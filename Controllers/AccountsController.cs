using MDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Web.Mvc;

namespace MDB.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AppDBEntities DB = new AppDBEntities();

        [HttpPost]
        public JsonResult EmailExist(string email)
        {
            return Json(DB.EmailExist(email));
        }

        #region Login and Logout
        public ActionResult Login(string message)
        {
            ViewBag.Message = message;
            return View(new LoginCredential());
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Login(LoginCredential loginCredential)
        {
            if (ModelState.IsValid)
            {
                if (DB.EmailBlocked(loginCredential.Email))
                {
                    ModelState.AddModelError("Email", "Ce compte est bloqué.");
                    return View(loginCredential);
                }
                if (!DB.EmailVerified(loginCredential.Email))
                {
                    ModelState.AddModelError("Email", "Ce courriel n'est pas vérifié.");
                    return View(loginCredential);
                }
                User user = DB.GetUser(loginCredential);
                if (user == null)
                {
                    ModelState.AddModelError("Password", "Mot de passe incorrecte.");
                    return View(loginCredential);
                }
                if (OnlineUsers.IsOnLine(user.Id))
                {
                    ModelState.AddModelError("Email", "Cet usager est déjà connecté.");
                    return View(loginCredential);
                }
                OnlineUsers.AddSessionUser(user.Id);
                Session["currentLoginId"] = DB.AddLogin(user.Id).Id;
                return RedirectToAction("Index", "Movies");
            }
            return View(loginCredential);
        }
        public ActionResult Logout()
        {
            if (Session["currentLoginId"] != null)
                DB.UpdateLogout((int)Session["currentLoginId"]);
            OnlineUsers.RemoveSessionUser();
            return RedirectToAction("Login");
        }
        #endregion

       
    }

}