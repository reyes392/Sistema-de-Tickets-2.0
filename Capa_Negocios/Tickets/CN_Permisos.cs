using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;

namespace Capa_Negocios.Tickets
{
    public class CN_Permisos
    {
        private readonly CD_Permisos _dao;

        public CN_Permisos(CD_Permisos dao)
        {
            _dao = dao;
        }

        public List<E_Permiso> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Permiso permiso, string accion, out string mensaje)
        {
            mensaje = "";

            if (string.IsNullOrEmpty(permiso.Descripcion))
            {
                mensaje = "Ingrese descripción";
                return false;
            }

            if (string.IsNullOrEmpty(permiso.Codigo))
            {
                mensaje = "Ingrese código";
                return false;
            }

            return _dao.Guardar(permiso, accion, out mensaje);
        }

        public void AsignarRol(int rol, int permiso)
        {
            _dao.AsignarRol(rol, permiso);
        }

        public List<string> ObtenerPermisosUsuario(int idUsuario)
        {
            return _dao.ObtenerPermisosUsuario(idUsuario);
        }



        public List<int> ObtenerPermisosPorRol(int idRol)
        {
            return _dao.ObtenerPermisosPorRol(idRol);
        }

        public void QuitarRol(int rol, int permiso)
        {
            _dao.QuitarRol(rol, permiso);
        }

        public void AsignarUsuario(int idUsuario, int idPermiso)
        {
            _dao.AsignarUsuario(idUsuario, idPermiso);
        }

        public void QuitarUsuario(int idUsuario, int idPermiso)
        {
            _dao.QuitarUsuario(idUsuario, idPermiso);
        }

        public List<E_Permiso> ObtenerPermisosUsuarioCompleto(int idUsuario)
        {
            return _dao.ObtenerPermisosUsuarioCompleto(idUsuario);
        }

    }
}
