using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Auditoria_Z
{
    public class E_Asignacion_Z
    {
        public int IdAsignacionZ { get; set; }

        // El Técnico/Usuario que atiende
        public int IdUsuarioAuditoriaZ { get; set; }
        public string? NombreUsuarioAuditoriaZ { get; set; } // Propiedad auxiliar

        // El Solicitante asignado
        public int IdUsuarioSolicitante { get; set; }
        public string? NombreUsuarioSolicitante { get; set; } // Propiedad auxiliar

        public DateTime FechaAsignacion { get; set; }
    }
}
