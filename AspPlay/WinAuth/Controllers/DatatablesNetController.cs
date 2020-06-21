using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WinAuth.Controllers
{
    public class DatatablesNetController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Data([FromBody]DatatablesNetPost model)
        {
            var nameContains = Request.Query["name-contains"].FirstOrDefault();
            var query = Mvc6GridController.GridModelList.AsQueryable();
            if (nameContains != null)
            {
                query = query.Where(e => e.Name.Contains(nameContains));
            }
            int totalRows = query.Count();
            ViewBag.TotalRows = totalRows;
            var order = model.Order?.FirstOrDefault();
            if (order != null) {
                query = query.OrderBy($"{model.Columns[order?.Column ?? 0].Data} {order?.Dir ?? "asc"}");
            }
            query = query.Skip(model.Start)
                .Take(Math.Max(model.Length, 10));
            model.RecordsTotal = totalRows;
            model.RecordsFiltered = totalRows;
            model.Data = query.ToList();
            return Json(model);
        }
    }
}