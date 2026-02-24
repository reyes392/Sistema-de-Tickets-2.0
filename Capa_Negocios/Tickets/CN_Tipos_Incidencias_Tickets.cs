using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using Capa_Negocios.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Tipos_Incidencias_Tickets
    {
        private readonly CD_Tipos_Incidencias_Tickets _dao;

        public CN_Tipos_Incidencias_Tickets(CD_Tipos_Incidencias_Tickets dao)
        {
            _dao = dao;
        }

        public List<E_Tipos_Incidencias_Tickets> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Tipos_Incidencias_Tickets obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }
            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
