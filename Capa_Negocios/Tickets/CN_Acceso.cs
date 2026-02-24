using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Tickets
{
    public class CN_Acceso
    {
        private readonly CD_Acceso _cdAcceso;

        public CN_Acceso(CD_Acceso cdAcceso)
        {
            _cdAcceso = cdAcceso;
        }

        // Validar usuario
        public bool ValidarUsuario(string usuario, string clave, out E_Usuarios usuarioObj)
        {
            // 1. Buscamos al usuario en la Capa de Datos
            usuarioObj = _cdAcceso.ObtenerUsuario(usuario);

            if (usuarioObj == null) return false;

            // 2. IMPORTANTE: Usar el método que separa el PIPE (|) y verifica el hash
            // En lugar de: return usuarioObj.Clave == claveHasheada;
            bool esValida = CN_Recursos.VerificarHashConSalt(clave, usuarioObj.Clave);

            return esValida;
        }


        // Obtener todos los permisos de un usuario (rol + permisos adicionales)
        public List<string> ObtenerPermisos(int idUsuario)
        {
            var usuario = _cdAcceso.ObtenerUsuarioPorId(idUsuario);

            if (usuario == null)
                return new List<string>();

            var permisosRol = _cdAcceso.ObtenerPermisosPorRol(usuario.IdRol);
            var permisosUsuario = _cdAcceso.ObtenerPermisosUsuario(idUsuario);

            return permisosRol
                    .Concat(permisosUsuario)
                    .Distinct()
                    .ToList();
        }


        // Verificar si un usuario tiene un permiso
        public bool TienePermiso(int idUsuario, string codigoPermiso)
        {
            return ObtenerPermisos(idUsuario).Contains(codigoPermiso);
        }



        // Cambiamos el orden de la tupla de (controller, action) a (action, controller)
        public (string action, string controller) ObtenerPrimeraVista(int idUsuario)
        {
            var permisos = ObtenerPermisos(idUsuario);

            var rutas = new Dictionary<string, (string, string)>
                {

                    { "USUARIOS_VER", ("Usuarios", "Home") }, // Acción Usuarios() en HomeController
                    { "INDEX_VER", ("Index", "Home") },       // Acción Index() en HomeController
                    { "PERMISOS_VER", ("Permisos", "Home") } , // Acción Permisos() en HomeController
                   { "ROLES_VER", ("RolesyPermisos", "Home") } , // Acción Permisos() en HomeController
                     { "ROLES_USUARIOS_VER", ("RolesyPermisos", "Home") },  // Acción Permisos() en HomeController
                      { "TICKET_VER", ("Tickets", "Home") },  // Acción Permisos() en HomeController
                };

            foreach (var permiso in permisos)
            {
                if (rutas.ContainsKey(permiso))
                {
                    return rutas[permiso];
                }
            }

            return ("SinAcceso", "Home");
        }

    }
}

