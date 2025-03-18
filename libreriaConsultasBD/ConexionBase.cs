using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using FirebirdSql.Data.Client;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;
using FirebirdSql.Data.FirebirdClient;

namespace libreriaConsultasBD
{
    public abstract class ConexionBase
    {
        public string TipoSGBD { get; set; }
        public string Servidor { get; set; }
        public string ModoAuth { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public bool EstaAbierta { get; set; }
        public string Identificador { get; set; }
        public ConexionBase(string tipoSGBD)
        {
            TipoSGBD = tipoSGBD;  // Asignamos el tipo de SGBD al construir la conexión
        }



        public abstract bool AbrirConexion();
        public abstract DataTable EjecutarConsulta(string query);
        public abstract void Cerrar();
    }

}
