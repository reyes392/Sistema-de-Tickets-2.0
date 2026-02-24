using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.ReclamosBodega
{
    public class E_ReclamosBodega
    {
        public int IdReclamo { get; set; }
        public string? UsuarioSolicitador { get; set; }
        public string? TipoIncidenciaReclamo { get; set; }
        public string? Nrequisa { get; set; }
        public string? FechaRequisa { get; set; }
        public string? Problema { get; set; }
        public string? Resolucion { get; set; }
        public string? Estado { get; set; }

        public string? UsuarioAsignado { get; set; }
        public DateTime? Registro { get; set; }
        public DateTime? Modificacion { get; set; }

        // Propiedades para inserción/edición
        public int IdUsuarioSolicitud { get; set; }
        public int IdTipoIncidencia { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuarioAsignado { get; set; }

        //Archivos

        public string? RutaServidor { get; set; }
        public string? Extension { get; set; }
        public string? NombreArchivo { get; set; }
    }
}
