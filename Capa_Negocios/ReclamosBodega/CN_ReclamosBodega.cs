using Capa_Datos.ReclamosBodega;
using Capa_Datos.Tickets;
using Capa_Entidad.ReclamosBodega;
using Capa_Entidad.Tickets;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.ReclamosBodega
{
    public class CN_ReclamosBodega
    {
        private readonly CD_ReclamosBodega _dao;

        public CN_ReclamosBodega(CD_ReclamosBodega dao)
        {
            _dao = dao;
        }

        public List<E_ReclamosBodega> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_ReclamosBodega obj, string accion, out string mensaje)
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

                // El problema SOLO es obligatorio al crear el ticket
                if (string.IsNullOrEmpty(obj.Problema))
                {
                    mensaje = "Debe describir el problema.";
                    return false;
                }

                obj.IdEstado = 3; // Estado inicial: Pendiente
            }

            // --- LÓGICA PARA ACTUALIZACIONES (UPDATE) ---
            else if (accion == "UPDATE")
            {
                // Aquí podrías validar que el IdTicket no sea 0
                if (obj.IdReclamo == 0)
                {
                    mensaje = "ID de reclamo no válido para actualizar.";
                    return false;
                }

                // No validamos 'Problema' porque ya existe en la BD
            }

            return _dao.Guardar(obj, accion, out mensaje);
        }


        public bool AsignarYProcesar(int idReclamo, int idUsuario, out string mensaje)
        
        {
            return _dao.AsignarYProcesar(idReclamo, idUsuario, out mensaje);
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
        public Capa_Entidad.ReclamosBodega.ChatEstadoDTO ObtenerEstadoChat(int idReclamo)
        {
            return _dao.ObtenerEstadoUltimoMensaje(idTicket: idReclamo);
        }

    }
}
