using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace libreriaConsultasBD
{
    public class ConexionPostgreSQL : ConexionBase
    {
        private NpgsqlConnection conexion;

        public ConexionPostgreSQL(string servidor, string usuario, string password, string baseDeDatos) : base ("POSTGRESQL")
        {
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.Password = password;
            this.Database = baseDeDatos;
        }

        public string Database { get; set; }
        
        public override bool AbrirConexion()
        {
            try
            {
                string connectionString = $"Host={Servidor};Username={Usuario};Password={Password};Database={Database}";
                conexion = new NpgsqlConnection(connectionString);
                conexion.Open();
                return conexion.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar a PostgreSQL: {ex.Message}");
                return false;
            }
        }

        public override DataTable EjecutarConsulta(string query)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, conexion))
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ejecutar la consulta PostgreSQL: {ex.Message}");
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


        #region CargarBasesDeDatosYTablas

        public async Task CargarBasesDeDatosYTablasAsync(TreeView treeView)
        {
            TreeNode rootNode = new TreeNode("BASES DE DATOS POSTGRESQL");
            treeView.Nodes.Add(rootNode);

            try
            {
                if (AbrirConexion())
                {
                    string query = "SELECT datname AS BaseDeDatos FROM pg_database WHERE datistemplate = false";
                    DataTable dbTable = await EjecutarConsultaAsync(query);

                    foreach (DataRow dbRow in dbTable.Rows)
                    {
                        string dbName = dbRow["BaseDeDatos"].ToString();
                        TreeNode dbNode = new TreeNode(dbName);

                        string tableQuery = $"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_catalog = '{dbName}'";
                        DataTable tableTable = await EjecutarConsultaAsync(tableQuery);

                        foreach (DataRow tableRow in tableTable.Rows)
                        {
                            string tableName = tableRow["table_name"].ToString();
                            TreeNode tableNode = new TreeNode(tableName);

                            string columnQuery = $@"
                        SELECT column_name, data_type, character_maximum_length, is_nullable 
                        FROM information_schema.columns 
                        WHERE table_name = '{tableName}' AND table_catalog = '{dbName}'";

                            DataTable columnTable = await EjecutarConsultaAsync(columnQuery);

                            foreach (DataRow columnRow in columnTable.Rows)
                            {
                                string columnName = columnRow["column_name"].ToString();
                                string dataType = columnRow["data_type"].ToString();
                                string lengthStr = columnRow["character_maximum_length"].ToString();
                                bool allowsNull = columnRow["is_nullable"].ToString() == "YES";

                                string columnDetails = $"Tipo: {dataType}, Longitud: {(string.IsNullOrEmpty(lengthStr) ? "N/A" : lengthStr)}, Null: {(allowsNull ? "Sí" : "No")}";
                                TreeNode columnNode = new TreeNode($"{columnName} ({columnDetails})");
                                tableNode.Nodes.Add(columnNode);
                            }

                            dbNode.Nodes.Add(tableNode);
                        }

                        rootNode.Nodes.Add(dbNode);
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo establecer la conexión con el servidor.", "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task<DataTable> EjecutarConsultaAsync(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                AbrirConexion();

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, conexion))
                {
                    await Task.Run(() => adapter.Fill(dataTable)); // Ejecuta la consulta en un hilo separado
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ejecutar la consulta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataTable;
        }
        #endregion
        public async Task<List<string>> ObtenerBasesDeDatosAsync()
        {
            List<string> basesDeDatos = new List<string>();

            try
            {
                if (AbrirConexion())
                {
                    string query = "SELECT datname FROM pg_database WHERE datistemplate = false";
                    DataTable dbTable = await EjecutarConsultaAsync(query);

                    foreach (DataRow dbRow in dbTable.Rows)
                    {
                        basesDeDatos.Add(dbRow["datname"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return basesDeDatos;
        }

        public async Task<DataTable> EjecutarConsultaEnBaseSeleccionadaPostgreSQL(string baseDeDatos, string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Abrir la conexión a PostgreSQL
                AbrirConexion();

                // Cambiar al esquema adecuado si es necesario (se recomienda usar 'public' si no se especifica otro)
                string switchDatabaseQuery = $"SET search_path TO public, {baseDeDatos}";  // Establecer 'public' como esquema por defecto
                await Task.Run(() => EjecutarConsulta(switchDatabaseQuery));  // Cambiar el esquema de búsqueda si es necesario

                // Ejecutar la consulta SQL
                using (NpgsqlCommand command = new NpgsqlCommand(query, conexion))
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                {
                    // Ejecutar la consulta de manera asíncrona y llenar el DataTable con los resultados
                    await Task.Run(() => adapter.Fill(dataTable));
                }
            }
            catch (NpgsqlException ex)
            {
                // Manejar los errores específicos de PostgreSQL
                MessageBox.Show($"Error al ejecutar consulta PostgreSQL: {ex.Message}", "Error PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Manejar los errores generales
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Retornar el DataTable con los resultados de la consulta
            return dataTable;
        }


    }
}

