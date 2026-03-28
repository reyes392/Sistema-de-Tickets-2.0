using Capa_Datos.Canjes;
using Capa_Entidad.Canjes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Canjes
{
    public class CN_TiposIncidenciasCanjes
    {

        private readonly CD_TiposIncidenciasCanjes _dao;

        public CN_TiposIncidenciasCanjes(CD_TiposIncidenciasCanjes dao)
        {
            _dao = dao;
        }

        public List<E_TiposIncidenciasCanjes> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_TiposIncidenciasCanjes obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }
            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
