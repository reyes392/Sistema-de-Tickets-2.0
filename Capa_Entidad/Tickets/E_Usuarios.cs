using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Tickets
{
    public class E_Usuarios
    {
        public int IdUsuario { get; set; }

        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? UserName { get; set; }
        public string? Clave { get; set; }

        public int IdRol { get; set; }
        public int IdEstado { get; set; }
        public int IdCategoria { get; set; }

        public string? Correo { get; set; }


        public DateTime? FechaRegistrar { get; set; }
        public DateTime? FechaModificacion { get; set; }


        public string? Rol { get; set; }
        public string? Estado { get; set; }
        public string? Categoria { get; set; }


    }
}
