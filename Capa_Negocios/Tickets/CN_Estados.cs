using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Estados
    {
        private readonly CD_Estados _dao;

        public CN_Estados(CD_Estados dao)
        {
            _dao = dao;
        }
        public List<E_Estados> ListarEstados() => _dao.ListarEstados();
    }
}
