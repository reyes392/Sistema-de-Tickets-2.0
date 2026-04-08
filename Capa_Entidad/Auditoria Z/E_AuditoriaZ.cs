using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Auditoria_Z
{
    public class E_AuditoriaZ
    {
        // Campos directos de la tabla AUDITORIA_Z
        public int IdAuditoriaZ { get; set; }
        public int IdUsuarioSolicitador { get; set; }
        public int IdTipoIncidenciaAuditoriaZ { get; set; }
        public string? DescripcionProblema { get; set; }
        public string? ResolucionProblema { get; set; }
        public int IdEstado { get; set; }
        public int? IdUsuarioAsignado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Propiedades de navegación / Visualización (Provenientes de los JOIN)
        public string? NombreSolicitador { get; set; }
        public string? TipoIncidenciaAuditoria { get; set; }
        public string? Estado { get; set; }
        public string? NombreAsignado { get; set; }
        public int? IdUsuarioModificador { get; set; }
        public string NombreModificador { get; set; }
    }
}
