using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace IPS
{
    public partial class Form1 : Form
    {


        private static string ServerName = @"localhost\sqlexpress";
        private static string Port = "3306";
        private static string Username = @"Владислав-ПК\\Владислав";
        private static string Password = "";
        private static string DBName = "IPSArchive";
        //private static string connectionString = @"server=" + ServerName + ";port=" + Port + ";username=" + Username + ";password=" + Password + ";database=" + DBName + ";";
        private static string connectionString = @"Data Source=ВЛАДИСЛАВ-ПК\SQLEXPRESS;Initial Catalog=IPSArchive;Integrated Security=True";
        //private string connectionString = "Data Source=MyData.sdf;Encrypt Database=True;Password=myPassword;File Mode=shared read;Persist Security Info=False;";
        //private string connectionString = "Data Source = MyData.sdf; Max Database Size=256;Persist Security Info=False;";
        //private string connectionString = "User=SYSDBA;Password=masterkey;Database=SampleDatabase.fdb;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime = 15; Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size = 8192;ServerType=0;";
        private SqlConnection conn;
        private SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dtMain;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //SqlCeEngine en = new SqlCeEngine(connectionString);
            //en.CreateDatabase();
            conn = new SqlConnection(connectionString);
            conn.Open();
            /* 0000
            using (var command = new FbCommand("select * from demo", connection, transaction))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var values = new object[reader.FieldCount];
                        reader.GetValues(values);
                        Console.WriteLine(string.Join("|", values));
                    }
                }
            }
            0000 */ 
            // connect to server
            // to database "master" to check if our database exists
            // to create it if it isn't exists
            Console.WriteLine("Подключение");

            {
                Console.WriteLine("Подключение открыто");
                Console.WriteLine("Свойства подключения:");
                Console.WriteLine("\tСтрока подключения: {0}", conn.ConnectionString);
                Console.WriteLine("\tБаза данных: {0}", conn.Database);
                Console.WriteLine("\tСервер: {0}", conn.DataSource);
                Console.WriteLine("\tВерсия сервера: {0}", conn.ServerVersion);
                Console.WriteLine("\tСостояние: {0}", conn.State);
                //Console.WriteLine("\tWorkstationld: {0}", conn.WorkstationId);
            }
            // create database if not exists
            /*
            try
            {
                using (cmd = new FbCommand(cmd_start, conn, transaction))
                {
                    Console.WriteLine(1);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // закрываем подключение
                conn.Close();
                Console.WriteLine("Подключение закрыто...");
            }
            */
            ///////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                using (cmd = new SqlCommand(String.Format("CREATE DATABASE [{0}] ON (" +
                                                    "    NAME = {0}, " +
                                                    "    FILENAME = '" + Application.StartupPath + "\\{0}.mdf'" +
                                                    ");",
                                                    DBName), conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // закрываем подключение
                conn.Close();
                Console.WriteLine("Подключение закрыто...");
            }

            // connect to db
            /////conn = new SqlCeConnection(connectionString);
            conn.Open();

            // create table "Table 1" if not exists
            using (SqlCommand cmd = new SqlCommand(String.Format(
                                      "IF NOT EXISTS (" +
                                      "    SELECT [name] " +
                                      "    FROM sys.tables " +
                                      "    WHERE [name] = '{0}'" +
                                      ") " +
                                      "CREATE TABLE [{0}] (" +
                                      "    id [INT] IDENTITY(1,1) PRIMARY KEY CLUSTERED, " +
                                      "    [text column] [TEXT] NULL, " +
                                      "    [int column] [INT] NULL " +
                                      ")",
                                      "Table 1"), conn))
            {
                cmd.ExecuteNonQuery();
            }
            
            // get all tables from DB
            using (DataTable dt = conn.GetSchema("Tables"))
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i].ItemArray[dt.Columns.IndexOf("TABLE_TYPE")].ToString() == "BASE TABLE")
                    {
                        comboBoxTables.Items.Add(dt.Rows[i].ItemArray[dt.Columns.IndexOf("TABLE_NAME")].ToString());
                    }
                }
            }//dtMain.Columns["id"].ReadOnly = true; // deprecate id field edit to prevent exceptions*/
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxTables.SelectedItem == null) return;
            adapter = new SqlDataAdapter("SELECT * FROM [" + comboBoxTables.SelectedItem.ToString() + "]", conn);
            new SqlCommandBuilder(adapter);
            dtMain = new DataTable();
            adapter.Fill(dtMain);
            dataGridView1.DataSource = dtMain;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (adapter == null) return;

            adapter.Update(dtMain);
        }

        // show tooltip (not intrusive MessageBox) when user trying to input letters into INT column cell
        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (dtMain.Columns[e.ColumnIndex].DataType == typeof(Int64) ||
                dtMain.Columns[e.ColumnIndex].DataType == typeof(Int32) ||
                dtMain.Columns[e.ColumnIndex].DataType == typeof(Int16))
            {
                Rectangle rectColumn;
                rectColumn = dataGridView1.GetColumnDisplayRectangle(e.ColumnIndex, false);

                Rectangle rectRow;
                rectRow = dataGridView1.GetRowDisplayRectangle(e.RowIndex, false);

                toolTip1.ToolTipTitle = "This field is for numbers only.";
                toolTip1.Show(" ",
                          dataGridView1,
                          rectColumn.Left, rectRow.Top + rectRow.Height);
            }
        }

        private string cmd_start = @"

        ";
    }
}
