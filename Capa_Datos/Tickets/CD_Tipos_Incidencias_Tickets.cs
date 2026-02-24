using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;


namespace Capa_Datos.Tickets
{
    public class CD_Tipos_Incidencias_Tickets
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_Tipos_Incidencias_Tickets(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_Tipos_Incidencias_Tickets> Listar()
        {
            var lista = new List<E_Tipos_Incidencias_Tickets>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_TIPOS_INCIDENCIAS_TICKETS", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_Tipos_Incidencias_Tickets
                                {
                                    IdTiposIncidenciasTickets = Convert.ToInt32(dr["Id"]),
                                    Descripcion = dr["Descripcion"].ToString(),
                                    Estado = dr["Estado"].ToString(),
                                    NivelUrgencia = dr["Urgencia"].ToString(),
                                    TiempoRespuesta = dr["Respuesta"].ToString(),
                                    FechaRegistro = dr["Registro"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(dr["Registro"]),

                                    FechaModificacion = dr["Modificacion"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(dr["Modificacion"]),



                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar Incidencias: " + ex.Message);
            }

            return lista;
        }

        public bool Guardar(E_Tipos_Incidencias_Tickets obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_GUARDAR_TIPOS_INCIDENCIAS_TICKETS", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ACCION", accion);
                        cmd.Parameters.AddWithValue("@ID_TIPOS_INCIDENCIAS_TICKETS", obj.IdTiposIncidenciasTickets);
                        cmd.Parameters.AddWithValue("@DESCRIPCION", obj.Descripcion);
                        cmd.Parameters.AddWithValue("@ID_ESTADO", obj.IdEstado);
                        cmd.Parameters.AddWithValue("@ID_NIVEL_URGENCIA_PRIORIDAD", obj.IdNivelUrgencia);
                        cmd.Parameters.Add("@RESULTADO", SqlDbType.Bit).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@MENSAJE", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                        cn.Open();
                        cmd.ExecuteNonQuery();

                        resultado = Convert.ToBoolean(cmd.Parameters["@RESULTADO"].Value);
                        mensaje = cmd.Parameters["@MENSAJE"].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                mensaje = "Error: " + ex.Message;
            }

            return resultado;
        }

    }
}
