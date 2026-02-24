using Capa_Entidad.Tickets;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos.Tickets
{
    public class CD_Permisos
    {
        private readonly string _connection;

        public CD_Permisos(IConfiguration config)
        {
            _connection = config.GetConnectionString("DefaultConnection");
        }

        // LISTAR
        public List<E_Permiso> Listar()
        {
            var lista = new List<E_Permiso>();

            using (var cn = new SqlConnection(_connection))
            {
                string sql = "SELECT * FROM PERMISOS";

                var cmd = new SqlCommand(sql, cn);

                cn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_Permiso
                        {
                            IdPermiso = Convert.ToInt32(dr["ID_PERMISO"]),
                            Descripcion = dr["DESCRIPCION"].ToString(),
                            Codigo = dr["CODIGO"].ToString()
                        });
                    }
                }
            }

            return lista;
        }

        // GUARDAR / EDITAR
        public bool Guardar(E_Permiso permiso, string accion, out string mensaje)
        {
            mensaje = "";
            bool ok = false;

            using (var cn = new SqlConnection(_connection))
            {
                string sql = "SP_GUARDAR_PERMISO";

                var cmd = new SqlCommand(sql, cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ACCION", accion);
                cmd.Parameters.AddWithValue("@ID_PERMISO", permiso.IdPermiso);
                cmd.Parameters.AddWithValue("@DESCRIPCION", permiso.Descripcion);
                cmd.Parameters.AddWithValue("@CODIGO", permiso.Codigo);

                cmd.Parameters.Add("@RESULTADO", SqlDbType.Bit)
                              .Direction = ParameterDirection.Output;

                cmd.Parameters.Add("@MENSAJE", SqlDbType.VarChar, 200)
                              .Direction = ParameterDirection.Output;

                cn.Open();
                cmd.ExecuteNonQuery();

                ok = Convert.ToBoolean(cmd.Parameters["@RESULTADO"].Value);
                mensaje = cmd.Parameters["@MENSAJE"].Value.ToString();
            }

            return ok;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        //Permisos por rol
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AsignarRol(int idRol, int idPermiso)
        {
            using var cn = new SqlConnection(_connection);

            var cmd = new SqlCommand("SP_ASIGNAR_PERMISO_ROL", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_ROL", idRol);
            cmd.Parameters.AddWithValue("@ID_PERMISO", idPermiso);

            cn.Open();
            cmd.ExecuteNonQuery();
        }



        // OBTENER PERMISOS FINALES
        public List<string> ObtenerPermisosUsuario(int idUsuario)
        {
            var lista = new List<string>();

            using var cn = new SqlConnection(_connection);

            var cmd = new SqlCommand("SP_OBTENER_PERMISOS_USUARIO", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_USUARIO", idUsuario);

            cn.Open();

            using var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(dr["CODIGO"].ToString());
            }

            return lista;
        }
        public List<int> ObtenerPermisosPorRol(int idRol)
        {
            var lista = new List<int>();

            using var cn = new SqlConnection(_connection);
            var cmd = new SqlCommand(
                "SELECT ID_PERMISO FROM ROL_PERMISO WHERE ID_ROL = @idRol",
                cn
            );

            cmd.Parameters.AddWithValue("@idRol", idRol);

            cn.Open();
            using var dr = cmd.ExecuteReader();

            while (dr.Read())
                lista.Add(Convert.ToInt32(dr["ID_PERMISO"]));

            return lista;
        }
        public void QuitarRol(int idRol, int idPermiso)
        {
            using var cn = new SqlConnection(_connection);

            var cmd = new SqlCommand("SP_QUITAR_PERMISO_ROL", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_ROL", idRol);
            cmd.Parameters.AddWithValue("@ID_PERMISO", idPermiso);

            cn.Open();
            cmd.ExecuteNonQuery();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///Permisos por usuario
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///

        // ASIGNAR PERMISOS A USUARIOS
        public void AsignarUsuario(int idUsuario, int idPermiso)
        {
            using var cn = new SqlConnection(_connection);
            var cmd = new SqlCommand("SP_ASIGNAR_PERMISO_USUARIO", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_USUARIO", idUsuario);
            cmd.Parameters.AddWithValue("@ID_PERMISO", idPermiso);

            cn.Open();
            cmd.ExecuteNonQuery();
        }

        // QUITAR PERMISOS A USUARIOS
        public void QuitarUsuario(int idUsuario, int idPermiso)
        {
            using var cn = new SqlConnection(_connection);
            var cmd = new SqlCommand("SP_QUITAR_PERMISO_USUARIO", cn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_USUARIO", idUsuario);
            cmd.Parameters.AddWithValue("@ID_PERMISO", idPermiso);

            cn.Open();
            cmd.ExecuteNonQuery();
        }

        // OBTENER PERMISOS DE UN USUARIO(ROL+USUARIO)
        public List<E_Permiso> ObtenerPermisosUsuarioCompleto(int idUsuario)
        {
            var lista = new List<E_Permiso>();

            using var cn = new SqlConnection(_connection);
            var cmd = new SqlCommand(@"
                SELECT 
                    P.ID_PERMISO,
                    P.DESCRIPCION,
                    CASE WHEN RP.ID_PERMISO IS NOT NULL THEN 1 ELSE 0 END AS AsignadoRol,
                    CASE WHEN UP.ID_PERMISO IS NOT NULL THEN 1 ELSE 0 END AS AsignadoUsuario
                FROM PERMISOS P
                LEFT JOIN ROL_PERMISO RP 
                    ON P.ID_PERMISO = RP.ID_PERMISO
                    AND RP.ID_ROL = (SELECT ID_ROL FROM USUARIOS WHERE ID_USUARIO = @ID_USUARIO)
                LEFT JOIN USUARIOS_PERMISOS UP
                    ON P.ID_PERMISO = UP.ID_PERMISO
                    AND UP.ID_USUARIO = @ID_USUARIO
            ", cn);

            cmd.Parameters.AddWithValue("@ID_USUARIO", idUsuario);
            cn.Open();

            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new E_Permiso
                {
                    IdPermiso = Convert.ToInt32(dr["ID_PERMISO"]),
                    Descripcion = dr["DESCRIPCION"].ToString(),
                    AsignadoRol = Convert.ToBoolean(dr["AsignadoRol"]),
                    AsignadoUsuario = Convert.ToBoolean(dr["AsignadoUsuario"])
                });
            }

            return lista;
        }



    }
}
