using Capa_Datos.Canjes;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocios.Canjes
{
    public class CN_Comentarios
    {
        private readonly CD_Comentarios _daoComentarios;

        public CN_Comentarios(CD_Comentarios dao)
        {
            _daoComentarios = dao;
        }

        public bool RegistrarComentario(Capa_Entidad.Canjes.E_Comentarios obj, IFormFile? archivo, string webRootPath)
        {
            // 1. Si hay un archivo, procesamos el guardado físico primero
            if (archivo != null && archivo.Length > 0)
            {
                string folderPath = Path.Combine(webRootPath, "uploads", "canjescomentarios");
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
                obj.RutaArchivo = "/uploads/canjescomentarios/" + nombreSistema; // Ruta relativa para la web
            }

            // 2. Enviamos a la base de datos
            return _daoComentarios.Registrar(obj);
        }

        public List<Capa_Entidad.Canjes.E_Comentarios> Listar(int idCanje)
        {
            return _daoComentarios.ListarPorCanje(idCanje);
        }
    }
}
