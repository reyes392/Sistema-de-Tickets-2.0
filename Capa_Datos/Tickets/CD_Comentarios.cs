using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos.Tickets
{
    public class CD_Comentarios
    {
        private readonly string _connectionString;

        public CD_Comentarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool Registrar(E_Comentarios obj)
        {
            bool respuesta = false;
            using (var cn = new SqlConnection(_connectionString))
            {
                // Insertamos el comentario y la metadata del adjunto (si existe)
                string query = @"INSERT INTO TICKETS_COMENTARIOS (ID_TICKET, ID_USUARIO, MENSAJE, NOMBRE_ARCHIVO, RUTA_ARCHIVO, EXTENSION) 
                                VALUES (@idTicket, @idUsuario, @mensaje, @nomArchivo, @ruta, @ext)";

                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@idTicket", obj.IdTicket);
                cmd.Parameters.AddWithValue("@idUsuario", obj.IdUsuario);
                cmd.Parameters.AddWithValue("@mensaje", (object)obj.Mensaje ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@nomArchivo", (object)obj.NombreArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ruta", (object)obj.RutaArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ext", (object)obj.Extension ?? DBNull.Value);

                cn.Open();
                respuesta = cmd.ExecuteNonQuery() > 0;
            }
            return respuesta;
        }

        public List<E_Comentarios> ListarPorTicket(int idTicket)
        {
            var lista = new List<E_Comentarios>();
            using (var cn = new SqlConnection(_connectionString))
            {
                // Hacemos JOIN con USUARIOS para obtener el nombre del emisor
                string query = @"SELECT C.ID_COMENTARIO, C.ID_TICKET, C.ID_USUARIO, U.NOMBRES+ ' '+u.APELLIDOS AS NOMBRE_USUARIO,
                                        C.MENSAJE, C.NOMBRE_ARCHIVO, C.RUTA_ARCHIVO, C.EXTENSION, C.FECHA_REGISTRO ,U.FOTO_PERFIL
                                 FROM TICKETS_COMENTARIOS C
                                 INNER JOIN USUARIOS U ON C.ID_USUARIO = U.ID_USUARIO
                                 WHERE C.ID_TICKET = @id 
                                 ORDER BY C.FECHA_REGISTRO ASC";

                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", idTicket);
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_Comentarios
                        {
                            IdComentario = Convert.ToInt32(dr["ID_COMENTARIO"]),
                            IdTicket = Convert.ToInt32(dr["ID_TICKET"]),
                            IdUsuario = Convert.ToInt32(dr["ID_USUARIO"]),
                            NombreUsuario = dr["NOMBRE_USUARIO"].ToString(),
                            Mensaje = dr["MENSAJE"].ToString(),
                            NombreArchivo = dr["NOMBRE_ARCHIVO"].ToString(),
                            RutaArchivo = dr["RUTA_ARCHIVO"].ToString(),
                            Extension = dr["EXTENSION"].ToString(),
                            FechaRegistro = Convert.ToDateTime(dr["FECHA_REGISTRO"]),
                            FotoPerfil = dr["FOTO_PERFIL"] == DBNull.Value ? "" : dr["FOTO_PERFIL"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}