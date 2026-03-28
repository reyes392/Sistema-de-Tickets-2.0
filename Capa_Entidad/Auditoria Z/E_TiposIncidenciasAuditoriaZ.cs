using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Auditoria_Z
{
    public class E_TiposIncidenciasAuditoriaZ
    {
        public int IdTiposIncidenciasAuditoriaZ { get; set; }
        public string? Descripcion { get; set; }
        public int IdEstado { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? Estado { get; set; }
    }
}
