using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        UserInfo userInfo;
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string UserLogin = LoginField.Text;
            string UserPass = PassField.Text;
            
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

        private void Login_Load(object sender, EventArgs e)
        {
            
            UserInfo usrInfo = new UserInfo("111", "222", "333", "333", "444", "444");
            using(FileStream fs = new FileStream("db.xml", FileMode.Create))
                {
                //formatter.Serialize(fs, usrInfo);
                DataContractSerializer ser = new DataContractSerializer(typeof(UserInfo));
                ser.WriteObject(fs, usrInfo);
                Console.WriteLine("Объект сериализован");
            }
            
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
                Console.WriteLine($"Данные получены:{message}");
                FileStream file1 = new FileStream("db1.xml", FileMode.Create); //создаем файловый поток
                StreamWriter writer = new StreamWriter(file1); //создаем «потоковый писатель» и связываем его с файловым потоком
                writer.Write(message); //записываем в файл
                writer.Close(); //закрываем поток. Не закрыв поток, в файл ничего не запишется
                Console.WriteLine("Данные записаны в файлы");
                file1.Close();
                /*
                BinaryFormatter formatter = new BinaryFormatter();
                //XmlSerializer formatter = new XmlSerializer(typeof(UserInfo));
                using (FileStream fs = new FileStream("db1.bin", FileMode.OpenOrCreate))
                {
                    //fs.Position = 0;
                    //UserInfo user = (UserInfo)formatter.Deserialize(fs);
                    //Console.WriteLine("Объект десериализован");
                    user = (UserInfo)formatter.Deserialize(fs);
                }
                */
                ////////////////////////
                using (FileStream fs = new FileStream("db1.xml", FileMode.Open))
                {
                    XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                    DataContractSerializer ser = new DataContractSerializer(typeof(UserInfo));
                    // Deserialize the data and read it from the instance.
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
            Console.WriteLine($"Данные преобразованы: {userInfo.CompanyName}");
            CompanyNameLabel.Text = userInfo.CompanyName;
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
