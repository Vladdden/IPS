using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace IPS
{
    public partial class IPS_Usr : Form
    {
        /*
        int num = 0; 
        // устанавливаем метод обратного вызова
        TimerCallback tm = new TimerCallback(Count);
        // создаем таймер
        Timer timer = new Timer(tm, null, 0, 2000);
            */
        const int port = 8888;
        const string address = "127.0.0.1";
        private static string connectionString = @"workstation id=ArchiveDB.mssql.somee.com;packet size=4096;user id=ArchiveUser;pwd=123456789;data source=ArchiveDB.mssql.somee.com;persist security info=False;initial catalog=ArchiveDB";
        private SqlConnection conn;
        private SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dtMain;
        public IPS_Usr()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try { 
                // connect to server
                conn = new SqlConnection(connectionString);
                conn.Open();
                Console.WriteLine("Подключение");

                {
                    Console.WriteLine("Подключение открыто");
                    Console.WriteLine("Свойства подключения:");
                    Console.WriteLine("\tСтрока подключения: {0}", conn.ConnectionString);
                    Console.WriteLine("\tБаза данных: {0}", conn.Database);
                    Console.WriteLine("\tСервер: {0}", conn.DataSource);
                    Console.WriteLine("\tВерсия сервера: {0}", conn.ServerVersion);
                    Console.WriteLine("\tСостояние: {0}", conn.State);
                }

                using (DataTable dt = conn.GetSchema("Tables"))
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i].ItemArray[dt.Columns.IndexOf("TABLE_TYPE")].ToString() == "BASE TABLE")
                        {
                            comboBoxTables.Items.Add(dt.Rows[i].ItemArray[dt.Columns.IndexOf("TABLE_NAME")].ToString());
                        }
                    }
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
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (comboBoxTables.SelectedItem == null) return;
            string q = $"SELECT * FROM [" + comboBoxTables.SelectedItem.ToString() + "]";
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

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login loginForm = new Login();
            loginForm.Show();
        }

        private void Search_btn_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            UserSearch sf = new UserSearch();
            sf.Owner = this;
            sf.Show();
        }
    }
}
