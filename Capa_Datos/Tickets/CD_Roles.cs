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
    public class CD_Roles
    {
        private readonly string _connectionString;
        public CD_Roles(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<E_Roles> ListarRoles()
        {
            var lista = new List<E_Roles>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT ID_ROL, DESCRIPCION FROM ROLES", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {

                    while (dr.Read())
                    {
                        lista.Add(new E_Roles
                        {
                            IdRol = Convert.ToInt32(dr["ID_ROL"]),
                            Descripcion = dr["DESCRIPCION"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
        public bool Guardar(E_Roles rol, string accion, out string mensaje)
        {
            bool ok = false;
            mensaje = "";

            using (var cn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SP_GUARDAR_ROL", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ACCION", accion);
                cmd.Parameters.AddWithValue("@ID_ROL", rol.IdRol);
                cmd.Parameters.AddWithValue("@DESCRIPCION", rol.Descripcion);

                cmd.Parameters.Add("@RESULTADO", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@MENSAJE", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();

                ok = Convert.ToBoolean(cmd.Parameters["@RESULTADO"].Value);
                mensaje = cmd.Parameters["@MENSAJE"].Value.ToString();
            }

            return ok;
        }

    }
}
