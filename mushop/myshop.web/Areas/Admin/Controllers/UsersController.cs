﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess;
using myshop.Utilities;
using System.Security.Claims;

namespace myshop.web.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var claimsIdentity=(ClaimsIdentity)User.Identity;
            var claim=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userid = claim.Value;
            return View(_context.ApplicationUsers.Where(x=>x.Id!=userid).ToList());
        }
        public IActionResult LockUnLock(string? id)
        {
            var user=_context.ApplicationUsers.FirstOrDefault(x=>x.Id==id);
            if (user == null)
            {
                return NotFound();
            }
            if(user.LockoutEnd==null || user.LockoutEnd < DateTime.Now)
            {
                user.LockoutEnd= DateTime.Now.AddYears(1);
            }
            else
            {
                user.LockoutEnd = DateTime.Now;
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Users", new { area = "Admin" });
        }
    }
}
