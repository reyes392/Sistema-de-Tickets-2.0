using Capa_Datos.Anulaciones;
using Capa_Entidad.Anulaciones;
using Capa_Entidad.ReclamosBodega;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Anulaciones
{
    public class CN_Anulaciones
    {
        private readonly CD_Anulaciones _dao;

        public CN_Anulaciones(CD_Anulaciones dao)
        {
            _dao = dao;
        }

        public List<E_Anulaciones> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Anulaciones obj, string accion, out string mensaje)
        {
            mensaje = "";

            // --- LÓGICA PARA NUEVOS TICKETS ---
            if (accion == "INSERT")
            {
                if (obj.IdUsuarioSolicitud == 0)
                {
                    mensaje = "El usuario solicitante es obligatorio.";
                    return false;
                }

                obj.IdEstado = 3; // Estado inicial: Pendiente
            }
            if (string.IsNullOrWhiteSpace(obj.DescripcionProblema))
            {
                mensaje = "La descripción del problema es obligatoria.";
                return false;
            }

            // --- LÓGICA PARA ACTUALIZACIONES (UPDATE) ---
            else if (accion == "UPDATE")
            {
                // Aquí podrías validar que el IdTicket no sea 0
                if (obj.IdAnulacion == 0)
                {
                    mensaje = "ID  no válido para actualizar.";
                    return false;
                }

                // No validamos 'Problema' porque ya existe en la BD
            }

            return _dao.Guardar(obj, accion, out mensaje);
        }


        public bool AsignarYProcesar(int idAnulacion, int idUsuario, out string mensaje)

        {
            return _dao.AsignarYProcesar(idAnulacion, idUsuario, out mensaje);
        }



        public string GuardarArchivoFisico(IFormFile archivo, string rutaCarpeta)
        {
            // 1. Validar extensiones
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

            if (!extensionesPermitidas.Contains(extension))
                throw new Exception("Extensión de archivo no permitida.");

            // 2. Crear carpeta si no existe
            if (!Directory.Exists(rutaCarpeta)) Directory.CreateDirectory(rutaCarpeta);

            // 3. Generar nombre único para evitar duplicados
            string nombreUnico = Guid.NewGuid().ToString() + extension;
            string rutaCompleta = Path.Combine(rutaCarpeta, nombreUnico);

            // 4. Guardar archivo
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            return nombreUnico;
        }
 
        public List<Capa_Entidad.Anulaciones.ChatEstadoDTO> ObtenerEstadosChats(List<int> idsAnulaciones)
        {
            // Si la lista viene nula o vacía, ni siquiera molestamos a la base de datos
            if (idsAnulaciones == null || !idsAnulaciones.Any())
                return new List<Capa_Entidad.Anulaciones.ChatEstadoDTO>();

            return _dao.ObtenerEstadosUltimosMensajes(idsAnulaciones);
        }
    }
}
