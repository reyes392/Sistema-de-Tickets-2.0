using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Tickets
{
    public class E_Archivos
    {
        public int IdArchivo { get; set; }
        public int IdReferencia { get; set; } // El ID del Ticket, Usuario, etc.
        public string Entidad { get; set; }    // "TICKETS"
        public string NombreOriginal { get; set; }
        public string NombreSistema { get; set; }
        public string Extension { get; set; }
        public string Ruta { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
