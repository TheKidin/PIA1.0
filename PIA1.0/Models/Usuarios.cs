using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIA1._0.Models
{
    internal class Usuarios
    {
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
