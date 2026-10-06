using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Pandora.ViewModels;
using Novell.Directory.Ldap;
using Pandora.Services;

namespace Pandora.Controllers
{
    //[Authorize(Roles = "Admin,Developer")]
    public class UsersController : Controller
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _usersService.GetUsersAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _usersService.GetUserDetailsAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var model = _usersService.GetUserCreateVM();
            return View(model);
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM user)
        {
            user = _usersService.GetRepopulatedUserCreateVM(user);

            if (ModelState.IsValid)
            {
                var result = await _usersService.CreateUserAsync(user);

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    TempData["error"] = result.ErrorMessage;
                    return View(user);
                }

                if (result.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                
                if (result.NewUser == null)
                {
                    return View(user);
                }
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _usersService.GetUserForEditAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserVM user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _usersService.UpdateUserAsync(user);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_usersService.UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _usersService.GetUserForDeleteAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _usersService.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}