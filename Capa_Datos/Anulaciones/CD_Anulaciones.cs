using Capa_Entidad.Anulaciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Anulaciones
{
    public class CD_Anulaciones
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_Anulaciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public List<E_Anulaciones> Listar()
        {
            var lista = new List<E_Anulaciones>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_ANULACIONES", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_Anulaciones
                                {
                                    IdAnulacion = Convert.ToInt32(dr["ID_ANULACION"]),
                                    IdCaja = dr["ID_CAJA"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_CAJA"]),
                                    // --- IDs necesarios para EDICIÓN ---
                                    IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),
                                    //IdUsuarioSolicitud = Convert.ToInt32(dr["ID_USUARIO_SOLICITUD"]),
                                    IdUsuarioSolicitud = dr["ID_USUARIO_SOLICITUD"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_SOLICITUD"]),
                                    // Asignado puede ser nulo si nadie ha tomado el ticket aún
                                    IdUsuarioAsignado = dr["ID_USUARIO_ASIGNADO"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_ASIGNADO"]),
                                    IdIncidencias = dr["ID_TIPO_INCIDENCIA_ANULACIONES"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_TIPO_INCIDENCIA_ANULACIONES"]),
                                    // --- Datos para VISUALIZACIÓN ---
                                    UsuarioSolicitador = dr["SOLICITANTE"].ToString(),
                                    Caja = dr["CAJA"].ToString(),
                                    Incidencia = dr["INCIDENCIA"].ToString(),
                                    DescripcionProblema = dr["DESCRIPCION_PROBLEMA"] == DBNull.Value ? "" : dr["DESCRIPCION_PROBLEMA"].ToString(),
                                    Resolucion = dr["RESOLUCION"].ToString(),
                                    Estado = dr["ESTADO"].ToString(),
                                    UsuarioAsignado = dr["USUARIO_ASIGNADO"].ToString(),
                                    Registro = dr["REGISTRO"] == DBNull.Value ? null : Convert.ToDateTime(dr["REGISTRO"]),
                                    Modificacion = dr["MODIFICACION"] == DBNull.Value ? null : Convert.ToDateTime(dr["MODIFICACION"]),
                                  
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar : " + ex.Message);
            }
            return lista;
        }


        public bool Guardar(E_Anulaciones obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    var cmd = new SqlCommand("SP_GUARDAR_ANULACIONES", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // --- CONFIGURACIÓN DE PARÁMETROS ---
                    cmd.Parameters.AddWithValue("@ACCION", accion);

                    // IMPORTANTE: Configuramos ID_ANULACION como InputOutput para recuperar el ID generado en INSERT
                    SqlParameter paramIdTicket = new SqlParameter("@ID_ANULACION", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = obj.IdAnulacion
                    };
                    cmd.Parameters.Add(paramIdTicket);

                    cmd.Parameters.AddWithValue("@ID_USUARIO_SOLICITUD", obj.IdUsuarioSolicitud);
                    cmd.Parameters.AddWithValue("@ID_CAJA", obj.IdCaja == 0 ? DBNull.Value : obj.IdCaja);
                    cmd.Parameters.AddWithValue("@ID_TIPO_INCIDENCIA_ANULACIONES", obj.IdIncidencias);
                    cmd.Parameters.AddWithValue("@DESCRIPCION_PROBLEMA", (object)obj.DescripcionProblema ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RESOLUCION_PROBLEMA", (object)obj.Resolucion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ID_ESTADO", obj.IdEstado);
                    cmd.Parameters.AddWithValue("@ID_USUARIO_ASIGNADO", obj.IdUsuarioAsignado == 0 ? DBNull.Value : obj.IdUsuarioAsignado);

                    // Parámetros de salida
                    cmd.Parameters.Add("@RESULTADO", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@MENSAJE", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    // --- CAPTURA DE RESULTADOS ---
                    resultado = Convert.ToBoolean(cmd.Parameters["@RESULTADO"].Value);
                    mensaje = cmd.Parameters["@MENSAJE"].Value.ToString();

                    // Si fue exitoso, actualizamos el ID en el objeto (muy importante para los archivos adjuntos)
                    if (resultado)
                    {
                        obj.IdAnulacion = Convert.ToInt32(cmd.Parameters["@ID_ANULACION"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = ex.Message;
            }
            return resultado;
        }

        public bool AsignarYProcesar(int idAnulacion, int idUsuario, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    // ID_ESTADO = 2 representa 'En Proceso'
                    // Solo actuamos si el ticket está en estado 'Pendiente' (ID_ESTADO = 3) 
                    // y no tiene técnico asignado.
                    string query = @"UPDATE ANULACIONES 
                             SET ID_USUARIO_ASIGNADO = @idUsuario, 
                                 ID_ESTADO = 5, 
                                 FECHA_MODIFICACION = GETDATE() 
                             WHERE ID_ANULACION = @idAnulacion
                             AND (ID_USUARIO_ASIGNADO IS NULL OR ID_USUARIO_ASIGNADO = 0)";

                    var cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idAnulacion", idAnulacion);
                    cn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        resultado = true;
                        mensaje = "Has tomado el caso. Estado actualizado a 'En Proceso'.";
                    }
                    else
                    {
                        resultado = false;
                        mensaje = "El caso ya está siendo atendido por otro técnico.";
                    }
                }
            }
            catch (Exception ex) { mensaje = ex.Message; resultado = false; }
            return resultado;
        }
        public List<ChatEstadoDTO> ObtenerEstadosUltimosMensajes(List<int> idsAnulaciones)
        {
            List<ChatEstadoDTO> lista = new List<ChatEstadoDTO>();
            if (idsAnulaciones == null || !idsAnulaciones.Any()) return lista;

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    string idsFormateados = string.Join(",", idsAnulaciones);

                    string query = $@"
                WITH UltimosMensajes AS (
                    SELECT 
                        ID_ANULACION, 
                        ID_COMENTARIO, 
                        ID_USUARIO,
                        ROW_NUMBER() OVER(PARTITION BY ID_ANULACION ORDER BY ID_COMENTARIO DESC) as Fila
                    FROM ANULACIONES_COMENTARIOS
                    WHERE ID_ANULACION IN ({idsFormateados})
                )
                SELECT ID_ANULACION, ID_COMENTARIO, ID_USUARIO
                FROM UltimosMensajes
                WHERE Fila = 1";

                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ChatEstadoDTO
                            {
                                IdAnulacion = Convert.ToInt32(dr["ID_ANULACION"]),
                                UltimoId = Convert.ToInt32(dr["ID_COMENTARIO"]),
                                IdAutor = Convert.ToInt32(dr["ID_USUARIO"])
                            });
                        }
                    }
                }
            }
            catch (Exception) { /* Loguear error */ }
            return lista;
        }
    }
}
