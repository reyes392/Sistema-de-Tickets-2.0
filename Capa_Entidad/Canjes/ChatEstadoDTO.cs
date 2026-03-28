using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Canjes
{
    public class ChatEstadoDTO
    {
        public int IdCanje { get; set; } // Agregado para identificar en listas masivas
        public int UltimoId { get; set; }
        public int IdAutor { get; set; }
    }
}
