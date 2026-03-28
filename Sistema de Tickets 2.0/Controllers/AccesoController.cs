using Microsoft.AspNetCore.Mvc;
using Capa_Entidad.Tickets;
using Capa_Negocios.Tickets;


namespace Sistema_de_Tickets_2._0.Controllers
{
    public class AccesoController : Controller
    {
        private readonly CN_Acceso _accesoNegocio;

        public AccesoController(CN_Acceso accesoNegocio)
        {
            _accesoNegocio = accesoNegocio;
        }


     
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string clave)
        {
            if (_accesoNegocio.ValidarUsuario(usuario, clave, out E_Usuarios usuarioObj))
            {
                // Guardamos los datos críticos en sesión
                HttpContext.Session.SetInt32("IdUsuario", usuarioObj.IdUsuario);
                HttpContext.Session.SetInt32("IdRol", usuarioObj.IdRol); // IMPORTANTE: Guardar el Rol
                HttpContext.Session.SetString("Usuario", usuarioObj.UserName);
                HttpContext.Session.SetString("NombreUsuario", usuarioObj.Nombres + " " + usuarioObj.Apellidos);
                HttpContext.Session.SetString("FotoPerfil", usuarioObj.FotoPerfil ?? "");

                var permisos = _accesoNegocio.ObtenerPermisos(usuarioObj.IdUsuario);

                HttpContext.Session.SetString(
                    "Permisos",
                    string.Join(",", permisos)
                );

                // 👉 OBTENER PRIMERA VISTA AUTORIZADA
                var ruta = _accesoNegocio.ObtenerPrimeraVista(usuarioObj.IdUsuario);

                return RedirectToAction(ruta.action, ruta.controller);
            }

            TempData["Error"] = "Usuario o contraseña incorrecta.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
