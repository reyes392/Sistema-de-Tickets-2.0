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
    public class CD_Categorias
    {
        private readonly string _connectionString;

        // Inyectamos IConfiguration
        public CD_Categorias(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public List<E_Categorias> ListarCategorias()
        {
            var lista = new List<E_Categorias>();
            using (var cn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT ID_CATEGORIA, DESCRIPCION FROM CATEGORIAS", cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new E_Categorias
                        {
                            IdCategoria = Convert.ToInt32(dr["ID_CATEGORIA"]),
                            Descripcion = dr["DESCRIPCION"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}
