using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Tickets
{
    public class E_Tickets
    {
        public int IdTicket { get; set; }
        public string? UsusarioSolicitador { get; set; }
        public string? Incidencia { get; set; }
        public string? Caja { get; set; }
        public string? NivelUrgencia { get; set; }
        public string? Respuesta { get; set; }
        public string? Problema { get; set; }
        public string? Resolucion { get; set; }
        public string? Estado { get; set; }
        public string? Categoria { get; set; }
        public string? UsuarioAsignado { get; set; }
        public DateTime? Registro { get; set; }
        public DateTime? Modificacion { get; set; }


        // Propiedades para inserción/edición
        public int IdUsuarioSolicitud { get; set; }
        public int IdCaja { get; set; }
        public int IdIncidencias { get; set; }
        public int IdNivelesUrgencias { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuarioAsignado { get; set; }
        public int IdCategorias { get; set; }

        // Dentro de Capa_Entidad/E_Tickets.cs
        public string? RutaServidor { get; set; }
        public string? Extension { get; set; }
        public string? NombreArchivo { get; set; }
        public DateTime? FechaTicket { get; set; } // Nuevo
        public string? Turno { get; set; }         // Nuevo

    }
}
