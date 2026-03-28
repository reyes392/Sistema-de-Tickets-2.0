using Capa_Entidad.Auditoria_Z;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Auditoria_Z
{
    public class CD_AsignacionZ
    {
        private readonly string _connectionString;
        public CD_AsignacionZ(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_Asignacion_Z> ListarAsignaciones()
        {
            List<E_Asignacion_Z> lista = new List<E_Asignacion_Z>();
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT A.ID_ASIGNACION_Z, A.ID_USUARIO_AUDITORIA_Z, U1.NOMBRES+' '+U1.APELLIDOS AS NombreTecnico, 
                                        A.ID_USUARIO_SOLICITANTE, U2.NOMBRES+' '+U2.APELLIDOS AS NombreSolicitante, A.FECHA_ASIGNACION
                                 FROM RELACION_AUDITORIA_Z_ASIGNACION A
                                 INNER JOIN USUARIOS U1 ON A.ID_USUARIO_AUDITORIA_Z = U1.ID_USUARIO
                                 INNER JOIN USUARIOS U2 ON A.ID_USUARIO_SOLICITANTE = U2.ID_USUARIO";

                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_Asignacion_Z
                        {
                            IdAsignacionZ = Convert.ToInt32(dr["ID_ASIGNACION_Z"]),
                            IdUsuarioAuditoriaZ = Convert.ToInt32(dr["ID_USUARIO_AUDITORIA_Z"]),
                            NombreUsuarioAuditoriaZ = dr["NombreTecnico"].ToString(),
                            IdUsuarioSolicitante = Convert.ToInt32(dr["ID_USUARIO_SOLICITANTE"]),
                            NombreUsuarioSolicitante = dr["NombreSolicitante"].ToString(),
                            FechaAsignacion = Convert.ToDateTime(dr["FECHA_ASIGNACION"])
                        });
                    }
                }
            }
            return lista;
        }


        // En CD_AsignacionCanjes.cs (Asegúrate de tener el try-catch)
        public bool Registrar(E_Asignacion_Z obj, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO RELACION_AUDITORIA_Z_ASIGNACION (ID_USUARIO_AUDITORIA_Z, ID_USUARIO_SOLICITANTE) VALUES (@idT, @idS)";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idT", obj.IdUsuarioAuditoriaZ);
                    cmd.Parameters.AddWithValue("@idS", obj.IdUsuarioSolicitante);
                    cn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) mensaje = "Ya existe."; // Ignoramos duplicados silenciosamente
                return false;
            }
        }
        public bool Eliminar(int idAsignacion)
        {
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM RELACION_AUDITORIA_Z_ASIGNACION WHERE ID_ASIGNACION_Z = @id";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", idAsignacion);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
