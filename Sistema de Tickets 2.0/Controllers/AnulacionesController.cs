using Capa_Entidad.Anulaciones;
using Capa_Entidad.Canjes;
using Capa_Entidad.ReclamosBodega;
using Capa_Entidad.Tickets;
using Capa_Negocios.Anulaciones;
using Capa_Negocios.Tickets;
using Microsoft.AspNetCore.Mvc;
using Sistema_de_Tickets_2._0.Filter;

namespace Sistema_de_Tickets_2._0.Controllers
{
    public class AnulacionesController : Controller
    {
        #region INYECCION DE DEPENDENCIAS
        private readonly ILogger<AnulacionesController> _logger;
        private readonly CN_Usuarios _usuarioNegocio;
        private readonly CN_Estados _estadoNegocio;
        private readonly Capa_Negocios.Anulaciones.CN_TiposIncidenciasAnulaciones _Incidencias;
        private readonly Capa_Negocios.Anulaciones.CN_Archivo _archivos;
        private readonly Capa_Negocios.Anulaciones.CN_Comentarios _comentarios;
        private readonly Capa_Negocios.Anulaciones.CN_Anulaciones _anulaciones;
        private readonly Capa_Negocios.Tickets.CN_Cajas _cajas;





        // UN SOLO CONSTRUCTOR PARA TODO
        public AnulacionesController(ILogger<AnulacionesController> logger, CN_Usuarios usuarioNegocio, Capa_Negocios.Anulaciones.CN_TiposIncidenciasAnulaciones canjesNegocio, CN_TiposIncidenciasAnulaciones Incidencias, CN_Estados estadoNegocio,
            CN_Archivo archivos,Capa_Negocios.Anulaciones.CN_Comentarios comentarios,CN_Anulaciones anulaciones, CN_Cajas cajas
            )
        {
            _logger = logger;
            _usuarioNegocio = usuarioNegocio;      
            _Incidencias = Incidencias;
            _estadoNegocio = estadoNegocio;
            _archivos = archivos;
            _comentarios = comentarios;
            _anulaciones = anulaciones;
            _cajas = cajas;
        }
        #endregion
    
        #region TIPOS DE INCIDENCIAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GESTION DE TIPOS DE INCIDENCIAS DE RECLAMOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("INCIDENCIAS_ANULACIONES_VER")]
        public IActionResult TiposIncidenciasAnulaciones()
        {

            ViewBag.Estados = _estadoNegocio.ListarEstados();
            var lista = _Incidencias.Listar();
            return View(lista);
        }

