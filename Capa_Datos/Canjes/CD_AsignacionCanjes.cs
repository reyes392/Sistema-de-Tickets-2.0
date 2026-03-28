using Capa_Entidad.Canjes;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Datos.Canjes
{
    public class CD_AsignacionCanjes
    {
        private readonly string _connectionString;
        public CD_AsignacionCanjes(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<E_AsignacionCanjes> ListarAsignaciones()
        {
            List<E_AsignacionCanjes> lista = new List<E_AsignacionCanjes>();
            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT A.ID_ASIGNACION, A.ID_USUARIO_CANJES, U1.NOMBRES+' '+U1.APELLIDOS AS NombreTecnico, 
                                        A.ID_USUARIO_SOLICITANTE, U2.NOMBRES+' '+U2.APELLIDOS AS NombreSolicitante, A.FECHA_ASIGNACION
                                 FROM RELACION_CANJES_ASIGNACION A
                                 INNER JOIN USUARIOS U1 ON A.ID_USUARIO_CANJES = U1.ID_USUARIO
                                 INNER JOIN USUARIOS U2 ON A.ID_USUARIO_SOLICITANTE = U2.ID_USUARIO";

                SqlCommand cmd = new SqlCommand(query, cn);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_AsignacionCanjes
                        {
                            IdAsignacion = Convert.ToInt32(dr["ID_ASIGNACION"]),
                            IdUsuarioCanjes = Convert.ToInt32(dr["ID_USUARIO_CANJES"]),
                            NombreUsuarioCanjes = dr["NombreTecnico"].ToString(),
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
        public bool Registrar(E_AsignacionCanjes obj, out string mensaje)
        {
            mensaje = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO RELACION_CANJES_ASIGNACION (ID_USUARIO_CANJES, ID_USUARIO_SOLICITANTE) VALUES (@idT, @idS)";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.Parameters.AddWithValue("@idT", obj.IdUsuarioCanjes);
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
                string query = "DELETE FROM RELACION_CANJES_ASIGNACION WHERE ID_ASIGNACION = @id";
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@id", idAsignacion);
                cn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

