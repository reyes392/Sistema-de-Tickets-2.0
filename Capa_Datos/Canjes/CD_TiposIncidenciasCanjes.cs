using Capa_Entidad.Canjes;
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
    public class CD_TiposIncidenciasCanjes
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_TiposIncidenciasCanjes(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_TiposIncidenciasCanjes> Listar()
        {
            var lista = new List<E_TiposIncidenciasCanjes>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_TIPOS_INCIDENCIAS_CANJES", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_TiposIncidenciasCanjes
                                {
                                    IdTiposIncidenciasCanjes = Convert.ToInt32(dr["Id"]),

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

        public bool Guardar(E_TiposIncidenciasCanjes obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_GUARDAR_TIPOS_INCIDENCIAS_CANJES", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ACCION", accion);
                        cmd.Parameters.AddWithValue("@ID_TIPO_INCICENCIA_CANJES", obj.IdTiposIncidenciasCanjes);
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
