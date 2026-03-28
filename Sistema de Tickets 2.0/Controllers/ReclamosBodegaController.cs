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
        //[Permiso("RECLAMO_CREAR_EDITAR")]
        //[HttpPost]
        //public IActionResult GuardarReclamo(E_ReclamosBodega reclamos, string Accion, List<IFormFile> archivos)
        //{
        //    // 1. Recuperamos el ID del usuario desde la Sesión
        //    int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

        //    if (idUsuarioSesion == null)
        //    {
        //        return Json(new { success = false, mensaje = "La sesión ha expirado. Por favor, inicie sesión nuevamente." });
        //    }

        //    // 2. Si es un INSERT, asignamos el ID del usuario de la sesión como solicitante
        //    if (Accion == "INSERT")
        //    {
        //        reclamos.IdUsuarioSolicitud = idUsuarioSesion.Value;
        //    }

        //    // 3. Guardamos el Ticket (Recuerda que ahora el SP devuelve el ID generado en tickets.IdTicket)
        //    bool resultado = _negocioReclamos.Guardar(reclamos, Accion, out string mensaje);

        //    //4.Lógica de Archivos: Solo si el ticket se guardó bien y vienen archivos
        //    if (resultado && archivos != null && archivos.Count > 0)
        //    {
        //        try
        //        {
        //            // Ruta física: wwwroot/uploads/reclamos/
        //            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reclamos");

        //            foreach (var file in archivos)
        //            {
        //                // Guardar el archivo físico usando tu Capa de Negocio de archivos
        //                string nombreSistema = _negocioArchivos.GuardarFisico(file, folderPath);

        //                // Registrar la referencia en la base de datos
        //                var entidadArchivo = new E_Archivos
        //                {
        //                    IdReferencia = reclamos.IdReclamo, // El ID que devolvió el SP
        //                    NombreOriginal = file.FileName,
        //                    NombreSistema = nombreSistema,
        //                    Extension = Path.GetExtension(file.FileName),
        //                    Ruta = "/uploads/reclamos/" + nombreSistema
        //                };

        //                _negocioArchivos.RegistrarEnBaseDatos(entidadArchivo);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Opcional: Podrías acumular un mensaje de error si los archivos fallan pero el ticket no
        //            mensaje += " (Aviso: El reclamo se guardó pero hubo problemas con los archivos: " + ex.Message + ")";
        //        }
        //    }

        //    return Json(new { success = resultado, mensaje = mensaje });
        //}
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

            // 3. Guardamos el Reclamo
            bool resultado = _negocioReclamos.Guardar(reclamos, Accion, out string mensaje);

            // 4. Lógica de Archivos y Correo (Solo si el reclamo se guardó bien)
            if (resultado)
            {
                // --- PROCESAMIENTO DE ARCHIVOS ---
                if (archivos != null && archivos.Count > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reclamos");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        foreach (var file in archivos)
                        {
                            string nombreSistema = _negocioArchivos.GuardarFisico(file, folderPath);

                            var entidadArchivo = new E_Archivos
                            {
                                IdReferencia = reclamos.IdReclamo,
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
                        mensaje += " (Aviso: El reclamo se guardó pero hubo problemas con los archivos: " + ex.Message + ")";
                    }
                }

                // --- ENVÍO DE CORREO POR CIERRE DE RECLAMO ---
                // Verifica si el ID de estado "Cerrado" para reclamos es el mismo (ej. 4)
                if (Accion == "UPDATE" && reclamos.IdEstado == 4)
                {
                    // CAPTURA de datos del Request antes de disparar el hilo
                    string esquema = Request.Scheme;
                    string host = Request.Host.Value;
                    string urlBase = $"{esquema}://{host}";
                    int idReclamoGenerado = reclamos.IdReclamo;

                    Task.Run(() => {
                        try
                        {
                            // Obtenemos la info completa del reclamo para el correo
                            // Asegúrate de tener un método Listar o Obtener en Negocio Reclamos
                            var infoReclamo = _negocioReclamos.Listar().FirstOrDefault(r => r.IdReclamo == idReclamoGenerado);

                            if (infoReclamo != null)
                            {
                                string correoDestino = _negocioUsuario.ObtenerCorreoPorId(infoReclamo.IdUsuarioSolicitud);

                                if (!string.IsNullOrEmpty(correoDestino))
                                {
                                    string asunto = $"Reclamo de Bodega Cerrado: #{infoReclamo.IdReclamo}";

                                    string cuerpo = $@"
                            <div style='font-family: sans-serif; border: 1px solid #ddd; border-radius: 10px; padding: 20px; max-width: 600px;'>
                                <h2 style='color: #e67e22; border-bottom: 2px solid #e67e22; padding-bottom: 10px;'>Finalización de Reclamo - Bodega</h2>
                                <p>Hola <strong>{infoReclamo.UsuarioSolicitador}</strong>,</p>
                                <p>Se ha procesado y cerrado tu reclamo en el sistema.</p>
                                
                                <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
                                    <tr style='background: #f8f9fa;'>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>N° Reclamo:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>#{infoReclamo.IdReclamo}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{infoReclamo.UsuarioAsignado}</td>
                                    </tr>
                                      <tr style='background: #f8f9fa;'>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{infoReclamo.TipoIncidenciaReclamo}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{infoReclamo.Nrequisa}</td>
                                    </tr>
                                    <tr style='background: #f8f9fa;'>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{infoReclamo.FechaRequisa}</td>
                                    </tr>
                                   <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución/Comentario:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee; font-weight: bold; color: #27ae60;'>{infoReclamo.Resolucion}</td>
                                    </tr>
                                </table>

                              

                                <p style='margin-top: 20px; font-size: 0.8em; color: #7f8c8d; text-align: center;'>
                                    Departamento de Bodega - Farmacias Saba Nicaragua
                                </p>
                            </div>";

                                    CN_Recursos.EnviarCorreoInterno(correoDestino, asunto, cuerpo);
                                }
                            }
                        }
                        catch { /* Errores de envío no bloquean la UI */ }
                    });
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
                esMio = c.IdUsuario == idUsuarioSesion,
                fotoPerfil = c.FotoPerfil
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

    

        [HttpPost]
        public JsonResult ObtenerEstadosChatsReclamos([FromBody] List<int> idsReclamos)
        {
            if (idsReclamos == null || !idsReclamos.Any())
                return Json(new List<Capa_Entidad.ReclamosBodega.ChatEstadoDTO>());

            var resultado = _negocioReclamos.ObtenerEstadosChatsReclamos(idsReclamos);
            return Json(resultado);
        }
        #endregion

        #region UTILIDADES

    
        [Permiso("RECLAMOS_VER")]
        [HttpGet]
        public JsonResult ObtenerEstadoGlobalReclamos()
        {
            // 1. Recuperar datos de sesión
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

            // 2. Obtener la lista base de reclamos
            var listaReclamos = _negocioReclamos.Listar();

            // 3. Aplicar filtro de seguridad según rol
            if (idRolLogueado != 1 && idRolLogueado != 3)
            {
                listaReclamos = listaReclamos.Where(t => t.IdUsuarioSolicitud == idLogueado).ToList();
            }

            // 4. Extraer solo los IDs de los reclamos filtrados
            var ids = listaReclamos.Select(r => r.IdReclamo).ToList();

            // 5. LLAMADA MASIVA: Traemos todos los estados de una sola vez
            // Usamos el nuevo método de negocio que creamos
            var estados = _negocioReclamos.ObtenerEstadosChatsReclamos(ids);

            // 6. Cruzar la información en memoria
            var resultado = listaReclamos.Select(r => {
                // Buscamos el estado correspondiente en la lista 'estados'
                var estado = estados.FirstOrDefault(e => e.IdReclamo == r.IdReclamo);

                return new
                {
                    id = r.IdReclamo,
                    // Si el estado existe y el ID es mayor a 0 (omitiendo el registro inicial si aplica)
                    ultimoIdMsg = estado?.UltimoId ?? 0,
                    idAutorUltimo = estado?.IdAutor ?? 0
                };
            }).ToList();

            return Json(resultado);
        }
        [Permiso("RECLAMOS_REPORTE")]
        public IActionResult ReportesReclamos()
        {
            // 1. Instanciar tu capa de negocio o llamar al método que trae la lista
            // Ajusta "CN_Reclamos" y "Listar" según los nombres reales de tus clases
            var lista = _negocioReclamos.Listar();

            // 2. Si la lista es null, enviamos una lista vacía para evitar el error
            if (lista == null)
            {
                lista = new List<Capa_Entidad.ReclamosBodega.E_ReclamosBodega>();
            }

            // 3. Pasar la lista a la vista
            return View(lista);
        }
        #endregion
    }
}
