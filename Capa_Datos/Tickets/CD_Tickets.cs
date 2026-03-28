using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Tickets
{
    public class CD_Tickets
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_Tickets(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public List<E_Tickets> Listar()
        {
            var lista = new List<E_Tickets>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_TICKETS", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_Tickets
                                {
                                    IdTicket = Convert.ToInt32(dr["ID_TICKET"]),
                                    // --- IDs necesarios para EDICIÓN ---

                                    IdCaja = dr["ID_CAJA"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_CAJA"]),
                                    IdIncidencias = Convert.ToInt32(dr["ID_TIPOS_INCIDENCIAS_TICKETS"]),
                                    IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),
                                    //IdUsuarioSolicitud = Convert.ToInt32(dr["ID_USUARIO_SOLICITUD"]),
                                    IdUsuarioSolicitud = dr["ID_USUARIO_SOLICITUD"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_SOLICITUD"]),
                                    // Asignado puede ser nulo si nadie ha tomado el ticket aún
                                    IdUsuarioAsignado = dr["ID_USUARIO_ASIGNADO"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_ASIGNADO"]),

                                    // --- Datos para VISUALIZACIÓN ---
                                    UsusarioSolicitador = dr["SOLICITANTE"].ToString(),
                                    Incidencia = dr["INCIDENCIA"].ToString(),
                                    Caja = dr["CAJA"].ToString(),
                                    NivelUrgencia = dr["NIVEL_URGENCIA"].ToString(),
                                    Respuesta = dr["RESPUESTA"].ToString(),
                                    Problema = dr["PROBLEMA"].ToString(),
                                    Resolucion = dr["RESOLUCION"].ToString(),
                                    Estado = dr["ESTADO"].ToString(),
                                    Categoria = dr["CATEGORIA"].ToString(),
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
                throw new Exception("Error al listar Tickets: " + ex.Message);
            }
            return lista;
        }


        public bool Guardar(E_Tickets obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    var cmd = new SqlCommand("SP_GUARDAR_TICKET", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // --- CONFIGURACIÓN DE PARÁMETROS ---
                    cmd.Parameters.AddWithValue("@ACCION", accion);

                    // IMPORTANTE: Configuramos ID_TICKET como InputOutput para recuperar el ID generado en INSERT
                    SqlParameter paramIdTicket = new SqlParameter("@ID_TICKET", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = obj.IdTicket
                    };
                    cmd.Parameters.Add(paramIdTicket);

                    cmd.Parameters.AddWithValue("@ID_USUARIO_SOLICITUD", obj.IdUsuarioSolicitud);
                    cmd.Parameters.AddWithValue("@ID_CAJA", obj.IdCaja == 0 ? DBNull.Value : obj.IdCaja);
                    cmd.Parameters.AddWithValue("@ID_TIPOS_INCIDENCIAS_TICKETS", obj.IdIncidencias);
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
                        obj.IdTicket = Convert.ToInt32(cmd.Parameters["@ID_TICKET"].Value);
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

        public bool AsignarYProcesar(int idTicket, int idUsuario, out string mensaje)
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
                    string query = @"UPDATE TICKETS 
                             SET ID_USUARIO_ASIGNADO = @idUsuario, 
                                 ID_ESTADO = 5, 
                                 FECHA_MODIFICACION = GETDATE() 
                             WHERE ID_TICKET = @idTicket 
                             AND (ID_USUARIO_ASIGNADO IS NULL OR ID_USUARIO_ASIGNADO = 0)";

                    var cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idTicket", idTicket);
                    cn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        resultado = true;
                        mensaje = "Has tomado el ticket. Estado actualizado a 'En Proceso'.";
                    }
                    else
                    {
                        resultado = false;
                        mensaje = "El ticket ya está siendo atendido por otro técnico.";
                    }
                }
            }
            catch (Exception ex) { mensaje = ex.Message; resultado = false; }
            return resultado;
        }
     
        public List<ChatEstadoDTO> ObtenerEstadosUltimosMensajes(List<int> idsTickets)
        {
            List<ChatEstadoDTO> lista = new List<ChatEstadoDTO>();
            if (idsTickets == null || !idsTickets.Any()) return lista;

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    // Creamos una cadena de IDs separados por coma (ej: "1,2,5,8")
                    string idsFormateados = string.Join(",", idsTickets);

                    string query = $@"
                WITH UltimosMensajes AS (
                    SELECT 
                        ID_TICKET, 
                        ID_COMENTARIO, 
                        ID_USUARIO,
                        ROW_NUMBER() OVER(PARTITION BY ID_TICKET ORDER BY ID_COMENTARIO DESC) as Fila
                    FROM TICKETS_COMENTARIOS
                    WHERE ID_TICKET IN ({idsFormateados})
                )
                SELECT ID_TICKET, ID_COMENTARIO, ID_USUARIO
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
                                IdTicket = Convert.ToInt32(dr["ID_TICKET"]),
                                UltimoId = Convert.ToInt32(dr["ID_COMENTARIO"]),
                                IdAutor = Convert.ToInt32(dr["ID_USUARIO"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Loguear error: ex.Message 
            }
            return lista;
        }

    }
}
