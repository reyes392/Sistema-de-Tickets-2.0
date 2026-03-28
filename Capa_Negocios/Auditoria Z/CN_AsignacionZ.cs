using Capa_Datos.Auditoria_Z;
using Capa_Entidad.Auditoria_Z;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Auditoria_Z
{
    public class CN_AsignacionZ
    {
        private readonly CD_AsignacionZ _daoCanjes;
        public CN_AsignacionZ(CD_AsignacionZ dao) { _daoCanjes = dao; }

        public List<E_Asignacion_Z> Listar() => _daoCanjes.ListarAsignaciones();

        public bool Registrar(E_Asignacion_Z obj, out string mensaje)
        {
            if (obj.IdAsignacionZ == obj.IdUsuarioSolicitante)
            {
                mensaje = "No puede asignar a un usuario como su propio técnico.";
                return false;
            }
            return _daoCanjes.Registrar(obj, out mensaje);
        }

        public bool Eliminar(int id) => _daoCanjes.Eliminar(id);
    }
}
