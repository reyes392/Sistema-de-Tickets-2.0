using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Anulaciones
{
    public class E_Anulaciones
    {
        public int IdAnulacion { get; set; }
        public string? UsuarioSolicitador { get; set; }
        public string? Incidencia { get; set; }
        public string? Caja { get; set; }
        public string? DescripcionProblema { get; set; } // NUEVO
        public string? Resolucion { get; set; }
        public string? Estado { get; set; }
        public string? UsuarioAsignado { get; set; }
        public DateTime? Registro { get; set; }
        public DateTime? Modificacion { get; set; }


        // Propiedades para inserción/edición
        public int IdCaja { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuarioSolicitud { get; set; }
        public int IdIncidencias { get; set; }
        public int IdUsuarioAsignado { get; set; }

        // Dentro de Capa_Entidad/E_Tickets.cs
        public string? RutaServidor { get; set; }
        public string? Extension { get; set; }
        public string? NombreArchivo { get; set; }
        public decimal Monto { get; set; }
        public string? NumeroFactura { get; set; }
        public DateTime? FechaFactura { get; set; }
    }
}
