using Capa_Datos.Canjes;
using Capa_Entidad.Canjes;
using Capa_Entidad.ReclamosBodega;
using Capa_Entidad.Tickets;
using Capa_Negocios.Canjes;
using Capa_Negocios.Tickets;
using Microsoft.AspNetCore.Mvc;
using Sistema_de_Tickets_2._0.Filter;
using System;

namespace Sistema_de_Tickets_2._0.Controllers
{
    public class CanjesController : Controller
    {
        #region INYECCION DE DEPENDENCIAS
        private readonly ILogger<CanjesController> _logger;
        private readonly CN_Usuarios _usuarioNegocio;
        private readonly CN_Estados _estadoNegocio;
        private readonly Capa_Negocios.Canjes.CN_AsignacionCanjes _canjesNegocio;
        private readonly Capa_Negocios.Canjes.CN_TiposIncidenciasCanjes _canjesIncidencias;
        private readonly Capa_Negocios.Canjes.CN_Canjes _canjes;
        private readonly Capa_Negocios.Canjes.CN_Archivos _archivos;
        private readonly Capa_Negocios.Canjes.CN_Comentarios _comentarios;
        private readonly Capa_Negocios.Tickets.CN_Roles _roles;




        // UN SOLO CONSTRUCTOR PARA TODO
        public CanjesController(ILogger<CanjesController> logger, CN_Usuarios usuarioNegocio,Capa_Negocios.Canjes.CN_AsignacionCanjes canjesNegocio,CN_TiposIncidenciasCanjes canjesIncidencias,CN_Estados estadoNegocio, CN_Canjes canjes,
            Capa_Negocios.Canjes.CN_Archivos archivos,Capa_Negocios.Canjes.CN_Comentarios comentarios,Capa_Negocios.Tickets.CN_Roles roles
            )
        {
            _logger = logger;
            _usuarioNegocio = usuarioNegocio;
            _canjesNegocio=canjesNegocio;
            _canjesIncidencias=canjesIncidencias;
            _estadoNegocio=estadoNegocio;
            _canjes=canjes;
            _archivos=archivos;
            _comentarios=comentarios;
            _roles = roles;
     

        }
        #endregion

        #region ASIGNACIONES DE SUCURSALES A AUDITOR DE CANJES
        //[Permiso("ASIGNACIONES_VER")]
        //public IActionResult AsignacionSucursales()
        //{
        //    // Llenamos combos para la vista
        //    ViewBag.Operador = _usuarioNegocio.Listar(); 
        //    ViewBag.Solicitantes = _usuarioNegocio.Listar();
        //    return View();
        //}
        [Permiso("ASIGNACIONES_VER")]
        public IActionResult AsignacionSucursales()
        {
            // Filtramos para que no aparezca "Farmacias Saba" en la lista de Técnicos (Operadores)
            var usuarios = _usuarioNegocio.Listar()
                .Where(u => !(u.Nombres.Contains("Farmacias Saba") || u.Apellidos.Contains("Farmacias Saba")))
                .ToList();

            ViewBag.Operador = usuarios;
            ViewBag.Solicitantes = _usuarioNegocio.Listar();
            return View();
        }

        [HttpGet]
        public JsonResult ListarAgrupado()
        {
            var lista = _canjesNegocio.Listar();
            // Agrupamos por técnico de canjes
            var agrupado = lista.GroupBy(x => new { x.IdUsuarioCanjes, x.NombreUsuarioCanjes })
                .Select(g => new
                {
                    idTecnico = g.Key.IdUsuarioCanjes,
                    nombreTecnico = g.Key.NombreUsuarioCanjes,
                    cantidad = g.Count()
                }).ToList();

            return Json(agrupado);
        }

        [HttpGet]
        public JsonResult ObtenerDetalleSucursales(int idTecnico)
        {
            var detalle = _canjesNegocio.Listar()
                .Where(x => x.IdUsuarioCanjes == idTecnico)
                .Select(x => new {
                    idAsignacion = x.IdAsignacion, // Asegúrate que el nombre de propiedad coincida con tu E_AsignacionCanjes
                    sucursal = x.NombreUsuarioSolicitante,
                    fecha = x.FechaAsignacion.ToString("dd/MM/yyyy HH:mm")
                }).ToList();
            return Json(detalle);
        }

