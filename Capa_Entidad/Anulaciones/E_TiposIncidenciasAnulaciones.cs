using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Anulaciones
{
    public class E_TiposIncidenciasAnulaciones
    {
        public int IdTiposIncidenciasAnulaciones { get; set; }
        public string? Descripcion { get; set; }
        public int IdEstado { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? Estado { get; set; }
    }
}
