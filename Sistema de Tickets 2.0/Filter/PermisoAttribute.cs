using Capa_Negocios.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Sistema_de_Tickets_2._0.Filter
{
    public class PermisoAttribute: Attribute, IAuthorizationFilter
    {
        private readonly string _permiso;

        public PermisoAttribute(string permiso)
        {
            _permiso = permiso;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            var idUsuario = session.GetInt32("IdUsuario");

            // No logueado
            if (idUsuario == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Acceso",
                    null);

                return;
            }

            // Obtener servicio CN_Acceso
            var acceso = context.HttpContext.RequestServices.GetService<CN_Acceso>();

            // Validar permiso
            bool tiene = acceso.TienePermiso(idUsuario.Value,_permiso);

            if (!tiene)
            {
                context.Result = new RedirectToActionResult("SinAcceso","Home",null);
            }
        }
    }
}
