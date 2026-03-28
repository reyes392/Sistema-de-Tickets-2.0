using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad.Tickets
{
    public class ChatEstadoDTO
    {
        public int IdTicket { get; set; } // <--- CRÍTICO: Para mapear en el cliente
        public int UltimoId { get; set; }
        public int IdAutor { get; set; }
    }
}
