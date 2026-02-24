using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Roles
    {
        private readonly CD_Roles _dao;

        public CN_Roles(CD_Roles dao)
        {
            _dao = dao;
        }
        public List<E_Roles> ListarRoles() => _dao.ListarRoles();




        public bool Guardar(E_Roles rol, string accion, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrEmpty(rol.Descripcion))
            {
                mensaje = "Ingrese descripción del rol";
                return false;
            }

            return _dao.Guardar(rol, accion, out mensaje);
        }

    }
}
