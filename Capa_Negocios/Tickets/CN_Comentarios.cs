using Capa_Datos.Tickets;
using Capa_Entidad.Tickets;
using Microsoft.AspNetCore.Http;

namespace Capa_Negocios.Tickets
{
    public class CN_Comentarios
    {
        private readonly CD_Comentarios _daoComentarios;

        public CN_Comentarios(CD_Comentarios dao)
        {
            _daoComentarios = dao;
        }

        public bool RegistrarComentario(E_Comentarios obj, IFormFile? archivo, string webRootPath)
        {
            // 1. Si hay un archivo, procesamos el guardado físico primero
            if (archivo != null && archivo.Length > 0)
            {
                string folderPath = Path.Combine(webRootPath, "uploads", "comentarios");
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
                obj.RutaArchivo = "/uploads/comentarios/" + nombreSistema; // Ruta relativa para la web
            }

            // 2. Enviamos a la base de datos
            return _daoComentarios.Registrar(obj);
        }

        public List<E_Comentarios> Listar(int idTicket)
        {
            return _daoComentarios.ListarPorTicket(idTicket);
        }
    }
}