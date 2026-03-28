using Capa_Datos.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Notificacion
    {
        private readonly CD_Notificacion _cdAcceso;

        public CN_Notificacion(CD_Notificacion cdAcceso)
        {
            _cdAcceso = cdAcceso;
        }

        public int ObtenerConteoActividad(int idUsuario, int idRol)
        {
            return _cdAcceso.ObtenerConteoActividad(idUsuario, idRol);
        }
        public int ObtenerConteoActividadGlobal(int idUsuario, int idRol)
        {
            // Aquí podrías agregar lógica extra si fuera necesario, 
            // pero por ahora solo llamamos a la capa de datos
            return _cdAcceso.ObtenerConteoActividadGlobal(idUsuario, idRol);
        }
    }
}
