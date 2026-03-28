using Capa_Datos.Anulaciones;
using Capa_Entidad.Anulaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Anulaciones
{
    public class CN_TiposIncidenciasAnulaciones
    {
        private readonly CD_TiposIncidenciasAnulaciones _dao;

        public CN_TiposIncidenciasAnulaciones(CD_TiposIncidenciasAnulaciones dao)
        {
            _dao = dao;
        }

        public List<E_TiposIncidenciasAnulaciones> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_TiposIncidenciasAnulaciones obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }
            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
