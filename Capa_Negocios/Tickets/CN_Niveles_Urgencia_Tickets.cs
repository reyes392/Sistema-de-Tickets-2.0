using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Niveles_Urgencia_Tickets
    {
        private readonly CD_Niveles_Urgencia_tickets _dao;

        public CN_Niveles_Urgencia_Tickets(CD_Niveles_Urgencia_tickets dao)
        {
            _dao = dao;
        }
        public List<E_Niveles_Urgencia_Tickets> Listar() => _dao.Listar();
        public bool Guardar(E_Niveles_Urgencia_Tickets obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }
            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
