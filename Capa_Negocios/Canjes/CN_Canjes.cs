using Capa_Datos.Canjes;
using Capa_Entidad.Canjes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Canjes
{
    public class CN_Canjes
    {
        private readonly CD_Canjes _cdCanjes;

        public CN_Canjes(CD_Canjes cdCanjes)
        {
            _cdCanjes = cdCanjes;
        }

        // 1. Listar Canjes
        public List<E_Canje> Listar(int idUsuarioLogueado, int idRol)
        {
            return _cdCanjes.Listar(idUsuarioLogueado,idRol);
        }

        public bool GuardarCanje(E_Canje obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.IdCanje == 0) // Si es nuevo
            {
                if (obj.IdUsuarioSolicitador == 0)
                    Mensaje += "Es necesario identificar al solicitante. ";

                if (obj.IdTipoIncidenciaCanjes == 0)
                    Mensaje += "Es necesario seleccionar el tipo de incidencia. ";

                // --- NUEVA VALIDACIÓN ---
                if (string.IsNullOrWhiteSpace(obj.DescripcionProblema))
                    Mensaje += "Debe ingresar una descripción del problema. ";
            }

            if (Mensaje != string.Empty) return false;

            return _cdCanjes.GuardarCanje(obj, out Mensaje);
        }

        public bool AsignarYProcesar(int idCanje, int idUsuario, out string mensaje)

        {
            return _cdCanjes.AsignarYProcesar(idCanje, idUsuario, out mensaje);
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
   
        public List<ChatEstadoDTO> ObtenerEstadosChats(List<int> ids)
        {
            return _cdCanjes.ObtenerEstadosChatsMasivo(ids);
        }
    }
}