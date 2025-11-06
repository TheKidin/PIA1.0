using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIA1._0.Models
{
    public class Productos
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Plataforma { get; set; } = string.Empty;

        public decimal PrecioVenta { get; set; }
    }
}