        [HttpGet]
        public JsonResult ObtenerAsignadosPorTecnico(int idTecnico)
        {
            var ids = _canjesNegocio.Listar()
                .Where(x => x.IdUsuarioCanjes == idTecnico)
                .Select(x => x.IdUsuarioSolicitante)
                .ToList();
            return Json(new { success = true, ids = ids });
        }
        [HttpGet]
        public JsonResult Listar() => Json(_canjesNegocio.Listar());

     
        [HttpPost]
        public JsonResult GuardarMultiple(int idTecnico, List<int> idsSolicitantes)
        {
            int procesados = 0;
            int errores = 0;

            foreach (var idSolicitante in idsSolicitantes)
            {
                var obj = new E_AsignacionCanjes
                {
                    IdUsuarioCanjes = idTecnico,
                    IdUsuarioSolicitante = idSolicitante
                };

                bool ok = _canjesNegocio.Registrar(obj, out _);
                if (ok) procesados++; else errores++;
            }

            return Json(new
            {
                success = procesados > 0,
                mensaje = $"Se asignaron {procesados} usuarios correctamente." + (errores > 0 ? $" {errores} ya estaban asignados." : "")
            });
        }
        [HttpPost]
        public JsonResult Eliminar(int id) => Json(new { success = _canjesNegocio.Eliminar(id) });
        #endregion

        #region TIPOS DE INCIDENCIAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GESTION DE TIPOS DE INCIDENCIAS DE RECLAMOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("INCIDENCIAS_CANJES_VER")]
        public IActionResult TiposIncidenciasCanjes()
        {

            ViewBag.Estados = _estadoNegocio.ListarEstados();
            var lista = _canjesIncidencias.Listar();
            return View(lista);
        }

