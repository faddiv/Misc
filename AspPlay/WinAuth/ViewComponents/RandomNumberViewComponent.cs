using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinAuth.ViewComponents
{
    public class RandomNumberViewComponent : ViewComponent
    {
        private readonly Random _rnd;

        public RandomNumberViewComponent()
        {
            _rnd = new Random();
        }

        public async Task<IViewComponentResult> InvokeAsync(int? max = Int32.MaxValue)
        {
            await Task.Delay(200);
            return View(_rnd.Next());
        }

    }
}
