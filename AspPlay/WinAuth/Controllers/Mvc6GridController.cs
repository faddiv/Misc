using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace WinAuth.Controllers
{
    public class Mvc6GridController : Controller
    {
        public static IList<GridModel> GridModelList { get; }
        static Mvc6GridController()
        {
            GridModelList = FizzWare.NBuilder.Builder<GridModel>.CreateListOfSize(1000)
                .Build();
        }
        public Mvc6GridController()
        {

        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Data(int page = 1, int rows = 10, string sort = "id", string order = "asc")
        {
            var nameContains = Request.Query["name-contains"].FirstOrDefault();
            var query = GridModelList.AsQueryable();
            if(nameContains != null)
            {
                query = query.Where(e => e.Name.Contains(nameContains));
            }
            ViewBag.TotalRows = query.Count();
            query = query.OrderBy($"{sort} {order}");
            query = query.Skip((page - 1) * rows)
                .Take(rows);
            return base.PartialView(query.ToList());
        }
    }
}