        [Permiso("INCIDENCIAS_CANJES_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardaIncidenciasCanjes(E_TiposIncidenciasCanjes incidencias, string Accion)
        {
            bool resultado = _canjesIncidencias.Guardar(incidencias, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region  VISTA Y CRUD DE CANJES
        [HttpGet]
        public JsonResult ObtenerCanjesTabla()
        {
            int idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRol = HttpContext.Session.GetInt32("IdRol") ?? 0;

            var lista = _canjes.Listar(idUsuarioLogueado, idRol);

            // Mantenemos tu lógica de filtrado para el Rol 6
            if (idRol == 6)
            {
                lista = lista.Where(c => c.IdEstado == 6 || c.IdEstado == 7).ToList();
            }

            return Json(lista);
        }
        [Permiso("CANJES_VER")]
        public IActionResult Canjes()
        {
            //ViewBag.Incidencias = _canjesIncidencias.Listar();

            //int idUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            //int idRol = HttpContext.Session.GetInt32("IdRol") ?? 0;

            //ViewBag.IdUsuarioLogueado = idUsuarioLogueado;
            //ViewBag.RolUsuario = idRol;
            //ViewBag.NombreUsuarioLogueado = HttpContext.Session.GetString("NombreUsuario") ?? "Usuario";

            //// 1. Obtenemos la lista base
            //var lista = _canjes.Listar(idUsuarioLogueado, idRol);

            //// 2. Si el rol es 6, filtramos la lista en memoria
            //if (idRol == 6)
            //{
            //    // Solo permitimos IdEstado 6 (Autorizado) y 7 (Anulado)
            //    lista = lista.Where(c => c.IdEstado == 6 || c.IdEstado == 7).ToList();
            //}

            //return View(lista);
            // Solo cargamos catálogos y datos de sesión para la UI
            ViewBag.Incidencias = _canjesIncidencias.Listar();
            ViewBag.IdUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            ViewBag.RolUsuario = HttpContext.Session.GetInt32("IdRol") ?? 0;
            ViewBag.NombreUsuarioLogueado = HttpContext.Session.GetString("NombreUsuario") ?? "Usuario";

            return View(new List<E_Canje>()); // Vista vacía, se llena por AJAX
        }
        // --- MÉTODO PARA GUARDAR (INSERT/UPDATE) ---

        //[HttpPost]
        //public JsonResult GuardarCanje(IFormCollection form)
        //{
        //    string mensaje = string.Empty;
        //    int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

        //    if (idUsuarioSesion == null) return Json(new { success = false, mensaje = "Sesión expirada." });

        //    E_Canje obj = new E_Canje();
        //    obj.IdCanje = int.Parse(form["canjes.IdCanje"]);
        //    obj.IdTipoIncidenciaCanjes = int.Parse(form["canjes.IdTipoIncidencia"]);
        //    obj.Resolucion = form["canjes.Resolucion"];
        //    obj.IdEstado = string.IsNullOrEmpty(form["canjes.IdEstado"]) ? 0 : int.Parse(form["canjes.IdEstado"]);
        //    // --- NUEVO MAPEO: Capturamos la descripción del problema ---
        //    obj.DescripcionProblema = form["canjes.DescripcionProblema"];
        //    if (obj.IdCanje == 0)
        //    {
        //        obj.IdUsuarioSolicitador = idUsuarioSesion.Value;
        //    }
        //    else
        //    {
        //        // --- NUEVO AJUSTE: Si el estado es 5 (En Proceso), se lo asignamos al usuario actual ---
        //        if (obj.IdEstado == 5)
        //        {
        //            obj.IdUsuarioAsignado = idUsuarioSesion.Value;
        //        }

        //        // Si el estado es 6 (Autorizado), asignamos autorizador
        //        if (obj.IdEstado == 6)
        //        {
        //            obj.IdUsuarioAutorizador = idUsuarioSesion.Value;
        //            // Opcional: Si quieres asegurar que el asignado se mantenga o se actualice
        //            obj.IdUsuarioAsignado = idUsuarioSesion.Value;
        //        }
        //        // Si el estado es 7 (Anulado)
        //        else if (obj.IdEstado == 7)
        //        {
        //            obj.IdUsuarioAnulador = idUsuarioSesion.Value;
        //        }
        //    }

        //    bool success = _canjes.GuardarCanje(obj, out mensaje);
        //    return Json(new { success = success, mensaje = mensaje });
        //}


        [HttpGet]
        public JsonResult ObtenerAdjuntos(int idCanjes)
        {
            // Llamamos a la capa de negocios que a su vez llama a la de datos
            // El método Listar(idTicket) ya lo tienes definido en CN_Archivos
            var lista = _archivos.Listar(idCanjes);

            // Retornamos la lista en formato JSON para que el JS la procese
            return Json(lista);
        }
        [HttpPost]
        public JsonResult GuardarCanje(IFormCollection form)
        {
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;
            string mensaje = string.Empty;
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null) return Json(new { success = false, mensaje = "Sesión expirada." });

            E_Canje obj = new E_Canje();
            obj.IdCanje = int.Parse(form["canjes.IdCanje"]);
            obj.IdTipoIncidenciaCanjes = int.Parse(form["canjes.IdTipoIncidencia"]);
            obj.Resolucion = form["canjes.Resolucion"];
            obj.IdEstado = string.IsNullOrEmpty(form["canjes.IdEstado"]) ? 0 : int.Parse(form["canjes.IdEstado"]);
            obj.DescripcionProblema = form["canjes.DescripcionProblema"];

            if (obj.IdCanje == 0)
            {
                obj.IdUsuarioSolicitador = idUsuarioSesion.Value;
            }
            else
            {
                if (obj.IdEstado == 5) obj.IdUsuarioAsignado = idUsuarioSesion.Value;

                if (obj.IdEstado == 6) // Autorizado
                {
                    obj.IdUsuarioAutorizador = idUsuarioSesion.Value;
                    obj.IdUsuarioAsignado = idUsuarioSesion.Value;
                }
                else if (obj.IdEstado == 7) // Anulado/Rechazado
                {
                    obj.IdUsuarioAnulador = idUsuarioSesion.Value;
                }
            }

            bool success = _canjes.GuardarCanje(obj, out mensaje);

            // --- LÓGICA DE CORREO INTEGRADA ---
            if (success && (obj.IdEstado == 6 || obj.IdEstado == 7))
            {
                // CAPTURA de variables del Request
                string esquema = Request.Scheme;
                string host = Request.Host.Value;
                string urlBase = $"{esquema}://{host}";
                int idCanjeCapturado = obj.IdCanje;
                int estadoFinal = obj.IdEstado;

                Task.Run(() => {
                    try
                    {
                        // Obtenemos info completa del canje
                        var infoCanje = _canjes.Listar(idLogueado, idRolLogueado).FirstOrDefault(c => c.IdCanje == idCanjeCapturado);

                        if (infoCanje != null)
                        {
                            string correoDestino = _usuarioNegocio.ObtenerCorreoPorId(infoCanje.IdUsuarioSolicitador);

                            if (!string.IsNullOrEmpty(correoDestino))
                            {
                                string statusText = (estadoFinal == 6) ? "AUTORIZADO" : "ANULADO";
                                string colorStatus = (estadoFinal == 6) ? "#27ae60" : "#c0392b";
                                string asunto = $"Canje #{infoCanje.IdCanje} - {statusText}";

                                string cuerpo = $@"
                        <div style='font-family: sans-serif; border: 1px solid #ddd; border-radius: 10px; padding: 20px; max-width: 600px;'>
                            <h2 style='color: #2c3e50; border-bottom: 2px solid {colorStatus}; padding-bottom: 10px;'>Estado de Solicitud de Canje</h2>
                            <p>Hola <strong>{infoCanje.NombreSolicitador}</strong>,</p>
                            <p>Tu solicitud de canje ha sido procesada con el siguiente resultado:</p>
                            
                            <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>

                                <tr style='background: #f8f9fa;'>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>N° de Regsitro:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>#{infoCanje.IdCanje}</td>
                                </tr>
                                 <tr>
                                       <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                       <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.NombreAsignado}</td>
             
                                <tr>
                                </tr>
                                   <tr>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Fecha de Registro:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.FechaRegistro}</td>
                                </tr>
                                 <tr>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Descripcion del Problema:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.DescripcionProblema}</td>
                                </tr>
                                <tr style='background: #f8f9fa;'>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.Resolucion}</td>
                               </tr>
                              <tr style='background: #f8f9fa;'>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.NombreAutorizador}</td>
                               </tr>
                                 <tr style='background: #f8f9fa;'>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.FechaAutorizado}</td>
                               </tr>
                               </tr>
                                 <tr style='background: #f8f9fa;'>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.NombreAnulador}</td>
                               </tr>
                               <tr style='background: #f8f9fa;'>
                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Resolución:</strong></td>
                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoCanje.FechaAnulado}</td>
                               </tr>
                            </table>

                            <p style='margin-top: 20px; font-size: 0.8em; color: #7f8c8d; text-align: center;'>
                                Gestión de Canjes - Farmacias Saba Nicaragua
                            </p>
                        </div>";

                                CN_Recursos.EnviarCorreoInterno(correoDestino, asunto, cuerpo);
                            }
                        }
                    }
                    catch { /* No interrumpir el flujo principal */ }
                });
            }

            return Json(new { success = success, mensaje = mensaje });
        }
        #endregion

        #region GESTIÓN DE COMENTARIOS (CHAT)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        ///// GESTIÓN DE COMENTARIOS (CHAT)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public IActionResult GuardarComentario(int idCanje, string mensaje, IFormFile? archivo)
        {
            // 1. Recuperamos el ID del usuario desde la Sesión (siguiendo tu ejemplo)
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null)
            {
                return Json(new { success = false, mensaje = "La sesión ha expirado." });
            }

            try
            {
                var oComentario = new Capa_Entidad.Canjes.E_Comentarios
                {
                    IdCanje = idCanje,
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
        public JsonResult ObtenerComentarios(int idCanje)
        {
            // Obtenemos el ID de sesión para que el JS sepa cuáles mensajes son "míos"
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            var lista = _comentarios.Listar(idCanje);

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
        public string ObtenerVersionCanjes(int idUsuarioLogueado,int idRol)
        {
            var lista = _canjes.Listar(idUsuarioLogueado,idRol);
            // Sumamos los estados + el conteo. 
            // Si un ticket pasa de estado 3 a 5, el total cambia y el JS lo detecta.
            int sumaEstados = lista.Sum(t => t.IdEstado);
            int conteo = lista.Count;

            return $"{conteo}-{sumaEstados}";
        }
        [HttpGet]
        public JsonResult ObtenerUltimoComentarioId(int idCanje)
        {
            var lista = _comentarios.Listar(idCanje);
            int ultimoId = 0;
            if (lista != null && lista.Count > 0)
            {
                ultimoId = lista.Max(x => x.IdComentario);
            }
            return Json(new { ultimoId = ultimoId });
        }

    
        [HttpPost]
        public JsonResult ObtenerEstadosChats([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new List<Capa_Entidad.Canjes.ChatEstadoDTO>());

            var resultado = _canjes.ObtenerEstadosChats(ids);
            return Json(resultado);
        }
        #endregion

        #region UTILIDADES
     
        [Permiso("CANJES_VER")]
        [HttpGet]
        public JsonResult ObtenerEstadoGlobalCanjes()
        {
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRol = HttpContext.Session.GetInt32("IdRol") ?? 0;

            // 1. Obtener la lista base filtrada
            var listaCanjes = _canjes.Listar(idLogueado, idRol);
            if (idRol == 6)
            {
                listaCanjes = listaCanjes.Where(c => c.IdEstado == 6 || c.IdEstado == 7).ToList();
            }

            // 2. Extraer solo los IDs
            var ids = listaCanjes.Select(c => c.IdCanje).ToList();

            // 3. Obtener todos los estados de una sola vez
            var estados = _canjes.ObtenerEstadosChats(ids);

            // 4. Cruzar información en memoria
            var resultado = listaCanjes.Select(c => {
                var estado = estados.FirstOrDefault(e => e.IdCanje == c.IdCanje);
                return new
                {
                    id = c.IdCanje,
                    ultimoIdMsg = estado?.UltimoId ?? 0,
                    idAutorUltimo = estado?.IdAutor ?? 0
                };
            }).ToList();

            return Json(resultado);
        }
        [Permiso("CANJES_REPORTE")]
        public IActionResult ReportesCanjes()
        {
            // 1. Instanciar tu capa de negocio o llamar al método que trae la lista
            // Ajusta "CN_Reclamos" y "Listar" según los nombres reales de tus clases
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;
            var lista = _canjes.Listar(idLogueado, idRolLogueado);

            // 2. Si la lista es null, enviamos una lista vacía para evitar el error
            if (lista == null)
            {
                lista = new List<Capa_Entidad.Canjes.E_Canje>();
            }

            // 3. Pasar la lista a la vista
            return View(lista);
        }
        #endregion
    }
}
