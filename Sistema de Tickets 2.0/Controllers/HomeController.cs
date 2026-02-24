using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sistema_de_Tickets_2._0.Models;
using Capa_Entidad;
using Sistema_de_Tickets_2._0.Filter;
using Capa_Datos;
using Capa_Entidad.Tickets;
using Capa_Negocios.Tickets;

namespace Sistema_de_Tickets_2._0.Controllers
{
    public class HomeController : Controller
    {

        #region INYECCION DE DEPENDENCIAS
        private readonly ILogger<HomeController> _logger;
        private readonly CN_Usuarios _usuarioNegocio;
        private readonly CN_Permisos _negocioPermisos;
        private readonly CN_Roles _negocioRoles;
        private readonly CN_Estados _negocioEstados;
        private readonly CN_Categorias _negocioCategorias;
        private readonly CN_Equipos _negocioEquipos;
        private readonly CN_Tipos_Incidencias_Tickets _negocioTiposIncidenciasTickets;
        private readonly CN_Niveles_Urgencia_Tickets _negocioNivelesUrgenciaTickets;
        private readonly CN_Cajas _negocioCajas;
        private readonly CN_Tickets _negocioTickets;
        private readonly CN_Archivos _negocioArchivos;
        private readonly CN_Comentarios _negocioComentarios;

        // UN SOLO CONSTRUCTOR PARA TODO
        public HomeController(ILogger<HomeController> logger,CN_Usuarios usuarioNegocio,CN_Permisos negocio,CN_Roles negocioRoles,CN_Estados negocioEstados, CN_Categorias negocioCategorias,CN_Equipos negocioEquipos,
            CN_Tipos_Incidencias_Tickets negocioTiposIncidenciasTickets, CN_Niveles_Urgencia_Tickets negocioNivelesUrgenciaTickets,CN_Cajas negocioCajas,CN_Tickets negocioTickets, CN_Archivos negocioArchivos, CN_Comentarios negocioComentarios
            )
        {
            _logger = logger;
            _usuarioNegocio = usuarioNegocio;
            _negocioPermisos = negocio;
            _negocioRoles=negocioRoles;
            _negocioEstados = negocioEstados;
            _negocioCategorias = negocioCategorias;
            _negocioEquipos = negocioEquipos;
            _negocioTiposIncidenciasTickets = negocioTiposIncidenciasTickets;
            _negocioNivelesUrgenciaTickets = negocioNivelesUrgenciaTickets;
            _negocioCajas = negocioCajas;
            _negocioTickets = negocioTickets;
            _negocioArchivos = negocioArchivos;
            _negocioComentarios = negocioComentarios;

        }
        #endregion

        #region DASHBOARD
        [Permiso("INDEX_VER")]
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region GESTION DE USUARIOS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE USUARIOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("USUARIOS_VER")]
        public IActionResult Usuarios()
        {
            ViewBag.Roles = _negocioRoles.ListarRoles();
            ViewBag.Estados = _negocioEstados .ListarEstados();
            ViewBag.Categorias = _negocioCategorias.ListarCategorias();
            var lista = _usuarioNegocio.Listar();
            return View(lista);
        }

