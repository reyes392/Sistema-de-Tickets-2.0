using Capa_Entidad.Auditoria_Z;
using Capa_Entidad.Tickets;
using Capa_Negocios.Auditoria_Z;
using Capa_Negocios.AuditoriaZ;
using Capa_Negocios.Tickets;
using Microsoft.AspNetCore.Mvc;
using Sistema_de_Tickets_2._0.Filter;

namespace Sistema_de_Tickets_2._0.Controllers
{
    public class AuditoriaZController : Controller
    {

        #region INYECCION DE DEPENDENCIAS
        private readonly ILogger<AuditoriaZController> _logger;
        private readonly CN_Usuarios _usuario;
        private readonly CN_Estados _estado;
        private readonly CN_TiposIncidenciasAuditoriaZ _incidencia;
        private readonly CN_AsignacionZ _asignacion;
        // Dependencias adicionales para la gestión de Auditoría
        private readonly CN_AuditoriaZ _negocioAuditoria;
        private readonly Capa_Negocios.Auditoria_Z.CN_Archivos _negocioArchivos;
        private readonly Capa_Negocios.Auditoria_Z.CN_Comentarios _negocioComentarios;






        // UN SOLO CONSTRUCTOR PARA TODO
        public AuditoriaZController(ILogger<AuditoriaZController> logger, CN_Usuarios usuario,CN_Estados estado,CN_TiposIncidenciasAuditoriaZ incidencia, CN_AsignacionZ asignacion, CN_AuditoriaZ negocioAuditoria,
            Capa_Negocios.Auditoria_Z.CN_Archivos negocioArchivos,
            Capa_Negocios.Auditoria_Z.CN_Comentarios negocioComentarios)
        {
            _logger = (ILogger<AuditoriaZController>?)logger;
            _usuario = usuario;
            _estado = estado;
            _incidencia = incidencia;
            _asignacion = asignacion;

            // ASIGNACIONES FALTANTES:
            _negocioAuditoria = negocioAuditoria;
            _negocioArchivos = negocioArchivos;
            _negocioComentarios = negocioComentarios;

        }
        #endregion

        #region ASIGNACIONES DE SUCURSALES A AUDITOR Z
        //[Permiso("ASIGNACIONES_VER")]
        //public IActionResult AsignacionSucursales_AuditoriaZ()
        //{
        //    // Llenamos combos para la vista
        //    ViewBag.Operador = _usuario.Listar();
        //    ViewBag.Solicitantes = _usuario.Listar();
        //    return View();
        //}
        [Permiso("ASIGNACIONES_VER")]
        public IActionResult AsignacionSucursales_AuditoriaZ()
        {
            // Filtramos para que no aparezca "Farmacias Saba" en la lista de Técnicos
            var usuarios = _usuario.Listar()
                .Where(u => !(u.Nombres.Contains("Farmacias Saba") || u.Apellidos.Contains("Farmacias Saba")))
                .ToList();

            ViewBag.Operador = usuarios;
            ViewBag.Solicitantes = _usuario.Listar(); // Aquí puedes decidir si filtrar también o no
            return View();
        }

        [HttpGet]
        public JsonResult ListarAgrupado()
        {
            var lista = _asignacion.Listar();
            // Agrupamos por técnico para que en la tabla principal solo salga una fila por persona
            var agrupado = lista.GroupBy(x => new { x.IdUsuarioAuditoriaZ, x.NombreUsuarioAuditoriaZ })
                .Select(g => new
                {
                    idTecnico = g.Key.IdUsuarioAuditoriaZ,
                    nombreTecnico = g.Key.NombreUsuarioAuditoriaZ,
                    cantidad = g.Count() // Opcional: para mostrar cuántas tiene
                }).ToList();

            return Json(agrupado);
        }

        [HttpGet]
        public JsonResult ObtenerDetalleSucursales(int idTecnico)
        {
            var detalle = _asignacion.Listar()
                .Where(x => x.IdUsuarioAuditoriaZ == idTecnico)
                .Select(x => new {
                    idAsignacion = x.IdAsignacionZ,
                    sucursal = x.NombreUsuarioSolicitante,
                    fecha = x.FechaAsignacion.ToString("dd/MM/yyyy HH:mm")
                }).ToList();
            return Json(detalle);
        }
        [HttpGet]
        public JsonResult Listar() => Json(_asignacion.Listar());


