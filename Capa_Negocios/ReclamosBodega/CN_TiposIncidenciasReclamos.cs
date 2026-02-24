using Capa_Datos.ReclamosBodega;
using Capa_Datos.Tickets;
using Capa_Entidad.ReclamosBodega;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.ReclamosBodega
{
    public class CN_TiposIncidenciasReclamos
    {
        private readonly CD_TiposIncidenciasReclamos _dao;

        public CN_TiposIncidenciasReclamos(CD_TiposIncidenciasReclamos dao)
        {
            _dao = dao;
        }

        public List<E_TiposIncidenciasReclamos> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_TiposIncidenciasReclamos obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }
            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
