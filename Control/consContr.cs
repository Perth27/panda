using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Pandora.Models;
using Pandora.Services;
using ReflectionIT.Mvc.Paging;

namespace Pandora.Controllers
{
    [Authorize]
    public class ConsolidatedBackupsController : Controller
    {
        private readonly IConsolidatedBackupsService _service;

        public ConsolidatedBackupsController(IConsolidatedBackupsService service)
        {
            _service = service;
        }

        // GET: ConsolidatedBackups
        public async Task<IActionResult> Index(string filter, string searchOption, string filterRule, string sortExpression = "CreatedYear", int pageIndex = 1)
        {
            ViewBag.filterView = filter;
            ViewBag.searchOptionView = searchOption;
            ViewBag.FilterRule = filterRule;

            var backupRecords = _service.GetAllAsNoTracking();
            backupRecords = _service.Filter(backupRecords, filter, filterRule, searchOption);

            var metrics = _service.GetIndexMetrics(backupRecords);
            ViewBag.DatasetEntry = metrics.DatasetEntry;
            ViewBag.TotalDatasets = metrics.TotalDatasets;
            ViewBag.PercentageBackedup = metrics.PercentageBackedup;
            ViewBag.PercentageValidated = metrics.PercentageValidated;

            var model = await PagingList.CreateAsync(backupRecords, 10, pageIndex, sortExpression, "CreatedYear");
            model.Action = "Index";
            model.RouteValue = new RouteValueDictionary { { "filter", filter }, { "searchOption", searchOption }, { "FilterRule", filterRule } };
            
            return View(model);
        }

        // GET: ConsolidatedBackups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _service.GetRecordByIdAsync(id);
            if (record == null) return NotFound();

            return View(record);
        }

        // GET: ConsolidatedBackups/Edit/5
        public async Task<IActionResult> Edit(int? id, string filter, string searchOption, string returnUrl)
        {
            if (id == null) return NotFound();

            var record = await _service.FindRecordByIdAsync(id);
            if (record == null) return NotFound();
            
            return View(record);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConsolidatedValidationsData data, string filter, string searchOption, string returnUrl)
        {
            if (id != data.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.ProcessEditAsync(data, User.Identity.Name);
                    TempData["success"] = "Item successfully updated!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index), new { filter = filter, searchOption = searchOption });
            }
            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MultipartBodyLengthLimit = 100_000_000)]
        [RequestFormLimits(ValueLengthLimit = 100_000_000)]
        public async Task<IActionResult> BulkValidation(IFormFile upload, string filter, string searchOption)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.Length > 0)
                {
                    var result = await _service.ProcessBulkValidationAsync(upload, User.Identity.Name, Directory.GetCurrentDirectory());
                    
                    if (result.IsSuccess)
                    {
                        TempData["success"] = $"{result.ValidatedCount} items validated successfully.";
                    }
                    else
                    {
                        TempData["error"] = result.ErrorMessage;
                    }
                }
                else
                {
                    TempData["error"] = "Error! No file uploaded.";
                }
                return RedirectToAction(nameof(Index), new { filter = filter, searchOption = searchOption });
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Unclaim(int? id, string returnUrl)
        {
            if (id == null) return BadRequest();

            var result = await _service.ProcessUnclaimAsync(id.Value, User.Identity.Name);
            
            if (result.ErrorMessage == "Not Found") return NotFound();

            if (result.IsSuccess)
            {
                TempData["success"] = "Item has been unclaimedi!";
            }
            else
            {
                TempData["error"] = result.ErrorMessage;
            }

            return RedirectToLocal(returnUrl);
        }

        public async Task<IActionResult> Invalidate(int? id, string returnUrl)
        {
            if (id == null) return BadRequest();

            await _service.ProcessInvalidateAsync(id.Value, User.Identity.Name);
            TempData["success"] = "Item has been invalidated!";

            return RedirectToLocal(returnUrl);
        }

        public async Task<IActionResult> MyClaimed(string filter, string searchOption, string filterRule, string sortExpression = "CreatedYear", int pageIndex = 1)
        {
            var backupRecords = _service.GetAll().Where(a => a.LastUpdatedBy == User.Identity.Name && a.Status.ToLower() == "under investigation").AsNoTracking();
            backupRecords = _service.Filter(backupRecords, filter, filterRule, searchOption);

            ViewBag.DatasetEntry = backupRecords.Count();

            var model = await PagingList.CreateAsync(backupRecords, 10, pageIndex, sortExpression, "CreatedYear");
            model.Action = "MyClaimed";
            model.RouteValue = new RouteValueDictionary { { "filter", filter }, { "searchOption", searchOption }, { "FilterRule", filterRule } };
            
            ViewBag.filterView = filter;
            ViewBag.searchOptionView = searchOption;
            ViewBag.FilterRule = filterRule;

            return View(model);
        }

        public IActionResult Export(string filter, string filterRule, string searchOption)
        {
            try
            {
                byte[] fileBytes = _service.GenerateExportCsv(filter, filterRule, searchOption);
                string fileName = $"Consolidated Backup Export Data {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}.csv";
                return File(fileBytes, "text/csv", fileName);
            }
            catch
            {
                throw new Exception("File could not be generated");
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }
    }
}