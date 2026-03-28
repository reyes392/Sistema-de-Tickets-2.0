using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Anulaciones
{
    public class CD_Comentarios
    {
        private readonly string _connectionString;

        public CD_Comentarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool Registrar(Capa_Entidad.Anulaciones.E_Comentarios obj)
        {
            bool respuesta = false;
            using (var cn = new SqlConnection(_connectionString))
            {
                // 1. Agregamos FECHA_REGISTRO en la lista de columnas
                // 2. Usamos GETDATE() en VALUES para que SQL Server asigne la hora actual
                string query = @"INSERT INTO ANULACIONES_COMENTARIOS 
                        (ID_ANULACION, ID_USUARIO, MENSAJE, NOMBRE_ARCHIVO, RUTA_ARCHIVO, EXTENSION, FECHA_REGISTRO) 
                        VALUES (@idAnulacion, @idUsuario, @mensaje, @nomArchivo, @ruta, @ext, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, cn);

                // Configuramos los parámetros (se mantienen igual, ya que la fecha la pone el SQL)
                cmd.Parameters.AddWithValue("@idAnulacion", obj.IdAnulacion);
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

        public List<Capa_Entidad.Anulaciones.E_Comentarios> ListarPorAnulacion(int idAnulacion)
        {
            var lista = new List<Capa_Entidad.Anulaciones.E_Comentarios>();
            using (var cn = new SqlConnection(_connectionString))
            {
                // Hacemos JOIN con USUARIOS para obtener el nombre del emisor
                string query = @"SELECT C.ID_COMENTARIO, C.ID_ANULACION, C.ID_USUARIO, U.NOMBRES+ ' '+u.APELLIDOS AS NOMBRE_USUARIO,
                                 C.MENSAJE, C.NOMBRE_ARCHIVO, C.RUTA_ARCHIVO, C.EXTENSION, C.FECHA_REGISTRO ,U.FOTO_PERFIL
                                 FROM ANULACIONES_COMENTARIOS C
                                 INNER JOIN USUARIOS U ON C.ID_USUARIO = U.ID_USUARIO
                                 WHERE C.ID_ANULACION = @id 
                                 ORDER BY C.FECHA_REGISTRO ASC";

                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", idAnulacion);
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Capa_Entidad.Anulaciones.E_Comentarios
                        {
                            IdComentario = Convert.ToInt32(dr["ID_COMENTARIO"]),
                            IdAnulacion = Convert.ToInt32(dr["ID_ANULACION"]),
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
