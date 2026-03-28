using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Canjes
{
    public class E_AsignacionCanjes
    {
        public int IdAsignacion { get; set; }

        // El Técnico/Usuario que atiende
        public int IdUsuarioCanjes { get; set; }
        public string? NombreUsuarioCanjes { get; set; } // Propiedad auxiliar

        // El Solicitante asignado
        public int IdUsuarioSolicitante { get; set; }
        public string? NombreUsuarioSolicitante { get; set; } // Propiedad auxiliar

        public DateTime FechaAsignacion { get; set; }
    }
}
