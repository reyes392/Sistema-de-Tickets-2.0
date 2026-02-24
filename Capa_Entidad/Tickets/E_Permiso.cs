using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Tickets
{
    public class E_Permiso
    {
        public int IdPermiso { get; set; }
        public string? Descripcion { get; set; }
        public string? Codigo { get; set; }
        public bool AsignadoRol { get; set; }
        public bool AsignadoUsuario { get; set; }
    }
}
