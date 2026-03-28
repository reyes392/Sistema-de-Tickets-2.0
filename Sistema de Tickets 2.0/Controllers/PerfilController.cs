using Capa_Negocios.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_de_Tickets_2._0.Controllers
{
    public class PerfilController : Controller
    {
        #region INYECCION DE DEPENDENCIAS
        private readonly ILogger<PerfilController> _logger;
        private readonly CN_Usuarios _usuarioNegocio;
      

        // UN SOLO CONSTRUCTOR PARA TODO
        public PerfilController(ILogger<PerfilController> logger, CN_Usuarios usuarioNegocio       )
        {
            _logger = logger;
            _usuarioNegocio = usuarioNegocio;
        

        }
        #endregion
        // Muestra la vista de perfil
        public IActionResult Perfil()
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            var usuario = _usuarioNegocio.Listar().FirstOrDefault(u => u.IdUsuario == idUsuario);
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarFotoPerfil(IFormFile fotoArchivo)
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            string mensaje = "";

            if (fotoArchivo != null && fotoArchivo.Length > 0)
            {
                // 1. Carpeta y Nombre Único
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "perfiles");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string extension = Path.GetExtension(fotoArchivo.FileName);
                string nombreArchivo = $"perfil_{idUsuario}_{DateTime.Now.Ticks}{extension}";
                string rutaCompleta = Path.Combine(folder, nombreArchivo);

                // 2. Guardar Físico
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await fotoArchivo.CopyToAsync(stream);
                }

                // 3. Llamar al NUEVO método especializado
                bool ok = _usuarioNegocio.ActualizarFoto(idUsuario, nombreArchivo, out mensaje);

                if (ok)
                {
                    HttpContext.Session.SetString("FotoPerfil", nombreArchivo);
                    TempData["S_Mensaje"] = "¡Foto actualizada con éxito!";
                }
                else
                {
                    TempData["E_Mensaje"] = "Error en BD: " + mensaje;
                }
            }

            return RedirectToAction("Perfil");
        }
    }
}
