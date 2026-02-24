using Capa_Entidad.ReclamosBodega;
using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.ReclamosBodega
{
    public class CD_ReclamosBodega
    {

        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_ReclamosBodega(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public List<E_ReclamosBodega> Listar()
        {
            var lista = new List<E_ReclamosBodega>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_RECLAMOS_BODEGA", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_ReclamosBodega
                                {
                                    IdReclamo = Convert.ToInt32(dr["ID_RECLAMO"]),
                                    // --- IDs necesarios para EDICIÓN ---

                               
                                    IdTipoIncidencia = Convert.ToInt32(dr["ID_TIPO_INCICENCIA_RECLAMO"]),
                                    IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),

                                    IdUsuarioSolicitud = dr["ID_USUARIO_SOLICITUD"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_SOLICITUD"]),
                                    // Asignado puede ser nulo si nadie ha tomado el ticket aún
                                    IdUsuarioAsignado = dr["ID_USUARIO_ASIGNADO"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_ASIGNADO"]),

                                    // --- Datos para VISUALIZACIÓN ---
                                    UsuarioSolicitador = dr["SOLICITANTE"].ToString(),
                                    TipoIncidenciaReclamo = dr["TIPO_INCIDENCIA"].ToString(),
                                    Nrequisa = dr["N_REQUISA"].ToString(),
                                    FechaRequisa = dr["FECHA_REQUISA"].ToString(),
                                    Problema = dr["DESCRIPCION_PROBLEMA"].ToString(),
                                    Resolucion = dr["RESOLUCION_PROBLEMA"].ToString(),
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
                throw new Exception("Error al listar Reclamo: " + ex.Message);
            }
            return lista;
        }


        public bool Guardar(E_ReclamosBodega obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    var cmd = new SqlCommand("SP_GUARDAR_RECLAMO", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // --- CONFIGURACIÓN DE PARÁMETROS ---
                    cmd.Parameters.AddWithValue("@ACCION", accion);

                    // IMPORTANTE: Configuramos ID_TICKET como InputOutput para recuperar el ID generado en INSERT
                    SqlParameter paramIdTicket = new SqlParameter("@ID_RECLAMO", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = obj.IdReclamo
                    };
                    cmd.Parameters.Add(paramIdTicket);

                    cmd.Parameters.AddWithValue("@ID_USUARIO_RECLAMO", obj.IdUsuarioSolicitud);
                    cmd.Parameters.AddWithValue("@ID_TIPO_INCICENCIA_RECLAMO", obj.IdTipoIncidencia);
                    cmd.Parameters.AddWithValue("@N_REQUISA", obj.Nrequisa);
                    cmd.Parameters.AddWithValue("@FECHA_REQUISA", obj.FechaRequisa);

                    cmd.Parameters.AddWithValue("@DESCRIPCION_PROBLEMA", obj.Problema);
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
                        obj.IdReclamo = Convert.ToInt32(cmd.Parameters["@ID_RECLAMO"].Value);
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

        public bool AsignarYProcesar(int idReclamo, int idUsuario, out string mensaje)
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
                    string query = @"UPDATE RECLAMOS_BODEGA 
                             SET ID_USUARIO_ASIGNADO = @idUsuario, 
                                 ID_ESTADO = 5, 
                                 FECHA_MODIFICACION = GETDATE() 
                             WHERE ID_RECLAMO = @idReclamo 
                             AND (ID_USUARIO_ASIGNADO IS NULL OR ID_USUARIO_ASIGNADO = 0)";

                    var cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idReclamo", idReclamo);
                    cn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        resultado = true;
                        mensaje = "Has tomado el Reclamo. Estado actualizado a 'En Proceso'.";
                    }
                    else
                    {
                        resultado = false;
                        mensaje = "El Reclamo ya está siendo atendido por otro Usuario.";
                    }
                }
            }
            catch (Exception ex) { mensaje = ex.Message; resultado = false; }
            return resultado;
        }
        public Capa_Entidad.ReclamosBodega.ChatEstadoDTO ObtenerEstadoUltimoMensaje(int idTicket)
        {
            Capa_Entidad.ReclamosBodega.ChatEstadoDTO dto = new Capa_Entidad.ReclamosBodega.ChatEstadoDTO { UltimoId = 0, IdAutor = 0 };
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    string query = @"SELECT TOP 1 ID_COMENTARIO, ID_USUARIO 
                             FROM RECLAMOS_COMENTARIOS 
                             WHERE ID_RECLAMO = @id 
                             ORDER BY ID_COMENTARIO DESC";

                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@id", idTicket);
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            dto.UltimoId = Convert.ToInt32(dr["ID_COMENTARIO"]);
                            dto.IdAutor = Convert.ToInt32(dr["ID_USUARIO"]);
                        }
                    }
                }
            }
            catch (Exception) { /* Manejar error */ }
            return dto;
        }
    }
}
