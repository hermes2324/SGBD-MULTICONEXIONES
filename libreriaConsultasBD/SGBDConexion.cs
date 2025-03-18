using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libreriaConsultasBD
{
    public class SGBDConexion
    {
        public string Identificador { get; set; }
        public string TipoSGBD { get; set; }
        public string BaseDatos { get; set; }

        // Propiedad para la conexión base
        public ConexionBase Conexion { get; set; }

        // Constructor que requiere una instancia de ConexionBase
        public SGBDConexion(ConexionBase conexion)
        {
            this.Conexion = conexion;
        }

        // Propiedades que "heredan" de ConexionBase
        public string Servidor
        {
            get => Conexion.Servidor;
            set => Conexion.Servidor = value;
        }

        public string ModoAuth
        {
            get => Conexion.ModoAuth;
            set => Conexion.ModoAuth = value;
        }

        public string Usuario
        {
            get => Conexion.Usuario;
            set => Conexion.Usuario = value;
        }

        public string Password
        {
            get => Conexion.Password;
            set => Conexion.Password = value;
        }
    }
}

