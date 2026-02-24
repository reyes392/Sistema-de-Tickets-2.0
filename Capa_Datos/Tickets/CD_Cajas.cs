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
    public class CD_Cajas
    {

        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_Cajas(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_Cajas> Listar()
        {
            var lista = new List<E_Cajas>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_CAJAS", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_Cajas
                                {
                                    IdCaja = Convert.ToInt32(dr["ID_CAJA"]),
                                    Descripcion = dr["DESCRIPCION"].ToString(),


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

        public bool Guardar(E_Cajas obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_GUARDAR_CAJA", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ACCION", accion);
                        cmd.Parameters.AddWithValue("@ID_CAJA", obj.IdCaja);
                        cmd.Parameters.AddWithValue("@DESCRIPCION", obj.Descripcion);


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
