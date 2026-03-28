using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Canjes
{
    public class E_Comentarios
    {
        public int IdComentario { get; set; }
        public int IdCanje { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; } // Propiedad extendida
        public string? Mensaje { get; set; }
        public string? NombreArchivo { get; set; }
        public string? RutaArchivo { get; set; }
        public string? Extension { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string FotoPerfil { get; set; } // <--- Debe existir exactamente así
    }
}
