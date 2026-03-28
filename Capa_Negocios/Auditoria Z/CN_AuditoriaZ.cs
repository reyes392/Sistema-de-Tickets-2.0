using Capa_Datos.AuditoriaZ;
using Capa_Entidad.Auditoria_Z;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Capa_Negocios.AuditoriaZ
{
    public class CN_AuditoriaZ
    {
        private readonly CD_AuditoriaZ _cdAuditoriaZ;

        public CN_AuditoriaZ(CD_AuditoriaZ cdAuditoriaZ)
        {
            _cdAuditoriaZ = cdAuditoriaZ;
        }

        // 1. Listar con filtros de seguridad
        public List<E_AuditoriaZ> Listar(int idUsuarioLogueado, int idRol)
        {
            return _cdAuditoriaZ.Listar(idUsuarioLogueado, idRol);
        }

        // 2. Guardar o Actualizar
        public bool Guardar(E_AuditoriaZ obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            // --- Validaciones de Negocio ---
            if (obj.IdUsuarioSolicitador == 0)
                Mensaje += "Debe identificar al usuario solicitador. ";

            if (obj.IdTipoIncidenciaAuditoriaZ == 0)
                Mensaje += "Debe seleccionar un tipo de incidencia. ";

            if (string.IsNullOrWhiteSpace(obj.DescripcionProblema))
                Mensaje += "La descripción del problema es obligatoria. ";

            if (!string.IsNullOrEmpty(Mensaje))
                return false;

            // --- Lógica para determinar la Acción ---
            // Si el ID es 0, es un registro nuevo, de lo contrario es una edición.
            string accion = (obj.IdAuditoriaZ == 0) ? "INSERT" : "UPDATE";

            // Llamada corregida pasando la 'accion' requerida por la Capa de Datos
            return _cdAuditoriaZ.Guardar(obj, accion, out Mensaje);
        }

        // 3. Método para que un técnico tome el caso
        public bool AsignarYProcesar(int idAuditoriaZ, int idUsuario, out string mensaje)
        {
            return _cdAuditoriaZ.AsignarYProcesar(idAuditoriaZ, idUsuario, out mensaje);
        }



        // 4. Obtener el estado del chat/comentarios
        public List<ChatEstadoDTO> ObtenerEstadosChats(List<int> ids)
        {
            return _cdAuditoriaZ.ObtenerEstadosUltimosMensajes(ids);
        }




        // 5. Manejo de archivos físicos (Documentos de Auditoría)
        public string GuardarArchivoFisico(IFormFile archivo, string rutaCarpeta)
        {
            // Validar extensiones permitidas
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

            if (!extensionesPermitidas.Contains(extension))
                throw new System.Exception("Extensión de archivo no permitida.");

            // Crear carpeta si no existe
            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            // Generar nombre único
            string nombreUnico = Guid.NewGuid().ToString() + extension;
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreUnico);

            // Guardar en el servidor/disco
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            return nombreUnico;
        }
    }
}