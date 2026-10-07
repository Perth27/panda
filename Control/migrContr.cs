using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;
using Pandora.Services;

namespace Pandora.Controllers
{
    public class MigratesController : Controller
    {
        private readonly IMigratesService _migratesService;

        public MigratesController(IMigratesService migratesService)
        {
            _migratesService = migratesService;
        }

        // GET: Migrates
        public async Task<IActionResult> Index()
        {
            return View(await _migratesService.GetMigratesAsync());
        }

        // GET: Migrates/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migrate = await _migratesService.GetMigrateDetailsAsync(id);
            if (migrate == null)
            {
                return NotFound();
            }

            return View(migrate);
        }

        // GET: Migrates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Migrates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,HLQ1,HLQ2,ManagementClass,CreatedYear,NoOfDatasets,Size")] Migrate migrate)
        {
            if (ModelState.IsValid)
            {
                await _migratesService.CreateMigrateAsync(migrate);
                return RedirectToAction(nameof(Index));
            }
            return View(migrate);
        }

        // GET: Migrates/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migrate = await _migratesService.GetMigrateForEditAsync(id);
            if (migrate == null)
            {
                return NotFound();
            }
            return View(migrate);
        }

        // POST: Migrates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Type,HLQ1,HLQ2,ManagementClass,CreatedYear,NoOfDatasets,Size")] Migrate migrate)
        {
            if (id != migrate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _migratesService.UpdateMigrateAsync(migrate);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_migratesService.MigrateExists(migrate.Id))
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
            return View(migrate);
        }

        // GET: Migrates/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var migrate = await _migratesService.GetMigrateForDeleteAsync(id);
            if (migrate == null)
            {
                return NotFound();
            }

            return View(migrate);
        }

        // POST: Migrates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _migratesService.DeleteMigrateAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}