using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pandora.Models;
using Pandora.Services;

namespace Pandora.Controllers
{
    public class StorageManagementController : Controller
    {
        private readonly IStorageManagementService _storageManagementService;

        public StorageManagementController(IStorageManagementService storageManagementService)
        {
            _storageManagementService = storageManagementService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(string managementClass)
        {
            if (managementClass == null)
            {
                TempData["error"] = "The Management Class could not be found.";
                return NotFound();
            }

            var storageManagent = await _storageManagementService.GetStorageManagementDetailsAsync(managementClass);
            
            if (storageManagent == null)
            {
                TempData["error"] = "The Management Class could not be found.";
                return NotFound();
            }

            return View(storageManagent);
        }
    }
}