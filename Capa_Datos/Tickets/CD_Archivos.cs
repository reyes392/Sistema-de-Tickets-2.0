using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos.Tickets
{
    public class CD_Archivos
    {
        private readonly string _connectionString;
        public CD_Archivos(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool Registrar(E_Archivos obj)
        {
            bool respuesta = false;
            using (var cn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO ARCHIVOS_ADJUNTOS (ID_TICKET, NOMBRE_ARCHIVO, NOMBRE_SISTEMA, EXTENSION, RUTA_SERVIDOR) 
                                VALUES (@id, @nom, @sys, @ext, @ruta)";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", obj.IdReferencia);
                cmd.Parameters.AddWithValue("@nom", obj.NombreOriginal);
                cmd.Parameters.AddWithValue("@sys", obj.NombreSistema);
                cmd.Parameters.AddWithValue("@ext", obj.Extension);
                cmd.Parameters.AddWithValue("@ruta", obj.Ruta);
                cn.Open();
                respuesta = cmd.ExecuteNonQuery() > 0;
            }
            return respuesta;
        }

        public List<E_Archivos> ListarPorTicket(int idTicket)
        {
            var lista = new List<E_Archivos>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string query = "SELECT NOMBRE_ARCHIVO, RUTA_SERVIDOR, EXTENSION FROM ARCHIVOS_ADJUNTOS WHERE ID_TICKET = @id";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", idTicket);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_Archivos
                        {
                            NombreOriginal = dr["NOMBRE_ARCHIVO"].ToString(),
                            Ruta = dr["RUTA_SERVIDOR"].ToString(),
                            Extension = dr["EXTENSION"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}