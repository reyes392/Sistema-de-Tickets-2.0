using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sistema_de_Tickets_2._0.Models;
using Capa_Entidad;
using Capa_Negocios;
using Sistema_de_Tickets_2._0.Filter;
using Capa_Datos;
using Capa_Entidad.Tickets;
using Capa_Entidad.ReclamosBodega;
using Capa_Negocios.Tickets;
using Capa_Negocios.ReclamosBodega;

namespace Sistema_de_Tickets_2._0.Controllers
{

    public class ReclamosBodegaController : Controller
    {

        #region INYECCION DE DEPENDENCIAS
        private readonly ILogger<ReclamosBodegaController> _logger;
        private readonly CN_Usuarios _negocioUsuario;
        private readonly CN_Permisos _negocioPermisos;
        private readonly CN_Roles _negocioRoles;
        private readonly CN_Estados _negocioEstados;
        private readonly CN_TiposIncidenciasReclamos _negocioIncidencias;
        private readonly CN_ReclamosBodega _negocioReclamos;
        private readonly Capa_Negocios.ReclamosBodega.CN_Archivos _negocioArchivos;
        private readonly Capa_Negocios.ReclamosBodega.CN_Comentarios _negocioComentarios;






        // UN SOLO CONSTRUCTOR PARA TODO
        public ReclamosBodegaController(ILogger<ReclamosBodegaController> logger, CN_Usuarios usuarioNegocio, CN_Permisos negocio, CN_Roles negocioRoles, CN_Estados negocioEstados, CN_Categorias negocioCategorias, CN_TiposIncidenciasReclamos negocioIncidencias,
            CN_ReclamosBodega negocioReclamos, Capa_Negocios.ReclamosBodega.CN_Archivos negocioArchivos, Capa_Negocios.ReclamosBodega.CN_Comentarios negocioComentarios)
        {
            _logger = (ILogger<ReclamosBodegaController>?)logger;
            _negocioUsuario = usuarioNegocio;
            _negocioPermisos = negocio;
            _negocioRoles = negocioRoles;
            _negocioEstados = negocioEstados;
            _negocioIncidencias = negocioIncidencias;
            _negocioReclamos = negocioReclamos;
            _negocioArchivos = negocioArchivos;
            _negocioComentarios = negocioComentarios;
        }
        #endregion

        #region TIPOS DE INCIDENCIAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GESTION DE TIPOS DE INCIDENCIAS DE RECLAMOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("INCIDENCIAS_RECLAMOS_VER")]
        public IActionResult TiposIncidenciasReclamos()
        {

            ViewBag.Estados = _negocioEstados.ListarEstados();
            var lista = _negocioIncidencias.Listar();
            return View(lista);
        }

