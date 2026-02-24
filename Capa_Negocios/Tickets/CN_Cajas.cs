using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Cajas
    {
        private readonly CD_Cajas _dao;

        public CN_Cajas(CD_Cajas dao)
        {
            _dao = dao;
        }

        public List<E_Cajas> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Cajas obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }

            return _dao.Guardar(obj, accion, out mensaje);
        }
    }
}