        [HttpPost]
        public JsonResult GuardarMultiple(int idTecnico, List<int> idsSolicitantes)
        {
            int procesados = 0;
            int errores = 0;

            foreach (var idSolicitante in idsSolicitantes)
            {
                var obj = new E_Asignacion_Z
                {
                    IdUsuarioAuditoriaZ = idTecnico,
                    IdUsuarioSolicitante = idSolicitante
                };

                bool ok = _asignacion.Registrar(obj, out _);
                if (ok) procesados++; else errores++;
            }

            return Json(new
            {
                success = procesados > 0,
                mensaje = $"Se asignaron {procesados} usuarios correctamente." + (errores > 0 ? $" {errores} ya estaban asignados." : "")
            });
        }
        [HttpPost]
        public JsonResult Eliminar(int id) => Json(new { success = _asignacion.Eliminar(id) });
        #endregion

        #region TIPOS DE INCIDENCIAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GESTION DE TIPOS DE INCIDENCIAS DE RECLAMOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("INCIDENCIAS_AUDITORIAZ_VER")]
        public IActionResult TiposIncidenciasAuditoriaZ()
        {

            ViewBag.Estados = _estado.ListarEstados();
            var lista = _incidencia.Listar();
            return View(lista);
        }

        [Permiso("INCIDENCIAS_AUDITORIAZ_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardaIncidenciasAuditoriaZ(E_TiposIncidenciasAuditoriaZ incidencias, string Accion)
        {
            bool resultado = _incidencia.Guardar(incidencias, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTIÓN DE AUDITORÍA Z
        [HttpGet]
        public JsonResult ObtenerAuditoriasTabla()
        {
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

            // Tu lógica de negocio ya tiene el filtro inteligente
            var lista = _negocioAuditoria.Listar(idLogueado, idRolLogueado);

            return Json(lista);
        }


        [Permiso("AUDITORIAZ_VER")]
     
        public IActionResult AuditoriaZ()
        {
            //int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            //int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

            //// Pasamos los datos a la vista para validaciones de UI
            //ViewBag.IdUsuarioLogueado = idLogueado;
            //ViewBag.RolUsuario = idRolLogueado;
            //ViewBag.Incidencias = _incidencia.Listar();

            //// Filtro inteligente: 
            //// Si es Administrador (1) o Gerencia (5), ve TODO.
            //// Si es Auditor de Z (8), ve lo que tiene asignado.
            //// Otros (Farmacia, etc.), ven solo lo que ellos solicitaron.
            //var lista = _negocioAuditoria.Listar(idLogueado, idRolLogueado);

            //return View(lista);
            // Solo cargamos datos para modales y UI
            ViewBag.IdUsuarioLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            ViewBag.RolUsuario = HttpContext.Session.GetInt32("IdRol") ?? 0;
            ViewBag.Incidencias = _incidencia.Listar();
            ViewBag.NombreUsuarioLogueado = HttpContext.Session.GetString("NombreUsuario") ?? "Usuario";

            // Retorna vista con lista vacía (AJAX hará el trabajo)
            return View(new List<E_AuditoriaZ>());
        }

        [Permiso("AUDITORIAZ_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarAuditoria(E_AuditoriaZ objeto, string Accion, List<IFormFile> archivos)
        {
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");
            if (idUsuarioSesion == null) return Json(new { success = false, mensaje = "Sesión expirada" });
          
            if (Accion == "INSERT")
            {
               
                objeto.IdUsuarioSolicitador = idUsuarioSesion.Value;
                objeto.IdUsuarioModificador = idUsuarioSesion.Value;
                objeto.IdEstado = 3; // Supongo que 3 es 'Pendiente' o 'Abierto'
            }
            else if (Accion == "UPDATE")
            {
                if (objeto.IdUsuarioSolicitador == 0)
                {
                    objeto.IdUsuarioSolicitador = idUsuarioSesion.Value;
                }
            }

            // Llama a la capa de negocio
            bool resultado = _negocioAuditoria.Guardar(objeto, out string mensaje);

            if (resultado)
            {
                // --- 1. LÓGICA DE ARCHIVOS ---
                if (archivos != null && archivos.Count > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "auditoriaZ");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        foreach (var file in archivos)
                        {
                            string nombreSistema = _negocioAuditoria.GuardarArchivoFisico(file, folderPath);
                            var entidadArchivo = new E_Archivos
                            {
                                IdReferencia = objeto.IdAuditoriaZ,
                                NombreOriginal = file.FileName,
                                NombreSistema = nombreSistema,
                                Extension = Path.GetExtension(file.FileName),
                                Ruta = "/uploads/auditoriaZ/" + nombreSistema
                            };
                            _negocioArchivos.RegistrarEnBaseDatos(entidadArchivo);
                        }
                    }
                    catch (Exception ex)
                    {
                        mensaje += " (Aviso: Se guardó pero hubo problemas con archivos: " + ex.Message + ")";
                    }
                }

                // --- 2. LÓGICA DE CORREO (Solo al CERRAR / Estado 4) ---
                if (Accion == "UPDATE" && objeto.IdEstado == 4)
                {
                    // CAPTURA de variables críticas antes de salir del hilo principal
                    string esquema = Request.Scheme;
                    string host = Request.Host.Value;
                    string urlBase = $"{esquema}://{host}";
                    int idAuditoriaCapturado = objeto.IdAuditoriaZ;

                    Task.Run(() => {
                        try
                        {
                            // Consultamos la información completa para el correo
                            var infoAuditoria = _negocioAuditoria.Listar(idLogueado, idRolLogueado).FirstOrDefault(a => a.IdAuditoriaZ == idAuditoriaCapturado);

                            if (infoAuditoria != null)
                            {
                                string correoDestino = _usuario.ObtenerCorreoPorId(infoAuditoria.IdUsuarioSolicitador);

                                if (!string.IsNullOrEmpty(correoDestino))
                                {
                                    string asunto = $"Auditoría Z Finalizada: #{infoAuditoria.IdAuditoriaZ}";

                                    string cuerpo = $@"
                            <div style='font-family: sans-serif; border: 1px solid #ddd; border-radius: 10px; padding: 20px; max-width: 600px;'>
                                <h2 style='color: #2c3e50; border-bottom: 2px solid #2c3e50; padding-bottom: 10px;'>Resultado de Auditoría Z</h2>
                                <p>Estimado/a <strong>{infoAuditoria.NombreSolicitador}</strong>,</p>
                                <p>Se ha completado el proceso de revisión de Auditoría Z.</p>
                                
                                <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
                                     <tr style='background: #f8f9fa;'>
                                                    <td style='padding: 8px; border: 1px solid #eee;'><strong>Atendido por:</strong></td>
                                                    <td style='padding: 8px; border: 1px solid #eee;'>{infoAuditoria.NombreAsignado}</td>
                                                </tr>
                                   <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Descripcion del Problema:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>#{infoAuditoria.DescripcionProblema}</td>
                                    </tr>
                                   <tr style='background: #f8f9fa;'>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Fecha Auditoría:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee;'>{infoAuditoria.FechaRegistro:dd/MM/yyyy}</td>
                                    </tr>

                                   <tr>
                                        <td style='padding: 8px; border: 1px solid #eee;'><strong>Observación Final /Resolucion:</strong></td>
                                        <td style='padding: 8px; border: 1px solid #eee; font-weight: bold; color: #16a085;'>{infoAuditoria.ResolucionProblema}</td>
                                    </tr>
                                </table>

                          
                                <p style='margin-top: 20px; font-size: 0.8em; color: #7f8c8d; text-align: center;'>
                                    Departamento de Auditoría - Farmacias Saba Nicaragua
                                </p>
                            </div>";

                                    CN_Recursos.EnviarCorreoInterno(correoDestino, asunto, cuerpo);
                                }
                            }
                        }
                        catch { /* Error silencioso */ }
                    });
                }
            }

            return Json(new { success = resultado, mensaje = mensaje });
        }
        //[HttpGet]
        //public IActionResult ObtenerVersionAuditoria()
        //{
        //    try
        //    {
        //        // 1. Obtenemos la lista desde la capa de negocio
        //        var lista = _negocioAuditoria.Listar(0, 0);

        //        // 2. Manejamos el caso de que la lista esté vacía para evitar errores en .Max()
        //        if (lista == null || !lista.Any())
        //        {
        //            return Json("0-0");
        //        }

        //        // 3. Creamos un "sello" único (Cantidad de registros + ID más alto)
        //        // Si tienes un campo FechaModificacion, sería incluso mejor usar el Max de esa fecha.
        //        string version = $"{lista.Count}-{lista.Max(x => x.IdAuditoriaZ)}";

        //        // En .NET Core, Json() permite GET por defecto
        //        return Json(version);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Retornamos un status 500 o simplemente un string de error para no romper el JS
        //        return StatusCode(500, "Error al obtener versión");
        //    }
        //}
        [HttpGet]
        public IActionResult ObtenerVersionAuditoria()
        {
            try
            {
                // 1. IMPORTANTE: Recuperar la sesión para que el filtro sea el mismo que en el Index
                int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
                int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

                // 2. CORRECCIÓN: Pasar los IDs reales, no (0, 0)
                var lista = _negocioAuditoria.Listar(idLogueado, idRolLogueado);

                if (lista == null || !lista.Any())
                {
                    return Json("0-0");
                }

                string version = $"{lista.Count}-{lista.Max(x => x.IdAuditoriaZ)}";
                return Json(version);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al obtener versión");
            }
        }
        [HttpPost]
        public IActionResult TomarAuditoria(int idAuditoriaZ)
        {
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRol = HttpContext.Session.GetInt32("IdRol") ?? 0;
            if (idUsuarioSesion == 0) return Json(new { success = false, mensaje = "Sesión expirada" });
            if (idRol != 8)
            {
                // Retornamos éxito falso para que el JS no actualice la UI como si se hubiera asignado
                return Json(new { success = false, mensaje = "Solo personal de bodega puede tomar reclamos." });
            }
            bool ok = _negocioAuditoria.AsignarYProcesar(idAuditoriaZ, idUsuarioSesion, out string mensaje);
            return Json(new { success = ok, mensaje = mensaje });
        }

        [HttpGet]
        public JsonResult ObtenerAdjuntos(int idAuditoriaZ) => Json(_negocioArchivos.Listar(idAuditoriaZ));
        #endregion

        #region GESTIÓN DE COMENTARIOS (CHAT)
        [HttpPost]
        public IActionResult GuardarComentario(int idAuditoriaZ, string mensaje, IFormFile? archivo)
        {
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");
            if (idUsuarioSesion == null) return Json(new { success = false, mensaje = "Sesión expirada." });

            try
            {
                var oComentario = new Capa_Entidad.Auditoria_Z.E_Comentarios
                {
                    IdAuditoriaZ = idAuditoriaZ,
                    IdUsuario = idUsuarioSesion.Value,
                    Mensaje = mensaje
                };
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                bool resultado = _negocioComentarios.RegistrarComentario(oComentario, archivo, webRootPath);
                return Json(new { success = resultado, mensaje = resultado ? "Enviado" : "Error" });
            }
            catch (Exception ex) { return Json(new { success = false, mensaje = ex.Message }); }
        }

        [HttpGet]
        public JsonResult ObtenerComentarios(int idAuditoriaZ)
        {
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            var lista = _negocioComentarios.Listar(idAuditoriaZ);
            return Json(lista.Select(c => new {
                idComentario = c.IdComentario,
                nombreUsuario = c.NombreUsuario,
                mensaje = c.Mensaje,
                nombreArchivo = c.NombreArchivo,
                rutaArchivo = c.RutaArchivo,
                extension = c.Extension,
                fecha = c.FechaRegistro.ToString("dd/MM HH:mm"),
                esMio = c.IdUsuario == idUsuarioSesion,
                fotoPerfil = c.FotoPerfil
            }));
        }


        [HttpPost]
        public JsonResult ObtenerEstadosChats([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return Json(new List<Capa_Entidad.Auditoria_Z.ChatEstadoDTO>());
            return Json(_negocioAuditoria.ObtenerEstadosChats(ids));
        }
        #endregion

        #region UTILIDADES
      

        [HttpGet]
        public JsonResult ObtenerEstadoGlobalAuditoria()
        {
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

            var listaBase = _negocioAuditoria.Listar(idLogueado, idRolLogueado);
            var ids = listaBase.Select(a => a.IdAuditoriaZ).ToList();
            var estados = _negocioAuditoria.ObtenerEstadosChats(ids);

            var resultado = listaBase.Select(a => {
                var estado = estados.FirstOrDefault(e => e.IdAuditoriaZ == a.IdAuditoriaZ);
                return new
                {
                    id = a.IdAuditoriaZ,
                    ultimoIdMsg = estado?.UltimoId ?? 0,
                    idAutorUltimo = estado?.IdAutor ?? 0
                };
            }).ToList();

            return Json(resultado);
        }


        [Permiso("AUDITORIAZ_REPORTE")]
        public IActionResult ReporteAuditoriaZ()
        {
            // Recuperar sesión para que el reporte respete los permisos de quién lo ve
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;

            // Pasar parámetros al Listar
            var lista = _negocioAuditoria.Listar(idLogueado, idRolLogueado);

            if (lista == null)
            {
                lista = new List<Capa_Entidad.Auditoria_Z.E_AuditoriaZ>();
            }

            return View(lista);
        }
        #endregion
    }
}
