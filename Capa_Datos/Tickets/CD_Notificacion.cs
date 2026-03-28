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
    public class CD_Notificacion
    {
        private readonly string _connectionString;
        public CD_Notificacion(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public int ObtenerConteoActividad(int idUsuario, int idRol)
        {
            int total = 0;
            // Usa tu cadena de conexión (ejemplo: Conexion.cadena)
            using (var cn = new SqlConnection(_connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_ObtenerNotificacionesActivas", cn);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@IdRol", idRol);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.Open();
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch (Exception)
                {
                    total = 0;
                }
            }
            return total;
        }
        public int ObtenerConteoActividadGlobal(int idUsuario, int idRol)
        {
            int total = 0;
            try
            {
                using (var cn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_MONITOR_ACTIVIDAD_GLOBAL", cn);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@IdRol", idRol);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cn.Open();
                    // Ejecutamos Scalar porque el SP devuelve una sola fila/columna con el SUM
                    total = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                total = 0;
            }
            return total;
        }
    }
}
