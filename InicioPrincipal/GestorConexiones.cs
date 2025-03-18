using libreriaConsultasBD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InicioPrincipal
{
    public class GestorConexiones
    {
        private List<ConexionBase> conexionesActivas;
        private Dictionary<string, int> contadorConexiones;
        public GestorConexiones()
        {
            conexionesActivas = new List<ConexionBase>();
            contadorConexiones = new Dictionary<string, int>();
        }

        // Método para agregar una nueva conexión
        //public void AgregarConexion(ConexionBase conexion)
        //{
        //    conexionesActivas.Add(conexion);

        //}


        // Método para obtener una conexión específica por índice

       // private Dictionary<string, int> contadorConexiones = new Dictionary<string, int>();

        public int ObtenerNumeroSiguiente(string tipoSGBD)
        {
            if (!contadorConexiones.ContainsKey(tipoSGBD))
            {
                contadorConexiones[tipoSGBD] = 1; // Si es la primera conexión, empieza en 1
            }
            else
            {
                contadorConexiones[tipoSGBD]++; // Aumenta el contador para el siguiente
            }

            return contadorConexiones[tipoSGBD];
        }

        public void AgregarConexion(ConexionBase conexion)
        {
            string tipoSGBD = conexion.TipoSGBD;

            if (!contadorConexiones.ContainsKey(tipoSGBD))
            {
                contadorConexiones[tipoSGBD] = 1; // Iniciar en 1 para la primera conexión
            }

            // Generar el identificador en el formato "TipoSGBD 1", "TipoSGBD 2", etc.
            string identificador = $"{tipoSGBD} {contadorConexiones[tipoSGBD]}";

            // Crear una nueva instancia de SGBDConexion con el identificador generado
            SGBDConexion nuevaConexion = new SGBDConexion(conexion)
            {
                Identificador = identificador,
                TipoSGBD = tipoSGBD
            };

            // Agregar la conexión a la lista de conexiones activas
            conexionesActivas.Add(nuevaConexion.Conexion);


            // Incrementar el contador para el siguiente identificador
            contadorConexiones[tipoSGBD]++;
        }

        public ConexionBase ObtenerConexion(int index)
        {
            if (index >= 0 && index < conexionesActivas.Count)
            {
                return conexionesActivas[index];
            }
            return null;
        }

        public ConexionBase ObtenerConexionPorTipo(string tipoSGBD)
        {
            // Filtra las conexiones activas por tipo de SGBD
            return conexionesActivas
                .FirstOrDefault(c => c.TipoSGBD != null && c.TipoSGBD.Equals(tipoSGBD, StringComparison.OrdinalIgnoreCase));
        }

        // Método para obtener una lista de conexiones por tipo de SGBD
        public List<ConexionBase> ObtenerConexionesPorTipo(string tipoSGBD)
        {
            return conexionesActivas
                .Where(c => c.TipoSGBD != null && c.TipoSGBD.Equals(tipoSGBD, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }



        // Método para cerrar todas las conexiones activas
        public void CerrarTodasLasConexiones()
        {
            foreach (var conexion in conexionesActivas)
            {
                conexion.Cerrar();
            }
            conexionesActivas.Clear();
        }

        public void MostrarConexionesActivas()
        {
            if (conexionesActivas.Count == 0)
            {
                MessageBox.Show("No hay conexiones activas.");
                return;
            }

            StringBuilder mensaje = new StringBuilder();
            mensaje.AppendLine("Conexiones activas:");

            foreach (var conexion in conexionesActivas)
            {
                string estado = conexion.EstaAbierta ? "Abierta" : "Cerrada";
                mensaje.AppendLine($"ID: {conexion.Identificador}, Tipo SGBD: {conexion.TipoSGBD}, Estado: {estado}");
            }

            MessageBox.Show(mensaje.ToString(), "Conexiones Activas");
        }

    }


}
