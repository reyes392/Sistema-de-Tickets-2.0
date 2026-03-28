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
    public class CD_TiposIncidenciasAnulaciones
    {

        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_TiposIncidenciasAnulaciones(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_TiposIncidenciasAnulaciones> Listar()
        {
            var lista = new List<E_TiposIncidenciasAnulaciones>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_TIPOS_INCIDENCIAS_ANULACIONES", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_TiposIncidenciasAnulaciones
                                {
                                    IdTiposIncidenciasAnulaciones = Convert.ToInt32(dr["Id"]),

                                    Descripcion = dr["Descripcion"].ToString(),

                                    Estado = dr["Estado"].ToString(),
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

        public bool Guardar(E_TiposIncidenciasAnulaciones obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_GUARDAR_TIPOS_INCIDENCIAS_ANULACIONES", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ACCION", accion);
                        cmd.Parameters.AddWithValue("@ID_TIPO_INCIDENCIA_ANULACIONES", obj.IdTiposIncidenciasAnulaciones);
                        cmd.Parameters.AddWithValue("@DESCRIPCION", obj.Descripcion);
                        cmd.Parameters.AddWithValue("@ID_ESTADO", obj.IdEstado);
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