        [Permiso("USUARIOS_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarUsuario(E_Usuarios usuario, string Accion)
        {
            bool resultado = _usuarioNegocio.Guardar(usuario, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE PERMISOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE PERMISOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("PERMISOS_VER")]
        public IActionResult Permisos()
        {
            var lista = _negocioPermisos.Listar();
            return View(lista);
        }

        [Permiso("PERMISOS_CREAR_EDITAR")]

        [HttpPost]
        public IActionResult GuardarPermiso(E_Permiso permiso, string accion)
        {
            bool ok = _negocioPermisos.Guardar(permiso, accion, out string mensaje);

            return Json(new
            {
                success = ok,
                mensaje = mensaje
            });
        }
        #endregion

        #region GESTION DE ROLES Y PERMISOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE ROLES Y PERMISOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        [Permiso("ROLES_VER")]
        public IActionResult RolesyPermisos() // Cambiamos el nombre para que coincida con la vista unificada
        {
            // 1. Instanciamos el ViewModel que creaste en la Capa Entidad
            var model = new SeguridadViewModel
            {
                // 2. Cargamos AMBAS listas usando tus servicios de negocio
                ListaRoles = _negocioRoles.ListarRoles(),
                ListaUsuarios = _usuarioNegocio.Listar() // Asegúrate de tener inyectado _usuarioNegocio
            };

            // 3. Retornamos el objeto 'model' en lugar de solo la lista de roles
            return View(model);
        }

        //CON ESTE METODO MANEJAMOS GUARDAR/EDITAR ROLES
        [Permiso("ROLES_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarRoles(E_Roles rol, string accion)
        {
            bool ok = _negocioRoles.Guardar(rol, accion, out string mensaje);

            return Json(new
            {
                success = ok,
                mensaje = mensaje
            });
        }

        // CON ESTE METODO PODEMOS OBTENER LOS PERMISOS POR ROLES
        [HttpGet]
        public IActionResult ObtenerPermisosRol(int idRol)
        {
            var permisosRol = _negocioPermisos.ObtenerPermisosPorRol(idRol);
            return Json(permisosRol);
        }

        //CON ESTE METODO PODEMOS AGREGAR O QUITAR PERMISOS POR ROLES
        [HttpPost]
        public IActionResult CambiarPermisoRol(int idRol, int idPermiso, string asignar)
        {
            bool asignarBool = asignar?.ToLower() == "true";

            try
            {
                if (asignarBool)
                    _negocioPermisos.AsignarRol(idRol, idPermiso);
                else
                    _negocioPermisos.QuitarRol(idRol, idPermiso);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        //CON ESTE METODOLISTAMOS TODOS LOS PERMISOS EXISTENTES
        [HttpGet]
        public IActionResult ListarPermisos()
        {
            var lista = _negocioPermisos.Listar();
            return Json(lista);
        }

        //CON ESTE METODO PODEMOS VER LOS PERMISOS DEL USUARIO POR ROLES Y POR USUARIOS
        [HttpGet]
        public IActionResult ObtenerPermisosUsuarioCompleto(int idUsuario)
        {
            var lista = _negocioPermisos.ObtenerPermisosUsuarioCompleto(idUsuario);
            return Json(lista);
        }

        //CON ESTE METODO PODEMOS AGREGAR O QUITAR PERMISOS POR USUARIOS MAS QUE TODOS PARA NO AFECTAR TODOS LOS USURIOS A X ROLES SOLO AGREGAMOS O QUITAMOS UN PERMISO NECESARIO
        [HttpPost]
        public IActionResult CambiarPermisoUsuario(int idUsuario, int idPermiso, string asignar)
        {
            bool asignarBool = asignar?.ToLower() == "true";
            try
            {
                if (asignarBool)
                    _negocioPermisos.AsignarUsuario(idUsuario, idPermiso);
                else
                    _negocioPermisos.QuitarUsuario(idUsuario, idPermiso);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }

        #endregion

        #region GESTION DE EQUIPOS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE EQUIPOS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("EQUIPOS_VER")]
        public IActionResult Equipos()
        {
      
            var lista = _negocioEquipos.Listar();
            return View(lista);
        }

        [Permiso("EQUIPOS_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarEquipos(E_Equipos equipos, string Accion)
        {
            bool resultado = _negocioEquipos.Guardar(equipos, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE NIVELES DE URGENCIAS DE LOS TICKETS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE NIVELES DE URGENCIAS DE LOS TICKETS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("URGENCIAS_TICKET_VER")]
        public IActionResult TipoUrgenciaTickets()
        {
            var lista = _negocioNivelesUrgenciaTickets.Listar();
            return View(lista);
        }

        [Permiso("URGENCIAS_TICKET_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarTipoUrgencia(E_Niveles_Urgencia_Tickets urgencias, string Accion)
        {
            bool resultado = _negocioNivelesUrgenciaTickets.Guardar(urgencias, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE TIPOS DE INCIDENCIAS DE TICKETS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GESTION DE TIPOS DE INCIDENCIAS DE TICKETS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("INCIDENCIAS_TICKET_VER")]
        public IActionResult IncidenciasTickets()
        {
         
            ViewBag.Estados = _negocioEstados.ListarEstados();
            ViewBag.Urgencia = _negocioNivelesUrgenciaTickets.Listar();
            var lista = _negocioTiposIncidenciasTickets.Listar();
            return View(lista);
        }

        [Permiso("INCIDENCIAS_TICKET_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardaIncidenciasTickets(E_Tipos_Incidencias_Tickets incidencias, string Accion)
        {
            bool resultado = _negocioTiposIncidenciasTickets.Guardar(incidencias, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE CAJAS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE CAJAS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        [Permiso("CAJAS_VER")]
        public IActionResult Cajas()
        {

            var lista = _negocioCajas.Listar();
            return View(lista);
        }

        [Permiso("CAJAS_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarCajas(E_Cajas cajas, string Accion)
        {
            bool resultado = _negocioCajas.Guardar(cajas, Accion, out string mensaje);
            // Devolver JSON para AJAX
            return Json(new { success = resultado, mensaje = mensaje });
        }
        #endregion

        #region GESTION DE TICKETS

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///GESTION DE TICKETS
        /////////////////////////////////////////////////////////////////////////////////////////////////////
     
        [Permiso("TICKET_VER")]
        public IActionResult Tickets()
        {
            ViewBag.Cajas = _negocioCajas.Listar();
            ViewBag.Incidencias = _negocioTiposIncidenciasTickets.Listar();
            ViewBag.Estados = _negocioEstados.ListarEstados();

            // Obtenemos datos de sesión
            int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            int idRolLogueado = HttpContext.Session.GetInt32("IdRol") ?? 0;
            string nombreLogueado = HttpContext.Session.GetString("NombreUsuario") ?? "Usuario";

            // Pasamos a la vista para el JS
            ViewBag.IdUsuarioLogueado = idLogueado;
            ViewBag.RolUsuario = idRolLogueado;
            ViewBag.NombreUsuarioLogueado = nombreLogueado;

            var lista = _negocioTickets.Listar();

            // LÓGICA DE FILTRADO SOLICITADA:
            // Solo Admin (1) y Soporte (3) ven todo. Los demás solo lo propio.
            if (idRolLogueado != 1 && idRolLogueado != 3)
            {
                lista = lista.Where(t => t.IdUsuarioSolicitud == idLogueado).ToList();
            }

            return View(lista);
        }
        [Permiso("TICKET_CREAR_EDITAR")]
        [HttpPost]
        public IActionResult GuardarTickets(E_Tickets tickets, string Accion, List<IFormFile> archivos)
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
                tickets.IdUsuarioSolicitud = idUsuarioSesion.Value;
            }

            // 3. Guardamos el Ticket (Recuerda que ahora el SP devuelve el ID generado en tickets.IdTicket)
            bool resultado = _negocioTickets.Guardar(tickets, Accion, out string mensaje);

            // 4. Lógica de Archivos: Solo si el ticket se guardó bien y vienen archivos
            if (resultado && archivos != null && archivos.Count > 0)
            {
                try
                {
                    // Ruta física: wwwroot/uploads/tickets/
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tickets");

                    foreach (var file in archivos)
                    {
                        // Guardar el archivo físico usando tu Capa de Negocio de archivos
                        string nombreSistema = _negocioArchivos.GuardarFisico(file, folderPath);

                        // Registrar la referencia en la base de datos
                        var entidadArchivo = new E_Archivos
                        {
                            IdReferencia = tickets.IdTicket, // El ID que devolvió el SP
                            NombreOriginal = file.FileName,
                            NombreSistema = nombreSistema,
                            Extension = Path.GetExtension(file.FileName),
                            Ruta = "/uploads/tickets/" + nombreSistema
                        };

                        _negocioArchivos.RegistrarEnBaseDatos(entidadArchivo);
                    }
                }
                catch (Exception ex)
                {
                    // Opcional: Podrías acumular un mensaje de error si los archivos fallan pero el ticket no
                    mensaje += " (Aviso: El ticket se guardó pero hubo problemas con los archivos: " + ex.Message + ")";
                }
            }

            return Json(new { success = resultado, mensaje = mensaje });
        }

        [HttpGet]
        public JsonResult ObtenerAdjuntos(int idTicket)
        {
            // Llamamos a la capa de negocios que a su vez llama a la de datos
            // El método Listar(idTicket) ya lo tienes definido en CN_Archivos
            var lista = _negocioArchivos.Listar(idTicket);

            // Retornamos la lista en formato JSON para que el JS la procese
            return Json(lista);
        }

        [HttpPost]
        public IActionResult TomarTicket(int idTicket)
        {
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuarioSesion == 0) return Json(new { success = false, mensaje = "Sesión expirada" });

            // Llamamos al método que asigna y cambia estado
            bool ok = _negocioTickets.AsignarYProcesar(idTicket, idUsuarioSesion, out string mensaje);

            return Json(new { success = ok, mensaje = mensaje });
        }

        #endregion

        #region GESTIÓN DE COMENTARIOS (CHAT)
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /// GESTIÓN DE COMENTARIOS (CHAT)
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost]
        public IActionResult GuardarComentario(int idTicket, string mensaje, IFormFile? archivo)
        {
            // 1. Recuperamos el ID del usuario desde la Sesión (siguiendo tu ejemplo)
            int? idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null)
            {
                return Json(new { success = false, mensaje = "La sesión ha expirado." });
            }

            try
            {
                var oComentario = new E_Comentarios
                {
                    IdTicket = idTicket,
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
        public JsonResult ObtenerComentarios(int idTicket)
        {
            // Obtenemos el ID de sesión para que el JS sepa cuáles mensajes son "míos"
            int idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            var lista = _negocioComentarios.Listar(idTicket);

            // Mapeamos para que el JS reciba nombres de propiedades consistentes
            var resultado = lista.Select(c => new {
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
        public string ObtenerVersionTickets()
        {
            var lista = _negocioTickets.Listar();
            // Sumamos los estados + el conteo. 
            // Si un ticket pasa de estado 3 a 5, el total cambia y el JS lo detecta.
            int sumaEstados = lista.Sum(t => t.IdEstado);
            int conteo = lista.Count;

            return $"{conteo}-{sumaEstados}";
        }
        [HttpGet]
        public JsonResult ObtenerUltimoComentarioId(int idTicket)
        {
            var lista = _negocioComentarios.Listar(idTicket);
            int ultimoId = 0;
            if (lista != null && lista.Count > 0)
            {
                ultimoId = lista.Max(x => x.IdComentario);
            }
            return Json(new { ultimoId = ultimoId });
        }

        [HttpGet]
        public JsonResult ObtenerEstadoChat(int idTicket)
        {
            var estado = _negocioTickets.ObtenerEstadoChat(idTicket);

            // En .NET Core, no hace falta JsonRequestBehavior.AllowGet
            return Json(new
            {
                ultimoId = estado.UltimoId,
                idAutor = estado.IdAutor
            });
        }
        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///REDIRECCIONAR A LOS USUARIOS SIN PERMISOS AL SISTEMA
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult SinAcceso()
        {
            return View();
        }

    }
}
