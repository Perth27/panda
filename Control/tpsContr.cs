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
    public class TapeDSController : Controller
    {
        private readonly ITapeDSService _tapeDSService;

        public TapeDSController(ITapeDSService tapeDSService)
        {
            _tapeDSService = tapeDSService;
        }

        // GET: TapeDS
        public async Task<IActionResult> Index()
        {
            return View(await _tapeDSService.GetTapeDSAsync());
        }

        // GET: TapeDS/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tapeDS = await _tapeDSService.GetTapeDSDetailsAsync(id);
            if (tapeDS == null)
            {
                return NotFound();
            }

            return View(tapeDS);
        }

        // GET: TapeDS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TapeDS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Type,HLQ1,HLQ2,ManagementClass,CreatedYear,NoOfDatasets,Size")] TapeDS tapeDS)
        {
            if (ModelState.IsValid)
            {
                await _tapeDSService.CreateTapeDSAsync(tapeDS);
                return RedirectToAction(nameof(Index));
            }
            return View(tapeDS);
        }

        // GET: TapeDS/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tapeDS = await _tapeDSService.GetTapeDSForEditAsync(id);
            if (tapeDS == null)
            {
                return NotFound();
            }
            return View(tapeDS);
        }

        // POST: TapeDS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Type,HLQ1,HLQ2,ManagementClass,CreatedYear,NoOfDatasets,Size")] TapeDS tapeDS)
        {
            if (id != tapeDS.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _tapeDSService.UpdateTapeDSAsync(tapeDS);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_tapeDSService.TapeDSExists(tapeDS.Id))
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
            return View(tapeDS);
        }

        // GET: TapeDS/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tapeDS = await _tapeDSService.GetTapeDSForDeleteAsync(id);
            if (tapeDS == null)
            {
                return NotFound();
            }

            return View(tapeDS);
        }

        // POST: TapeDS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _tapeDSService.DeleteTapeDSAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}