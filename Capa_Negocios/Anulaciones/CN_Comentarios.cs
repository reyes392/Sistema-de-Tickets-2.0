using Capa_Datos.Anulaciones;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Anulaciones
{
    public class CN_Comentarios
    {
        private readonly CD_Comentarios _daoComentarios;

        public CN_Comentarios(CD_Comentarios dao)
        {
            _daoComentarios = dao;
        }

        public bool RegistrarComentario(Capa_Entidad.Anulaciones.E_Comentarios obj, IFormFile? archivo, string webRootPath)
        {
            // 1. Si hay un archivo, procesamos el guardado físico primero
            if (archivo != null && archivo.Length > 0)
            {
                string folderPath = Path.Combine(webRootPath, "uploads", "anulacioncomentarios");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string extension = Path.GetExtension(archivo.FileName).ToLower();
                string nombreSistema = Guid.NewGuid().ToString() + extension;
                string rutaCompleta = Path.Combine(folderPath, nombreSistema);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    archivo.CopyTo(stream);
                }

                // Llenamos los datos del adjunto en el objeto entidad
                obj.NombreArchivo = archivo.FileName;
                obj.Extension = extension;
                obj.RutaArchivo = "/uploads/anulacioncomentarios/" + nombreSistema; // Ruta relativa para la web
            }

            // 2. Enviamos a la base de datos
            return _daoComentarios.Registrar(obj);
        }

        public List<Capa_Entidad.Anulaciones.E_Comentarios> Listar(int idAnulacion)
        {
            return _daoComentarios.ListarPorAnulacion(idAnulacion);
        }
    }
}
