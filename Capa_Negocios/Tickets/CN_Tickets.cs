using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capa_Negocios.Tickets
{
    public class CN_Tickets
    {
        private readonly CD_Tickets _dao;

        public CN_Tickets(CD_Tickets dao)
        {
            _dao = dao;
        }

        public List<E_Tickets> Listar()
        {
            return _dao.Listar();
        }

        public bool Guardar(E_Tickets obj, string accion, out string mensaje)
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
                if (obj.IdTicket == 0)
                {
                    mensaje = "ID de ticket no válido para actualizar.";
                    return false;
                }

                // No validamos 'Problema' porque ya existe en la BD
            }

            return _dao.Guardar(obj, accion, out mensaje);
        }


        public bool AsignarYProcesar(int idTicket, int idUsuario, out string mensaje)
        {
            return _dao.AsignarYProcesar(idTicket, idUsuario, out mensaje);
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
        public ChatEstadoDTO ObtenerEstadoChat(int idTicket)
        {
            return _dao.ObtenerEstadoUltimoMensaje(idTicket);
        }

    }
}