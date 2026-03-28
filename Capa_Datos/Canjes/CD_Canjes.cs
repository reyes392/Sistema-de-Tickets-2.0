using Capa_Entidad.Canjes;
using Capa_Entidad.ReclamosBodega;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Canjes
{
    public class CD_Canjes
    {
        private readonly string _connectionString;
        public CD_Canjes(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // 1. Método para Listar con el filtro de seguridad
        public List<E_Canje> Listar(int idUsuarioLogueado, int idRol)
        {
            List<E_Canje> lista = new List<E_Canje>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_LISTAR_CANJES_FILTRADO", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Asegúrate de pasar los parámetros necesarios aquí si el SP los requiere:
                    // --- AÑADIR PARÁMETROS ---
                    cmd.Parameters.AddWithValue("@IdUsuarioLogueado", idUsuarioLogueado);
                    cmd.Parameters.AddWithValue("@IdRol", idRol);

                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new E_Canje()
                            {
                                // --- CAMPOS OBLIGATORIOS (Claves primarias, etc.) ---
                                IdCanje = Convert.ToInt32(dr["ID_CANJE"]),
                                IdUsuarioSolicitador = Convert.ToInt32(dr["ID_USUARIO_SOLICITADOR"]),
                                IdTipoIncidenciaCanjes = Convert.ToInt32(dr["ID_TIPO_INCICENCIA_CANJES"]),
                                IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),
                                FechaRegistro = Convert.ToDateTime(dr["FECHA_REGISTRO"]),

                                // --- CAMPOS NULABLES OBLIGATORIOS (BD) ---
                                // Si en la base de datos estos campos son NULLABLES, 
                                // en C# deben ser int? o DateTime?
                                IdUsuarioAsignado = dr["ID_USUARIO_ASIGNADO"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_USUARIO_ASIGNADO"]),

                                // Estos campos los agregaste a la clase como anulables (?)
                                IdUsuarioAutorizador = dr["ID_USUARIO_AUTORIZADOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["ID_USUARIO_AUTORIZADOR"]),
                                IdUsuarioAnulador = dr["ID_USUARIO_ANULADOR"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["ID_USUARIO_ANULADOR"]),

                                FechaAutorizado = dr["FECHA_AUTORIZADO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FECHA_AUTORIZADO"]),
                                FechaAnulado = dr["FECHA_ANULADO"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FECHA_ANULADO"]),

                                // --- STRINGS (Validación de nulos para no romper ToString) ---
                                // --- NUEVO CAMPO LEÍDO DEL SP ---
                                DescripcionProblema = dr["DESCRIPCION_PROBLEMA"]?.ToString(),
                                Resolucion = dr["RESOLUCION"]?.ToString(), // Puede ser nulo, la propiedad es string?
                                NombreSolicitador = dr["NombreSolicitador"]?.ToString() ?? string.Empty,
                                TipoIncidencia = dr["TipoIncidencia"]?.ToString() ?? string.Empty,
                                Estado = dr["Estado"]?.ToString() ?? string.Empty,
                                NombreAsignado = dr["NombreAsignado"]?.ToString() ?? string.Empty,

                                // Nuevos campos de nombres (Autorizador/Anulador)
                                NombreAutorizador = dr["NombreAutorizador"]?.ToString(),
                                NombreAnulador = dr["NombreAnulador"]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Loguear error: ex.Message
                lista = new List<E_Canje>();
            }
            return lista;
        }
        public bool GuardarCanje(E_Canje obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            bool respuesta = false;

            using (var cn = new SqlConnection(_connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SP_GUARDAR_CANJE", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Determinamos si es INSERT o UPDATE basado en si el ID ya existe
                    cmd.Parameters.AddWithValue("ACCION", obj.IdCanje == 0 ? "INSERT" : "UPDATE");

                    cmd.Parameters.Add("ID_CANJE", SqlDbType.Int).Value = obj.IdCanje;
                    cmd.Parameters["ID_CANJE"].Direction = ParameterDirection.InputOutput; // Importante para recuperar el ID en INSERT

                    cmd.Parameters.AddWithValue("ID_USUARIO_SOLICITADOR", obj.IdUsuarioSolicitador);
                    cmd.Parameters.AddWithValue("ID_TIPO_INCICENCIA_CANJES", obj.IdTipoIncidenciaCanjes);
                    // --- NUEVO PARÁMETRO ENVIADO ---
                    cmd.Parameters.AddWithValue("DESCRIPCION_PROBLEMA", obj.DescripcionProblema ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("RESOLUCION", obj.Resolucion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("ID_ESTADO", obj.IdEstado);
                    cmd.Parameters.AddWithValue("ID_USUARIO_ASIGNADO", obj.IdUsuarioAsignado == 0 ? DBNull.Value : (object)obj.IdUsuarioAsignado);
                    cmd.Parameters.AddWithValue("ID_USUARIO_AUTORIZADOR", obj.IdUsuarioAutorizador == 0 ? DBNull.Value : (object)obj.IdUsuarioAutorizador);
                    cmd.Parameters.AddWithValue("ID_USUARIO_ANULADOR", obj.IdUsuarioAnulador == 0 ? DBNull.Value : (object)obj.IdUsuarioAnulador);

                    // Parámetros de salida
                    cmd.Parameters.Add("RESULTADO", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("MENSAJE", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["RESULTADO"].Value);
                    Mensaje = cmd.Parameters["MENSAJE"].Value.ToString();

                    // Si fue un INSERT, actualizamos el objeto con el nuevo ID generado
                    if (respuesta && obj.IdCanje == 0)
                    {
                        obj.IdCanje = Convert.ToInt32(cmd.Parameters["ID_CANJE"].Value);
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    Mensaje = ex.Message;
                }
            }
            return respuesta;
        }
        public bool AsignarYProcesar(int idCanje, int idUsuario, out string mensaje)
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
                    string query = @"UPDATE CANJES 
                             SET ID_USUARIO_ASIGNADO = @idUsuario, 
                                 ID_ESTADO = 5
                           
                             WHERE ID_CANJE = @idCanje 
                             AND (ID_USUARIO_ASIGNADO IS NULL OR ID_USUARIO_ASIGNADO = 0)";

                    var cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idCanje", idCanje);
                    cn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        resultado = true;
                        mensaje = "Has tomado el Canje. Estado actualizado a 'En Proceso'.";
                    }
                    else
                    {
                        resultado = false;
                        mensaje = "El Canje ya está siendo atendido por otro Usuario.";
                    }
                }
            }
            catch (Exception ex) { mensaje = ex.Message; resultado = false; }
            return resultado;
        }
 
        public List<Capa_Entidad.Canjes.ChatEstadoDTO> ObtenerEstadosChatsMasivo(List<int> ids)
        {
            List<Capa_Entidad.Canjes.ChatEstadoDTO> lista = new List<Capa_Entidad.Canjes.ChatEstadoDTO>();
            if (ids == null || !ids.Any()) return lista;

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    // Usamos una CTE para obtener el último mensaje de cada canje en una sola consulta
                    string query = $@"
                WITH UltimosMensajes AS (
                    SELECT ID_CANJE, ID_COMENTARIO, ID_USUARIO,
                           ROW_NUMBER() OVER(PARTITION BY ID_CANJE ORDER BY ID_COMENTARIO DESC) as rn
                    FROM CANJES_COMENTARIOS
                    WHERE ID_CANJE IN ({string.Join(",", ids)})
                )
                SELECT ID_CANJE, ID_COMENTARIO, ID_USUARIO
                FROM UltimosMensajes
                WHERE rn = 1";

                    SqlCommand cmd = new SqlCommand(query, cn);
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Capa_Entidad.Canjes.ChatEstadoDTO
                            {
                                IdCanje = Convert.ToInt32(dr["ID_CANJE"]),
                                UltimoId = Convert.ToInt32(dr["ID_COMENTARIO"]),
                                IdAutor = Convert.ToInt32(dr["ID_USUARIO"])
                            });
                        }
                    }
                }
            }
            catch (Exception) { /* Manejar error */ }
            return lista;
        }
    }
}

