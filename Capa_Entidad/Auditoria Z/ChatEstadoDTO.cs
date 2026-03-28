using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Auditoria_Z
{
    public class ChatEstadoDTO
    {
        public int IdAuditoriaZ { get; set; } // Identificador del registro
        public int UltimoId { get; set; }
        public int IdAutor { get; set; }
    }
}
