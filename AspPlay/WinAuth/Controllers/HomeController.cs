using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WinAuth.Models;

namespace WinAuth.Controllers
{
    public class HomeController : Controller
    {
        public IActiveDirectoryService ActiveDirectory { get; }

        public HomeController(IActiveDirectoryService activeDirectory)
        {
            ActiveDirectory = activeDirectory;
        }
        public IActionResult Index()
        {
            var result = ActiveDirectory.LoadData(User.Identity.Name);
            return View(result);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
