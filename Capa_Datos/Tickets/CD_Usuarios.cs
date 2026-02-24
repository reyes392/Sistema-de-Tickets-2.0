using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos.Tickets
{
    public class CD_Usuarios
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_Usuarios(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_Usuarios> Listar()
        {
            var lista = new List<E_Usuarios>();
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_LISTAR_USUARIOS", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cn.Open();

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                lista.Add(new E_Usuarios
                                {
                                    IdUsuario = Convert.ToInt32(dr["ID_USUARIO"]),
                                    Nombres = dr["NOMBRES"].ToString(),
                                    Apellidos = dr["APELLIDOS"].ToString(),
                                    UserName = dr["USUARIO"].ToString(),
                                    Correo = dr["CORREO"].ToString(),
                                    Rol = dr["ROL"].ToString(),
                                    Estado = dr["ESTADO"].ToString(),
                                    Categoria = dr["CATEGORIA"].ToString(),
                                    FechaRegistrar = dr["REGISTRO"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(dr["REGISTRO"]),

                                    FechaModificacion = dr["MODIFICACION"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(dr["MODIFICACION"]),
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios: " + ex.Message);
            }

            return lista;
        }

        public bool Guardar(E_Usuarios obj, string accion, out string mensaje)
        {
            bool resultado = false;
            mensaje = "";

            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("SP_GUARDAR_USUARIO", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ACCION", accion);
                        cmd.Parameters.AddWithValue("@ID_USUARIO", obj.IdUsuario);
                        cmd.Parameters.AddWithValue("@NOMBRES", obj.Nombres);
                        cmd.Parameters.AddWithValue("@APELLIDOS", obj.Apellidos);
                        cmd.Parameters.AddWithValue("@USUARIO", obj.UserName);
                        cmd.Parameters.AddWithValue("@CLAVE", obj.Clave ?? "");
                        cmd.Parameters.AddWithValue("@ID_ROL", obj.IdRol);
                        cmd.Parameters.AddWithValue("@ID_ESTADO", obj.IdEstado);
                        cmd.Parameters.AddWithValue("@ID_CATEGORIA", obj.IdCategoria);
                        cmd.Parameters.AddWithValue("@CORREO", obj.Correo);

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
