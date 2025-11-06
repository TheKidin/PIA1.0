using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIA1._0.Models
{
    public class Inventario
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int CantidadActual { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public int PuntoReorden { get; set; }
    }
}