        [Permiso("INCIDENCIAS_RECLAMOS_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardaIncidenciasReclamos(E_TiposIncidenciasReclamos incidencias, string Accion)
        {
            bool resultado = _negocioIncidencias.Guardar(incidencias, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE RECLAMOS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE RECLAMOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        [Permiso("RECLAMOS_VER")]
        public IActionResult Reclamos()
        {
            ViewBag.Incidencias = _negocioIncidencias.Listar();
            ViewBag.Estados = _negocioEstados.ListarEstados();

            // Obtenemos datos de sesión
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;
            string nombreLogueado = HttpContext.Session.GetString("NombreUsuario") ?? "Usuario";

            // Pasamos a la vista para el JS
            ViewBag.IdUsuarioLogueado = idLogueado;
            ViewBag.RolUsuario = idRolLogueado;
            ViewBag.NombreUsuarioLogueado = nombreLogueado;

            var lista = _negocioReclamos.Listar();

            // LÓGICA DE FILTRADO SOLICITADA:
            // Solo Admin (1) y Soporte (3) ven todo. Los demás solo lo propio.
            if (idRolLogueado != 1 && idRolLogueado != 3)
            {
                lista = lista.Where(t => t.IdUsuarioSolicitud == idLogueado).ToList();
            }

            return View(lista);
        }
        [Permiso("RECLAMO_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarReclamo(E_ReclamosBodega reclamos, string Accion, List<IFormFile> archivos)
        {
            // 1. Recuperamos el ID del usuario desde la Sesión
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null)
            {
                return Json(new { success = false, mensaje = "La sesión ha expirado. Por favor, inicie sesión nuevamente." });
            }

            // 2. Si es un INSERT, asignamos el ID del usuario de la sesión como solicitante
            if (Accion == "INSERT")
            {
                reclamos.IdUsuarioSolicitud = idUsuarioSesion.Value;
            }

            // 3. Guardamos el Ticket (Recuerda que ahora el SP devuelve el ID generado en tickets.IdTicket)
            bool resultado = _negocioReclamos.Guardar(reclamos, Accion, out string mensaje);

            //4.Lógica de Archivos: Solo si el ticket se guardó bien y vienen archivos
            if (resultado && archivos != null && archivos.Count > 0)
            {
                try
                {
                    // Ruta física: wwwroot/uploads/reclamos/
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reclamos");

                    foreach (var file in archivos)
                    {
                        // Guardar el archivo físico usando tu Capa de Negocio de archivos
                        string nombreSistema = _negocioArchivos.GuardarFisico(file, folderPath);

                        // Registrar la referencia en la base de datos
                        var entidadArchivo = new E_Archivos
                        {
                            IdReferencia = reclamos.IdReclamo, // El ID que devolvió el SP
                            NombreOriginal = file.FileName,
                            NombreSistema = nombreSistema,
                            Extension = Path.GetExtension(file.FileName),
                            Ruta = "/uploads/reclamos/" + nombreSistema
                        };

                        _negocioArchivos.RegistrarEnBaseDatos(entidadArchivo);
                    }
                }
                catch (Exception ex)
                {
                    // Opcional: Podrías acumular un mensaje de error si los archivos fallan pero el ticket no
                    mensaje += " (Aviso: El reclamo se guardó pero hubo problemas con los archivos: " + ex.Message + ")";
                }
            }

            return Json(new { success = resultado, mensaje = mensaje });
        }

        [HttpGet]
        public JsonResult ObtenerAdjuntos(int idReclamo)
        {
            // Llamamos a la capa de negocios que a su vez llama a la de datos
            // El método Listar(idTicket) ya lo tienes definido en CN_Archivos
            var lista = _negocioArchivos.Listar(idReclamo);

            // Retornamos la lista en formato JSON para que el JS la procese
            return Json(lista);
        }

        [HttpPost]
        public IActionResult TomarReclamo(int idReclamo)
        {
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuarioSesion == 0) return Json(new { success = false, mensaje = "Sesión expirada" });

            // Llamamos al método que asigna y cambia estado
            bool ok = _negocioReclamos.AsignarYProcesar(idReclamo, idUsuarioSesion, out string mensaje);

            return Json(new { success = ok, mensaje = mensaje });
        }

        #endregion

        #region GESTIÓN DE COMENTARIOS (CHAT)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// GESTIÓN DE COMENTARIOS (CHAT)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public IActionResult GuardarComentario(int idReclamo, string mensaje, IFormFile? archivo)
        {
            // 1. Recuperamos el ID del usuario desde la Sesión (siguiendo tu ejemplo)
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null)
            {
                return Json(new { success = false, mensaje = "La sesión ha expirado." });
            }

            try
            {
                var oComentario = new Capa_Entidad.ReclamosBodega.E_Comentarios
                {
                    IdReclamo = idReclamo,
                    IdUsuario = idUsuarioSesion.Value,
                    Mensaje = mensaje
                };

                // CAMBIO AQUÍ: Solo enviamos la ruta base del proyecto
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // La Capa de Negocio se encargará de entrar a uploads/comentarios
                bool resultado = _negocioComentarios.RegistrarComentario(oComentario, archivo, webRootPath);

                return Json(new { success = resultado, mensaje = resultado ? "Enviado" : "Error" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerComentarios(int idReclamo)
        {
            // Obtenemos el ID de sesión para que el JS sepa cuáles mensajes son "míos"
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            var lista = _negocioComentarios.Listar(idReclamo);

            // Mapeamos para que el JS reciba nombres de propiedades consistentes
            var resultado = lista.Select(c => new
            {
                idComentario = c.IdComentario,
                nombreUsuario = c.NombreUsuario,
                mensaje = c.Mensaje,
                nombreArchivo = c.NombreArchivo,
                rutaArchivo = c.RutaArchivo,
                extension = c.Extension,
                fecha = c.FechaRegistro.ToString("dd/MM HH:mm"),
                esMio = c.IdUsuario == idUsuarioSesion
            }).ToList();

            return Json(resultado);
        }


        [HttpGet]
        public string ObtenerVersionReclamos()
        {
            var lista = _negocioReclamos.Listar();
            // Sumamos los estados + el conteo. 
            // Si un ticket pasa de estado 3 a 5, el total cambia y el JS lo detecta.
            int sumaEstados = lista.Sum(t => t.IdEstado);
            int conteo = lista.Count;

            return $"{conteo}-{sumaEstados}";
        }
        [HttpGet]
        public JsonResult ObtenerUltimoComentarioId(int idReclamo)
        {
            var lista = _negocioComentarios.Listar(idReclamo);
            int ultimoId = 0;
            if (lista != null && lista.Count > 0)
            {
                ultimoId = lista.Max(x => x.IdComentario);
            }
            return Json(new { ultimoId = ultimoId });
        }

        [HttpGet]
        public JsonResult ObtenerEstadoChat(int idReclamo)
        {
            var estado = _negocioReclamos.ObtenerEstadoChat(idReclamo);

            // En .NET Core, no hace falta JsonRequestBehavior.AllowGet
            return Json(new
            {
                ultimoId = estado.UltimoId,
                idAutor = estado.IdAutor
            });
        }
        #endregion
    }
}
