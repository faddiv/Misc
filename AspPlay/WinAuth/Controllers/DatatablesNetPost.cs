using System.Collections;
using System.Collections.Generic;

namespace WinAuth.Controllers
{
    public class DatatablesNetPost
    {
        public int Draw { get; set; }
        public List<DNColumn> Columns { get; set; }
        public List<DNOrder> Order { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public ICollection Data { get; set; }
    }
}