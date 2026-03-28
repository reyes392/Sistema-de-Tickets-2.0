using Capa_Entidad.Auditoria_Z;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Auditoria_Z
{
    public class CD_TiposIncidenciasAuditoriaZ
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_TiposIncidenciasAuditoriaZ(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_TiposIncidenciasAuditoriaZ> Listar()
        {
            var lista = new List<E_TiposIncidenciasAuditoriaZ>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_TIPOS_INCIDENCIAS_AUDITORIA_Z", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_TiposIncidenciasAuditoriaZ
                                {
                                    IdTiposIncidenciasAuditoriaZ = Convert.ToInt32(dr["Id"]),

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

        public bool Guardar(E_TiposIncidenciasAuditoriaZ obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_GUARDAR_TIPOS_INCIDENCIAS_AUDITORIA_Z", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ACCION", accion);
                        cmd.Parameters.AddWithValue("@ID_TIPO_INCIDENCIA_AUDITORIA_Z", obj.IdTiposIncidenciasAuditoriaZ);
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