        [Permiso("INCIDENCIAS_ANULACIONES_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardaIncidencias(E_TiposIncidenciasAnulaciones incidencias, string Accion)
        {
            bool resultado = _Incidencias.Guardar(incidencias, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE ANULACIONES

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE ANULACIONES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        [Permiso("ANULACIONES_VER")]
        public IActionResult Anulaciones()
        {
            ViewBag.Incidencias = _Incidencias.Listar();
            ViewBag.Estados = _estadoNegocio.ListarEstados();
            ViewBag.Cajas = _cajas.Listar();

            // Obtenemos datos de sesión
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;
            string nombreLogueado = HttpContext.Session.GetString("NombreUsuario") ?? "Usuario";

            // Pasamos a la vista para el JS
            ViewBag.IdUsuarioLogueado = idLogueado;
            ViewBag.RolUsuario = idRolLogueado;
            ViewBag.NombreUsuarioLogueado = nombreLogueado;

            var lista = _anulaciones.Listar();

            // LÓGICA DE FILTRADO SOLICITADA:
            // Solo Admin (1) y Soporte (3) ven todo. Los demás solo lo propio.
            if (idRolLogueado != 1 && idRolLogueado != 6)
            {
                lista = lista.Where(t => t.IdUsuarioSolicitud == idLogueado).ToList();
            }

            return View(lista);
        }
        //[Permiso("ANULACIONES_CREAR_EDITAR")]
        //[HttpPost]
        //public IActionResult GuardarAnulacion(E_Anulaciones anulacion, string Accion, List<IFormFile> archivos)
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
        //        anulacion.IdUsuarioSolicitud = idUsuarioSesion.Value;
        //    }

        //    // 3. Guardamos el Ticket (Recuerda que ahora el SP devuelve el ID generado en tickets.IdTicket)
        //    bool resultado = _anulaciones.Guardar(anulacion, Accion, out string mensaje);

        //    //4.Lógica de Archivos: Solo si el ticket se guardó bien y vienen archivos
        //    if (resultado && archivos != null && archivos.Count > 0)
        //    {
        //        try
        //        {
        //            // Ruta física: wwwroot/uploads/reclamos/
        //            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "anulacion");

        //            foreach (var file in archivos)
        //            {
        //                // Guardar el archivo físico usando tu Capa de Negocio de archivos
        //                string nombreSistema = _archivos.GuardarFisico(file, folderPath);

        //                // Registrar la referencia en la base de datos
        //                var entidadArchivo = new E_Archivos
        //                {
        //                    IdReferencia = anulacion.IdAnulacion, // El ID que devolvió el SP
        //                    NombreOriginal = file.FileName,
        //                    NombreSistema = nombreSistema,
        //                    Extension = Path.GetExtension(file.FileName),
        //                    Ruta = "/uploads/anulacion/" + nombreSistema
        //                };

        //                _archivos.RegistrarEnBaseDatos(entidadArchivo);
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
        [Permiso("ANULACIONES_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarAnulacion(E_Anulaciones anulacion, string Accion, List<IFormFile> archivos)
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
                anulacion.IdUsuarioSolicitud = idUsuarioSesion.Value;
            }

            // 3. Guardamos la Anulación
            bool resultado = _anulaciones.Guardar(anulacion, Accion, out string mensaje);

            // 4. Lógica de Archivos y Correo (Solo si se guardó bien)
            if (resultado)
            {
                // --- PROCESAMIENTO DE ARCHIVOS ---
                if (archivos != null && archivos.Count > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "anulacion");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        foreach (var file in archivos)
                        {
                            string nombreSistema = _archivos.GuardarFisico(file, folderPath);

                            var entidadArchivo = new E_Archivos
                            {
                                IdReferencia = anulacion.IdAnulacion,
                                NombreOriginal = file.FileName,
                                NombreSistema = nombreSistema,
                                Extension = Path.GetExtension(file.FileName),
                                Ruta = "/uploads/anulacion/" + nombreSistema
                            };

                            _archivos.RegistrarEnBaseDatos(entidadArchivo);
                        }
                    }
                    catch (Exception ex)
                    {
                        mensaje += " (Aviso: Se guardó pero hubo problemas con los archivos: " + ex.Message + ")";
                    }
                }

                // --- ENVÍO DE CORREO POR CIERRE DE ANULACIÓN ---
                // Verificamos si el estado es Cerrado/Finalizado (asumiendo ID 4)
                if (Accion == "UPDATE" && anulacion.IdEstado == 4)
                {
                    // CAPTURA de variables para el hilo secundario
                    string esquema = Request.Scheme;
                    string host = Request.Host.Value;
                    string urlBase = $"{esquema}://{host}";
                    int idAnulacionCapturado = anulacion.IdAnulacion;

                    Task.Run(() => {
                        try
                        {
                            // Obtenemos los detalles de la anulación para el correo
                            var infoAnulacion = _anulaciones.Listar().FirstOrDefault(a => a.IdAnulacion == idAnulacionCapturado);

                            if (infoAnulacion != null)
                            {
                                string correoDestino = _usuarioNegocio.ObtenerCorreoPorId(infoAnulacion.IdUsuarioSolicitud);

                                if (!string.IsNullOrEmpty(correoDestino))
                                {
                                    string asunto = $"Anulación Procesada: #{infoAnulacion.IdAnulacion}";

                                    string cuerpo = $@"
                            <div style='font-family: sans-serif; border: 1px solid #ddd; border-radius: 10px; padding: 20px; max-width: 600px;'>
                                <h2 style='color: #d35400; border-bottom: 2px solid #d35400; padding-bottom: 10px;'>Estado de Anulación</h2>
                                <p>Hola <strong>{infoAnulacion.UsuarioSolicitador}</strong>,</p>
                                <p>Se ha completado el proceso de anulación solicitado.</p>
                                
                                <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
                                                  <tr style='background: #f8f9fa;'>
                                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoAnulacion.UsuarioAsignado}</td>
                                                </tr>
                                  <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>N° Anulación:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>#{infoAnulacion.IdAnulacion}</td>
                                    </tr>
                                    <tr style='background: #f8f9fa;'>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Factura/Referencia:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{infoAnulacion.Incidencia}</td>
                                    </tr>
                                     <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee; font-weight: bold; color: #c0392b;'>{infoAnulacion.Resolucion}</td>
                                    </tr>
                                    <tr style='background: #f8f9fa;'>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Fecha de Cierre:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{DateTime.Now:dd/MM/yyyy hh:mm tt}</td>
                                    </tr>
                                </table>

                            
                                <p style='margin-top: 20px; font-size: 0.8em; color: #7f8c8d; text-align: center;'>
                                Farmacias Saba Nicaragua
                                </p>
                            </div>";

                                    CN_Recursos.EnviarCorreoInterno(correoDestino, asunto, cuerpo);
                                }
                            }
                        }
                        catch { /* Error silencioso en hilo secundario */ }
                    });
                }
            }

            return Json(new { success = resultado, mensaje = mensaje });
        }
        [HttpGet]
        public JsonResult ObtenerAdjuntos(int idAnulacion)
        {
            // Llamamos a la capa de negocios que a su vez llama a la de datos
            // El método Listar(idTicket) ya lo tienes definido en CN_Archivos
            var lista = _archivos.Listar(idAnulacion);

            // Retornamos la lista en formato JSON para que el JS la procese
            return Json(lista);
        }

        [HttpPost]
        public IActionResult TomarAnulacion(int idAnulacion)
        {
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuarioSesion == 0) return Json(new { success = false, mensaje = "Sesión expirada" });

            // Llamamos al método que asigna y cambia estado
            bool ok = _anulaciones.AsignarYProcesar(idAnulacion, idUsuarioSesion, out string mensaje);

            return Json(new { success = ok, mensaje = mensaje });
        }

