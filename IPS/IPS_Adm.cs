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
    public partial class IPS_Adm : Form
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        private static string connectionString = @"workstation id=ArchiveDB.mssql.somee.com;packet size=4096;user id=ArchiveUser;pwd=123456789;data source=ArchiveDB.mssql.somee.com;persist security info=False;initial catalog=ArchiveDB";
        private SqlConnection conn;
        private SqlCommand cmd;
        SqlDataAdapter adapter;
        DataTable dtMain;

        public IPS_Adm()
        {
            InitializeComponent();
        }

        private void Search_btn_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            SearchForm sf = new SearchForm();
            sf.Owner = this;
            sf.Show();
        }

        private void Save_btn_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Сохранить изменения?", "Сохранение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                SqlCommandBuilder cmdbl = new SqlCommandBuilder(adapter);
                adapter.Update(dtMain);
            }

            dataGridView1.Update();
            if (dr == DialogResult.No)
            {
                SqlCommandBuilder cmdbl = new SqlCommandBuilder(adapter);
                adapter.Fill(dtMain);
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

        private void IPS_Adm_Load(object sender, EventArgs e)
        {
            try
            {
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

        private void IPS_Adm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (adapter == null) return;

            adapter.Update(dtMain);
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login loginForm = new Login();
            loginForm.Show();
        }
    }
}
