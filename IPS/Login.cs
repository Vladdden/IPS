using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace IPS
{
    public partial class Login : Form
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        static bool flag = false;
        UserInfo userInfo;
        public Login()
        {
            InitializeComponent();
        }

        private void Enter_IPS_btn_Click(object sender, EventArgs e)
        {
            if (flag == true)
            {
                string UserLogin = LoginField.Text;
                string UserPass = PassField.Text;
                Enter_IPS_btn.Enabled = true;
                if (UserLogin != "Введите логин" && UserPass != "Введите пароль"){
                    if (UserLogin == userInfo.AdmLogin && UserPass == userInfo.AdmPass) //Admin
                    {
                        this.Hide();
                        IPS_Adm adm = new IPS_Adm();
                        adm.Show();
                    }
                    if (UserLogin == userInfo.UsrLogin && UserPass == userInfo.UsrPass) //User
                    {
                        this.Hide();
                        IPS_Usr usr = new IPS_Usr();
                        usr.Show();
                    }
                }
                else MessageBox.Show(
                        "Заполните все поля для входа!",
                         "Ошибка",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error,
                         MessageBoxDefaultButton.Button1,
                         MessageBoxOptions.DefaultDesktopOnly);
            }
            else Enter_IPS_btn.Enabled = false;
        }

        private void Login_Load(object sender, EventArgs e)
        {
            
            if (System.IO.File.Exists("db.xml"))
            {
                using (FileStream fs = new FileStream("db.xml", FileMode.Open))
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                    DataContractSerializer ser = new DataContractSerializer(typeof(UserInfo)); // Deserialize the data and read it from the instance.
                    userInfo = (UserInfo)ser.ReadObject(reader, true);
                    reader.Close();
                }
                ConnectionTrue();
            }
            else
            {
                CompanyNameLabel.Visible = false;
            }


        }

        private void Connect_btn_Click(object sender, EventArgs e)
        {
            UserInfo user = null;
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.Unicode.GetBytes("Connected");
                // отправка сообщения
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Данные отправлены");
                // получаем ответ
                data = new byte[1024]; // буфер для получаемых данных
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                string message = builder.ToString();
                message = message.Replace("Archive_Demo", "IPS");
                FileStream file1 = new FileStream("db.xml", FileMode.Create); //создаем файловый поток
                StreamWriter writer = new StreamWriter(file1); //создаем «потоковый писатель» и связываем его с файловым потоком
                writer.Write(message); //записываем в файл
                writer.Close(); //закрываем поток. Не закрыв поток, в файл ничего не запишется
                Console.WriteLine("Данные записаны в файлы");
                file1.Close();
                using (FileStream fs = new FileStream("db.xml", FileMode.Open))
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                    DataContractSerializer ser = new DataContractSerializer(typeof(UserInfo)); // Deserialize the data and read it from the instance.
                    userInfo = (UserInfo)ser.ReadObject(reader, true);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
            ConnectionTrue();
        }
         
        private void ConnectionTrue()
        {
            CompanyNameLabel.Text = userInfo.CompanyName;
            CompanyNameLabel.Visible = true;
            Sync_label.Text = "ДА";
            Sync_label.ForeColor = Color.Green;
            Connect_btn.BackColor = SystemColors.Control;
            Connect_btn.Enabled = false;
            flag = true;
        }
        private void LoginField_Enter(object sender, EventArgs e)
        {
            if (LoginField.Text == "Введите логин")
            {
                LoginField.Text = "";
                LoginField.ForeColor = Color.Black;
            }
        }

        private void LoginField_Leave(object sender, EventArgs e)
        {
            if (LoginField.Text == "")
            {
                LoginField.Text = "Введите логин";
                LoginField.ForeColor = Color.Gray;
            }
        }

        private void PassField_Enter(object sender, EventArgs e)
        {
            if (PassField.Text == "Введите пароль")
            {
                PassField.Text = "";
                PassField.UseSystemPasswordChar = true;
                PassField.ForeColor = Color.Black;
            }
        }

        private void PassField_Leave(object sender, EventArgs e)
        {
            if (PassField.Text == "")
            {
                PassField.UseSystemPasswordChar = false;
                PassField.Text = "Введите пароль";
                PassField.ForeColor = Color.Gray;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        
            const string url = "https://google.ru";
            const string browserPath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            Process.Start(browserPath, url);
        }
    }

    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string CompanyName;
        [DataMember]
        public string CompanyID;
        [DataMember]
        public string AdmLogin;
        [DataMember]
        public string AdmPass;
        [DataMember]
        public string UsrLogin;
        [DataMember]
        public string UsrPass;

        public UserInfo(string companyName, string companyID, string admLogin, string admPass, string usrLogin, string usrPass)
        {
            CompanyName = companyName;
            CompanyID = companyID;
            AdmLogin = admLogin;
            AdmPass = admPass;
            UsrLogin = usrLogin;
            UsrPass = usrPass;
        }
    }
}
