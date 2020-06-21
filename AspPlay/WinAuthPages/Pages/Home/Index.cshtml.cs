using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WinAuthPages.Pages.Home
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public int Number { get; set; }

        public void OnGet()
        {

        }
    }
}