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
    public class CD_Estados
    {
        private readonly string _connectionString;
        public CD_Estados(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<E_Estados> ListarEstados()
        {
            var lista = new List<E_Estados>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT ID_ESTADO, DESCRIPCION FROM ESTADOS", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_Estados
                        {
                            IdEstado = Convert.ToInt32(dr["ID_ESTADO"]),
                            Descripcion = dr["DESCRIPCION"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}