        #endregion

        #region GESTIÓN DE COMENTARIOS (CHAT)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// GESTIÓN DE COMENTARIOS (CHAT)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public IActionResult GuardarComentario(int idAnulacion, string mensaje, IFormFile? archivo)
        {
            // 1. Recuperamos el ID del usuario desde la Sesión (siguiendo tu ejemplo)
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null)
            {
                return Json(new { success = false, mensaje = "La sesión ha expirado." });
            }

            try
            {
                var oComentario = new Capa_Entidad.Anulaciones.E_Comentarios
                {
                    IdAnulacion = idAnulacion,
                    IdUsuario = idUsuarioSesion.Value,
                    Mensaje = mensaje
                };

                // CAMBIO AQUÍ: Solo enviamos la ruta base del proyecto
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // La Capa de Negocio se encargará de entrar a uploads/comentarios
                bool resultado = _comentarios.RegistrarComentario(oComentario, archivo, webRootPath);

                return Json(new { success = resultado, mensaje = resultado ? "Enviado" : "Error" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult ObtenerComentarios(int idAnulacion)
        {
            // Obtenemos el ID de sesión para que el JS sepa cuáles mensajes son "míos"
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            var lista = _comentarios.Listar(idAnulacion);

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
            var lista = _anulaciones.Listar();
            // Sumamos los estados + el conteo. 
            // Si un ticket pasa de estado 3 a 5, el total cambia y el JS lo detecta.
            int sumaEstados = lista.Sum(t => t.IdEstado);
            int conteo = lista.Count;

            return $"{conteo}-{sumaEstados}";
        }
        [HttpGet]
        public JsonResult ObtenerUltimoComentarioId(int idAnulacion)
        {
            var lista = _comentarios.Listar(idAnulacion);
            int ultimoId = 0;
            if (lista != null && lista.Count > 0)
            {
                ultimoId = lista.Max(x => x.IdComentario);
            }
            return Json(new { ultimoId = ultimoId });
        }

        [HttpPost]
        public JsonResult ObtenerEstadosChats([FromBody] List<int> idsAnulaciones)
        {
            if (idsAnulaciones == null || !idsAnulaciones.Any())
                return Json(new List<Capa_Entidad.Anulaciones.ChatEstadoDTO>());

            var resultado = _anulaciones.ObtenerEstadosChats(idsAnulaciones);
            return Json(resultado);
        }

        #endregion

        #region UTILIDADES
    
        [Permiso("ANULACIONES_VER")]
        [HttpGet]
        public JsonResult ObtenerEstadoGlobalAnulaciones()
        {
            // 1. Recuperar datos de sesión para filtrar (ID y Rol)
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

            // 2. Obtener la lista base completa de anulaciones desde la capa de negocio
            var listaBase = _anulaciones.Listar();

            // 3. APLICAR FILTRO DE PRIVACIDAD (Igual que en tu vista principal)
            // Suponiendo que Rol 1 es Admin y Rol 3 o 6 es Soporte/Staff
            if (idRolLogueado != 1 && idRolLogueado != 3 && idRolLogueado != 6)
            {
                listaBase = listaBase.Where(a => a.IdUsuarioSolicitud == idLogueado).ToList();
            }

            // 4. EXTRAER IDs Y CONSULTAR EN BLOQUE (Optimización Crítica)
            // Extraemos solo los IDs de las anulaciones que pasaron el filtro
            var ids = listaBase.Select(a => a.IdAnulacion).ToList();

            // LLAMADA ÚNICA: Traemos los estados de todos los chats de una sola vez
            var estados = _anulaciones.ObtenerEstadosChats(ids);

            // 5. MAPEAR EL RESULTADO FINAL
            // Cruzamos la listaBase con los estados obtenidos en memoria
            var resultado = listaBase.Select(a => {
                // Buscamos en la lista de estados el que corresponda a esta anulación
                var est = estados.FirstOrDefault(e => e.IdAnulacion == a.IdAnulacion);

                return new
                {
                    id = a.IdAnulacion,
                    // Si est es null (no hay comentarios), devolvemos 0
                    ultimoIdMsg = est?.UltimoId ?? 0,
                    idAutorUltimo = est?.IdAutor ?? 0
                };
            }).ToList();

            return Json(resultado);
        }


        [Permiso("ANULACIONES_REPORTE")]
        public IActionResult ReporteAnulaciones()
        {
            // 1. Instanciar tu capa de negocio o llamar al método que trae la lista
            // Ajusta "CN_Reclamos" y "Listar" según los nombres reales de tus clases
            var lista = _anulaciones.Listar();

            // 2. Si la lista es null, enviamos una lista vacía para evitar el error
            if (lista == null)
            {
                lista = new List<Capa_Entidad.Anulaciones.E_Anulaciones>();
            }

            // 3. Pasar la lista a la vista
            return View(lista);
        }

        #endregion
    }
}
