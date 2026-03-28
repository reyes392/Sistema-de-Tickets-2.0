using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Canjes
{
    public class E_Canje
    {
        // Campos directos de la tabla CANJES
        public int IdCanje { get; set; }
        public int IdUsuarioSolicitador { get; set; }
        public int IdTipoIncidenciaCanjes { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuarioAsignado { get; set; }

        // Campos que pueden ser nulos (Nullable)
        public int? IdUsuarioAutorizador { get; set; }
        public int? IdUsuarioAnulador { get; set; }

        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaAutorizado { get; set; }
        public DateTime? FechaAnulado { get; set; }

        // Propiedades de navegación / Visualización (Provenientes de los JOIN)
        // Estas son las que usa tu SP_LISTAR_CANJES_FILTRADO
        // --- NUEVO CAMPO ---
        public string? DescripcionProblema { get; set; }
        public string? Resolucion { get; set; }
        public string? NombreSolicitador { get; set; }
        public string? TipoIncidencia { get; set; }
        public string? Estado { get; set; }
        public string? NombreAsignado { get; set; }

        // Opcional: Si necesitas mostrar quién autorizó o anuló en algún detalle
        public string? NombreAutorizador { get; set; }
        public string? NombreAnulador { get; set; }
    }
}
