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
    public class CD_Niveles_Urgencia_tickets
    {
        private readonly string _connectionString;
        public CD_Niveles_Urgencia_tickets(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<E_Niveles_Urgencia_Tickets> Listar()
        {
            var lista = new List<E_Niveles_Urgencia_Tickets>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT ID_NIVEL_URGENCIA_PRIORIDAD,DESCRIPCION,TIEMPO_RESPUESTA_HORAS FROM NIVELES_URGENCIA_PRIORIDAD", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {

                    while (dr.Read())
                    {
                        lista.Add(new E_Niveles_Urgencia_Tickets
                        {
                            IdNivelesUrgencia = Convert.ToInt32(dr["ID_NIVEL_URGENCIA_PRIORIDAD"]),
                            Descripcion = dr["DESCRIPCION"].ToString(),
                            TiempoRespuesta = dr["TIEMPO_RESPUESTA_HORAS"].ToString(),
                        });
                    }
                }
            }
            return lista;
        }
        public bool Guardar(E_Niveles_Urgencia_Tickets rol, string accion, out string mensaje)
        {
            bool ok = false;
            mensaje = "";

            using (var cn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SP_GUARDAR_NIVELES_URGENCIAS", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ACCION", accion);
                cmd.Parameters.AddWithValue("@ID_NIVEL_URGENCIA_PRIORIDAD", rol.IdNivelesUrgencia);
                cmd.Parameters.AddWithValue("@DESCRIPCION", rol.Descripcion);
                cmd.Parameters.AddWithValue("@TIEMPO_RESPUESTA_HORAS", rol.TiempoRespuesta);

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
