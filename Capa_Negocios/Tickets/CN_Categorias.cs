using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Categorias
    {
        private readonly CD_Categorias _dao;

        public CN_Categorias(CD_Categorias dao)
        {
            _dao = dao;
        }
        public List<E_Categorias> ListarCategorias() => _dao.ListarCategorias();
    }
}
