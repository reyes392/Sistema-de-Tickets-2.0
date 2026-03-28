using Capa_Datos.Auditoria_Z;
using Capa_Entidad.Auditoria_Z;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Auditoria_Z
{
    public class CN_TiposIncidenciasAuditoriaZ
    {
        private readonly CD_TiposIncidenciasAuditoriaZ _dao;

        public CN_TiposIncidenciasAuditoriaZ(CD_TiposIncidenciasAuditoriaZ dao)
        {
            _dao = dao;
        }

        public List<E_TiposIncidenciasAuditoriaZ> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_TiposIncidenciasAuditoriaZ obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }
            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
