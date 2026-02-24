using Capa_Datos.ReclamosBodega;
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
    public class CN_Archivos
    {
        private readonly CD_Archivos _daoArchivos;
        public CN_Archivos(CD_Archivos dao) { _daoArchivos = dao; }

        public string GuardarFisico(IFormFile archivo, string folderPath)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string extension = Path.GetExtension(archivo.FileName).ToLower();
            string nombreSistema = Guid.NewGuid().ToString() + extension;
            string rutaCompleta = Path.Combine(folderPath, nombreSistema);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }
            return nombreSistema;
        }

        public bool RegistrarEnBaseDatos(E_Archivos obj) => _daoArchivos.Registrar(obj);

        public List<E_Archivos> Listar(int idTicket) => _daoArchivos.ListarPorTicket(idTicket);
    }
}
