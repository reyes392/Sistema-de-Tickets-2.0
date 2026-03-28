using Capa_Datos.Canjes;

using Capa_Entidad.Canjes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Canjes
{
    public class CN_AsignacionCanjes
    {
        private readonly CD_AsignacionCanjes _daoCanjes;
        public CN_AsignacionCanjes(CD_AsignacionCanjes dao) { _daoCanjes = dao; }

        public List<E_AsignacionCanjes> Listar() => _daoCanjes.ListarAsignaciones();

        public bool Registrar(E_AsignacionCanjes obj, out string mensaje)
        {
            if (obj.IdUsuarioCanjes == obj.IdUsuarioSolicitante)
            {
                mensaje = "No puede asignar a un usuario como su propio técnico.";
                return false;
            }
            return _daoCanjes.Registrar(obj, out mensaje);
        }

        public bool Eliminar(int id) => _daoCanjes.Eliminar(id);
    }

}
