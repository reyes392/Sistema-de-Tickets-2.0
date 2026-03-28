using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using System.Collections.Generic;

namespace Capa_Negocios.Tickets
{
    public class CN_Usuarios
    {
        private readonly CD_Usuarios _dao;

        public CN_Usuarios(CD_Usuarios dao)
        {
            _dao = dao;
        }

        public List<E_Usuarios> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Usuarios obj, string accion, int idUsuarioLogueado, out string mensaje)
        {
            mensaje = "";

            // REGLA DE SEGURIDAD: No permitirse editarse a sí mismo
            if (accion == "UPDATE" && obj.IdUsuario == idUsuarioLogueado)
            {
                mensaje = "Por políticas de seguridad, no puedes editar tu propio perfil desde este módulo.";
                return false;
            }

            // Validaciones básicas
            if (string.IsNullOrEmpty(obj.Nombres)) { mensaje = "Ingrese nombres"; return false; }
            if (string.IsNullOrEmpty(obj.UserName)) { mensaje = "Ingrese usuario"; return false; }

            // Hash + Salt solo si es INSERT o si UPDATE trae contraseña
            if (accion == "INSERT" || accion == "UPDATE" && !string.IsNullOrEmpty(obj.Clave))
            {
                obj.Clave = CN_Recursos.GenerarHashConSalt(obj.Clave);
            }

            return _dao.Guardar(obj, accion, out mensaje);
        }
        public bool ActualizarFoto(int idUsuario, string nombreArchivo, out string mensaje)
        {
            return _dao.ActualizarFoto(idUsuario, nombreArchivo, out mensaje);
        }
        public string ObtenerCorreoPorId(int idUsuario)
        {
            // Buscamos en la lista el usuario y devolvemos su correo
            var usuario = _dao.Listar().FirstOrDefault(u => u.IdUsuario == idUsuario);
            return usuario?.Correo ?? string.Empty;
        }
    }
}
