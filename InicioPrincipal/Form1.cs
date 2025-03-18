using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using libreriaConsultasBD;

namespace InicioPrincipal
{
    public partial class Form1 : Form
    {

       //GestorConexiones gestorConexiones = new GestorConexiones();
        private GestorConexiones gestorConexiones;
        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.Add("Windows Authentication");
            comboBox1.Items.Add("SQL Server Authentication");
            comboBox1.SelectedIndex = 0;
            textBox1.Visible = false;
            label6.Visible = false;
            gestorConexiones = new GestorConexiones();
        }

        private async void btnConectar_Click(object sender, EventArgs e)
        {
            string servidor = tbServidor.Text;
            string usuario = tbUsuario.Text;
            string password = tbContraseña.Text;
            string modoAuth = comboBox1.SelectedItem?.ToString();
            string sgbdSeleccionado = comboBoxSGBD.SelectedItem?.ToString();
            string baseDatos = textBox1.Text;



            if (string.IsNullOrWhiteSpace(servidor) || string.IsNullOrWhiteSpace(sgbdSeleccionado) ||
                (modoAuth != "Windows Authentication" &&
                 (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))))
            {
                MessageBox.Show("Por favor, ingrese todos los datos requeridos.");
                return;
            }

            bool conexionExitosa = false;
            ConexionBase conexion = null;

            switch (sgbdSeleccionado)
            {
                case "SQL Server":
                    conexion = new ConexionSQLServer(servidor, usuario, password, modoAuth);
                    conexionExitosa = conexion.AbrirConexion();
                    break;
                case "MySQL":
                    conexion = new ConexionMySQL(servidor, usuario, password);
                    conexionExitosa = conexion.AbrirConexion();
                    break;
                case "Oracle":
                    conexion = new ConexionOracle(servidor, usuario, password);
                    conexionExitosa = conexion.AbrirConexion();
                    break;
                case "Firebird":
                    conexion = new ConexionFirebird(servidor,usuario,password,baseDatos);
                    conexionExitosa = conexion.AbrirConexion();
                    break;
                case "PostgreSQL":
                    conexion = new ConexionPostgreSQL(servidor,usuario,password,baseDatos);
                    conexionExitosa = conexion.AbrirConexion();
                    break;
                default:
                    MessageBox.Show("Seleccione un SGBD válido.");
                    return;
            }

            if (conexionExitosa)
            {
                // Crear una nueva instancia de SGBDConexion
                var nuevaConexion = new SGBDConexion(conexion)
                {
                    TipoSGBD = sgbdSeleccionado,
                    BaseDatos = baseDatos
                };

                // Agregar la nueva conexión al gestor
                gestorConexiones.AgregarConexion(nuevaConexion.Conexion);

                // Mostrar el formulario TablaBD
                TablaBD forms = new TablaBD(sgbdSeleccionado, servidor, modoAuth, usuario, password, baseDatos, gestorConexiones);
                forms.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se pudo establecer la conexión con el servidor.");
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Windows Authentication")
            {
                tbUsuario.Enabled = false;
                tbContraseña.Enabled = false;
            }
            else
            {
                tbUsuario.Enabled = true;
                tbContraseña.Enabled = true;
            }
        }

        private void comboBoxSGBD_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string seleccionado = comboBoxSGBD.SelectedItem.ToString().ToLower();

                if (seleccionado == "postgresql" || seleccionado == "firebird")
                {
                    textBox1.Visible = true;
                    label6.Visible = true;
                }
                else
                {
                    textBox1.Visible = false;
                    label6.Visible = false;
                }
            }
        }
    }
}
