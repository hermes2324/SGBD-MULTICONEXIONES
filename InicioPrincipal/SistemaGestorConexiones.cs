using libreriaConsultasBD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InicioPrincipal
{
    public class SistemaGestorConexiones
    {
        private List<SGBDConexion> conexiones;

        public SistemaGestorConexiones()
        {
            conexiones = new List<SGBDConexion>();
        }

        public void AgregarConexion(SGBDConexion conexion)
        {
            if (conexion != null)
            {
                conexiones.Add(conexion);
            }
        }

        public List<SGBDConexion> ObtenerConexiones()
        {
            return conexiones;
        }
    }

}
