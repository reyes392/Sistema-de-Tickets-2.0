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
    public class CN_Equipos
    {


        private readonly CD_Equipos _dao;

        public CN_Equipos(CD_Equipos dao)
        {
            _dao = dao;
        }

        public List<E_Equipos> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Equipos obj, string accion, out string mensaje)
        {
            mensaje = "";

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Descripcion)) { mensaje = "Ingrese Descripcion"; return false; }

            return _dao.Guardar(obj, accion, out mensaje);
        }


    }
}
