using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Tickets
{
    public class CD_Acceso
    {
        private readonly string _connectionString;

        public CD_Acceso(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Obtener usuario por nombre de usuario
        public E_Usuarios ObtenerUsuario(string userName)
        {
            E_Usuarios usuario = null;
            using (var cn = new SqlConnection(_connectionString))
            {
                // Ajusta los nombres de columnas a tu base de datos
                string query = "SELECT ID_USUARIO, USUARIO, CLAVE, ID_ROL FROM USUARIOS WHERE USUARIO = @user AND ID_ESTADO = 1";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@user", userName);
                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        usuario = new E_Usuarios
                        {
                            IdUsuario = Convert.ToInt32(dr["ID_USUARIO"]),
                            UserName = dr["USUARIO"].ToString(),
                            Clave = dr["CLAVE"].ToString(),
                            IdRol = Convert.ToInt32(dr["ID_ROL"])
                        };
                    }
                }
            }
            return usuario;
        }

        // Obtener usuario por ID
        public E_Usuarios ObtenerUsuarioPorId(int idUsuario)
        {
            E_Usuarios u = null;

            using (var cn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT * FROM USUARIOS WHERE ID_USUARIO = @id";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        u = new E_Usuarios
                        {
                            IdUsuario = Convert.ToInt32(dr["ID_USUARIO"]),
                            Nombres = dr["NOMBRES"].ToString(),
                            Apellidos = dr["APELLIDOS"].ToString(),
                            UserName = dr["USUARIO"].ToString(),
                            Clave = dr["CLAVE"].ToString(),
                            IdRol = Convert.ToInt32(dr["ID_ROL"]),
                            IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),
                            IdCategoria = Convert.ToInt32(dr["ID_CATEGORIA"]),
                            Correo = dr["CORREO"].ToString()
                        };
                    }
                }
            }

            return u;
        }


        // Obtener permisos por rol
        public List<string> ObtenerPermisosPorRol(int idRol)
        {
            var lista = new List<string>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT P.CODIGO
                    FROM PERMISOS P
                    INNER JOIN ROL_PERMISO RP ON RP.ID_PERMISO = P.ID_PERMISO
                    WHERE RP.ID_ROL = @idRol
                     order by P.ID_PERMISO ASC";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@idRol", idRol);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        lista.Add(dr["CODIGO"].ToString());
                }
            }
            return lista;
        }

        // Obtener permisos adicionales de un usuario
        public List<string> ObtenerPermisosUsuario(int idUsuario)
        {
            var lista = new List<string>();
            using (var cn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT P.CODIGO
                    FROM PERMISOS P
                    INNER JOIN USUARIOS_PERMISOS UP ON UP.ID_PERMISO = P.ID_PERMISO
                    WHERE UP.ID_USUARIO = @idUsuario";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        lista.Add(dr["CODIGO"].ToString());
                }
            }
            return lista;
        }

    }
}
