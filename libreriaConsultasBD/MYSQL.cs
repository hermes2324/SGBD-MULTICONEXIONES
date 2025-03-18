using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace libreriaConsultasBD
{
    public class ConexionMySQL : ConexionBase
    {
        private MySqlConnection conexion;

        public ConexionMySQL(string servidor, string usuario, string password) : base("MySQL")
        {
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.Password = password;
        }

        public override bool AbrirConexion()
        {
            try
            {
                // Verifica si la conexión ya está abierta
                if (conexion != null && conexion.State == ConnectionState.Open)
                {
                    Console.WriteLine("La conexión MySQL ya está abierta.");
                    return true;  // Ya está abierta, no es necesario hacer nada
                }

                // Cierra la conexión si está abierta antes de intentar abrir una nueva
                if (conexion != null && conexion.State == ConnectionState.Open)
                {
                    conexion.Close();
                }

                // Crear una nueva instancia de MySQL Connection con los parámetros correctos
                string connectionString = $"Server={Servidor};User Id={Usuario};Password={Password};";
                conexion = new MySqlConnection(connectionString);

                // Abre la conexión
                conexion.Open();

                return conexion.State == ConnectionState.Open;
            }
            catch (MySqlException ex)
            {
                // Maneja errores de MySQL
                Console.WriteLine($"Error al abrir la conexión MySQL: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Maneja errores generales
                Console.WriteLine($"Error general: {ex.Message}");
                return false;
            }
        }

        public override DataTable EjecutarConsulta(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                if (conexion == null || conexion.State != ConnectionState.Open)
                    throw new InvalidOperationException("La conexión MySQL no está abierta.");

                using (MySqlCommand command = new MySqlCommand(query, conexion))
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar consulta MySQL: {ex.Message}");
            }
            return dataTable;
        }

        public override void Cerrar()
        {
            if (conexion != null && conexion.State == ConnectionState.Open)
            {
                conexion.Close();
            }
        }

        public async Task CargarBasesDeDatosYTablasMYSQL(TreeView treeView)
        {
           // treeView.Nodes.Clear();
            TreeNode rootNode = new TreeNode("BASES DE DATOS MYSQL");
            treeView.Nodes.Add(rootNode);

            try
            {
                if (AbrirConexion())
                {
                    string query = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA";
                    DataTable dbTable = EjecutarConsulta(query);

                    foreach (DataRow row in dbTable.Rows)
                    {
                        string dbName = row["SCHEMA_NAME"].ToString();
                        TreeNode dbNode = new TreeNode(dbName);
                        rootNode.Nodes.Add(dbNode);

                        string queryTables = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{dbName}'";
                        DataTable tables = EjecutarConsulta(queryTables);

                        foreach (DataRow tableRow in tables.Rows)
                        {
                            string tableName = tableRow["TABLE_NAME"].ToString();
                            TreeNode tableNode = new TreeNode(tableName);
                            dbNode.Nodes.Add(tableNode);

                            string queryColumns = $"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{dbName}' AND TABLE_NAME = '{tableName}'";
                            DataTable columns = EjecutarConsulta(queryColumns);

                            foreach (DataRow columnRow in columns.Rows)
                            {
                                string columnName = columnRow["COLUMN_NAME"].ToString();
                                string dataType = columnRow["DATA_TYPE"].ToString();
                                string length = columnRow["CHARACTER_MAXIMUM_LENGTH"].ToString();
                                string isNullable = columnRow["IS_NULLABLE"].ToString();

                                string columnDetails = $"{columnName} ({dataType}, {length}, Null: {isNullable})";
                                tableNode.Nodes.Add(new TreeNode(columnDetails));
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No se pudo conectar a MySQL.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar bases de datos y tablas MySQL: {ex.Message}");
            }
            finally
            {
                Cerrar();
            }
        }

        public async Task<List<string>> ObtenerBasesDeDatosAsyncmysql()
        {
            List<string> basesDeDatos = new List<string>();

            try
            {
                AbrirConexion();

                // Consulta para obtener las bases de datos
                string query = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA";

                // Ejecutar la consulta
                using (MySqlCommand command = new MySqlCommand(query, conexion))
                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Agregar el nombre de la base de datos a la lista
                        string nombreBaseDeDatos = reader["SCHEMA_NAME"].ToString();
                        basesDeDatos.Add(nombreBaseDeDatos);
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Lanzar una excepción personalizada o registrar el error
                throw new Exception($"Error MySQL ({ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                // Lanzar una excepción personalizada o registrar el error
                throw new Exception($"Error general: {ex.Message}");
            }

            return basesDeDatos;
        }

        public async Task<DataTable> EjecutarConsultaEnBaseSeleccionadaMySQL(string baseDeDatos, string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                AbrirConexion();
                // Cambiar la base de datos antes de ejecutar la consulta (esto es específico de MySQL)
                string useDatabaseQuery = $"USE {baseDeDatos}";
                await Task.Run(() => EjecutarConsulta(useDatabaseQuery));  // Ejecutar el cambio de base de datos

                // Ejecutar la consulta SQL
                using (MySqlCommand command = new MySqlCommand(query, conexion))
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    await Task.Run(() => adapter.Fill(dataTable));  // Llenar el DataTable
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Error al ejecutar consulta MySQL: {ex.Message}", "Error MySQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }



    }
}
