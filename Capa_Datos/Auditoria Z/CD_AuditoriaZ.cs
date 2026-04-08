using Capa_Entidad.Auditoria_Z;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos.AuditoriaZ
{
    public class CD_AuditoriaZ
    {
        private readonly string _connectionString;
        public CD_AuditoriaZ(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_AuditoriaZ> Listar(int idUsuarioLogueado, int idRol)
        {
            List<E_AuditoriaZ> lista = new List<E_AuditoriaZ>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_LISTAR_AUDITORIA_Z_FILTRADO", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuarioLogueado", idUsuarioLogueado);
                    cmd.Parameters.AddWithValue("@IdRol", idRol);

                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new E_AuditoriaZ()
                            {
                                IdAuditoriaZ = Convert.ToInt32(dr["ID_AUDITORIA_Z"]),
                                IdUsuarioSolicitador = Convert.ToInt32(dr["ID_USUARIO_SOLICITADOR"]),
                                IdTipoIncidenciaAuditoriaZ = Convert.ToInt32(dr["ID_TIPO_INCICENCIA_AUDITORIA_Z"]),
                                DescripcionProblema = dr["DESCRIPCION_PROBLEMA"]?.ToString(),
                                ResolucionProblema = dr["RESOLUCION_PROBLEMA"]?.ToString(),
                                IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),
                                IdUsuarioAsignado = dr["ID_USUARIO_ASIGNADO"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["ID_USUARIO_ASIGNADO"]),
                                FechaRegistro = Convert.ToDateTime(dr["FECHA_REGISTRO"]),
                                FechaModificacion = dr["FECHA_MODIFICACION"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FECHA_MODIFICACION"]),

                                // Campos de los JOIN
                                NombreSolicitador = dr["NombreSolicitador"]?.ToString(),
                                TipoIncidenciaAuditoria = dr["TipoIncidenciaAuditoria"]?.ToString(),
                                Estado = dr["Estado"]?.ToString(),
                                NombreAsignado = dr["NombreAsignado"]?.ToString(),
                                // Mapeo del NOMBRE del modificador para la vista
                                NombreModificador = dr["NombreModificador"]?.ToString() // <--- LÍNEA NUEVA
                            });
                        }
                    }
                }
            }
            catch (Exception) { lista = new List<E_AuditoriaZ>(); }
            return lista;
        }

        public bool Guardar(E_AuditoriaZ obj, string AccionManual, out string Mensaje)
        {
            Mensaje = string.Empty;
            bool respuesta = false;

            using (var cn = new SqlConnection(_connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SP_GUARDAR_AUDITORIA_Z", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 1. Usar la Acción que viene del Controlador para evitar errores de lógica
                    cmd.Parameters.AddWithValue("@ACCION", AccionManual);

                    // 2. Parámetro de Entrada/Salida con @
                    SqlParameter paramId = new SqlParameter("@ID_AUDITORIA_Z", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = obj.IdAuditoriaZ
                    };
                    cmd.Parameters.Add(paramId);

                    // 3. Mapeo exacto con los nombres del SP (incluyendo el typo 'INCICENCIA')
                    cmd.Parameters.AddWithValue("@ID_USUARIO_SOLICITADOR", obj.IdUsuarioSolicitador);
                    cmd.Parameters.AddWithValue("@ID_TIPO_INCICENCIA_AUDITORIA_Z", obj.IdTipoIncidenciaAuditoriaZ);
                    cmd.Parameters.AddWithValue("@DESCRIPCION_PROBLEMA", (object)obj.DescripcionProblema ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RESOLUCION_PROBLEMA", (object)obj.ResolucionProblema ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ID_ESTADO", obj.IdEstado);
                    cmd.Parameters.AddWithValue("@ID_USUARIO_ASIGNADO", (object)obj.IdUsuarioAsignado ?? DBNull.Value);
                    // Dentro del método Guardar, donde agregas los parámetros:
                    cmd.Parameters.AddWithValue("@ID_USUARIO_MODIFICADOR", (object)obj.IdUsuarioModificador ?? DBNull.Value);
                    // 4. Parámetros de salida con @
                    cmd.Parameters.Add("@RESULTADO", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@MENSAJE", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    // 5. Recuperar resultados
                    respuesta = Convert.ToBoolean(cmd.Parameters["@RESULTADO"].Value);
                    Mensaje = cmd.Parameters["@MENSAJE"].Value.ToString() ?? "";

                    // 6. ACTUALIZACIÓN CRÍTICA: Asignar el nuevo ID al objeto para los adjuntos
                    if (respuesta && AccionManual == "INSERT")
                    {
                        obj.IdAuditoriaZ = Convert.ToInt32(cmd.Parameters["@ID_AUDITORIA_Z"].Value);
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    Mensaje = "Error en Capa Datos: " + ex.Message;
                }
            }
            return respuesta;
        }
        // ... (Métodos anteriores Listar y Guardar)

        public bool AsignarYProcesar(int idAuditoriaZ, int idUsuario, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    // ID_ESTADO = 5 (En Proceso, según tu lógica)
                    // Actualizamos solo si no tiene un usuario asignado aún
                    string query = @"UPDATE AUDITORIA_Z 
                             SET ID_USUARIO_ASIGNADO = @idUsuario, 
                                 ID_ESTADO = 5, 
                                 FECHA_MODIFICACION = GETDATE() 
                             WHERE ID_AUDITORIA_Z = @idAuditoriaZ 
                             AND (ID_USUARIO_ASIGNADO IS NULL OR ID_USUARIO_ASIGNADO = 0)";

                    var cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idAuditoriaZ", idAuditoriaZ);
                    cn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        resultado = true;
                        mensaje = "Has tomado el caso de Auditoría. Estado actualizado a 'En Proceso'.";
                    }
                    else
                    {
                        resultado = false;
                        mensaje = "El caso de Auditoría ya está siendo atendido por otro técnico.";
                    }
                }
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                resultado = false;
            }
            return resultado;
        }


        public List<ChatEstadoDTO> ObtenerEstadosUltimosMensajes(List<int> ids)
        {
            List<ChatEstadoDTO> lista = new List<ChatEstadoDTO>();
            if (ids == null || !ids.Any()) return lista;

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    string idsFormateados = string.Join(",", ids);
                    string query = $@"
                WITH UltimosMensajes AS (
                    SELECT 
                        ID_AUDITORIA_Z, 
                        ID_COMENTARIO, 
                        ID_USUARIO,
                        ROW_NUMBER() OVER(PARTITION BY ID_AUDITORIA_Z ORDER BY ID_COMENTARIO DESC) as Fila
                    FROM AUDITORIA_Z_COMENTARIOS
                    WHERE ID_AUDITORIA_Z IN ({idsFormateados})
                )
                SELECT ID_AUDITORIA_Z, ID_COMENTARIO, ID_USUARIO
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
                                IdAuditoriaZ = Convert.ToInt32(dr["ID_AUDITORIA_Z"]),
                                UltimoId = Convert.ToInt32(dr["ID_COMENTARIO"]),
                                IdAutor = Convert.ToInt32(dr["ID_USUARIO"])
                            });
                        }
                    }
                }
            }
            catch (Exception) { /* Log error */ }
            return lista;
        }

    }
}