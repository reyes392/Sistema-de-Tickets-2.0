using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Tickets
{
    public class E_Tipos_Incidencias_Tickets
    {
        public int IdTiposIncidenciasTickets { get; set; }
        public string? Descripcion { get; set; }
        public int IdEstado { get; set; }
        public int IdNivelUrgencia { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? Estado { get; set; }
        public string? NivelUrgencia { get; set; }
        public string? TiempoRespuesta { get; set; }
    }
}
