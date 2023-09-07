
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace ClientsXml
{

    public class Clients
    {
        [XmlElement("Client")]
        public List<Client> ClientList = new List<Client>();
    }
    public class Errors
    {
        public string Type { get; set; }
        public string Count { get; set; }
    }
    public class Client
    {

        public string FIO { get; set; }
        public int RegNumber { get; set; }
        public string DiasoftID { get; set; }
        public string Registrator { get; set; }
        public int RegistratorID { get; set; }

        public static void Main(string[] args)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Clients));

            //Запись содержимого в список
            TextReader reader = new StreamReader(@"C:\\Users\\Svetik\\source\\repos\\Clients\\Clients.xml");
            object obj = deserializer.Deserialize(reader);
            Clients XmlData = (Clients)obj;
            reader.Close();
            Client[] output = XmlData.ClientList.ToArray();
            Console.WriteLine(output.Length);
            List<Client> ClientListTry = new List<Client>();


            //------------------------------------------


            //Добавление столбца в генератором ID 
            Random rnd = new Random();
            int newElement;
            var knownNumbers = new HashSet<int>();
            foreach (Client client in output)
            {
                if (client.FIO != null && client.RegNumber != 0 && client.DiasoftID != null && client.Registrator != null)
                {

                    if (client.RegistratorID == 0)
                    {
                        do
                        {
                            client.RegistratorID = rnd.Next(5000);
                        } while (!knownNumbers.Add(client.RegistratorID));


                    }
                    ClientListTry.Add(client);

                }



            }
            Console.WriteLine(ClientListTry.Count);

            //--------------------------------------------------------------------


            //Запись массива в файл, промежуточный вариант, итоговый ниже
            string path = "Clients2.xml";

            bool fileExist = File.Exists(path);
            if (fileExist)
            {
                Console.WriteLine("Файл уже создан.");
            }
            else
            {
                Console.WriteLine(ClientListTry[10].RegistratorID);


                Client[] ClietMassTry = ClientListTry.ToArray();
                int count = ClietMassTry.Length;
                Console.WriteLine(count);
                XmlSerializer users = new XmlSerializer(typeof(Client[]));
                using (FileStream fs = new FileStream("Clients2.xml", FileMode.OpenOrCreate))
                {
                    users.Serialize(fs, ClietMassTry);
                }

            }
            //Итоговый вариант записи xml
            string path2 = "Clients3.xml";

            bool fileExist2 = File.Exists(path2);
            if (fileExist2)
            {
                Console.WriteLine("Файл уже создан.");
            }
            else
            {
                XDocument doc = new XDocument();
                XElement Clients = new XElement("Clients");
                doc.Add(Clients);
                for (int i = 0; i < ClientListTry.Count; i++)
                {
                    if (Clients != null)
                    {
                        Clients.Add(
                                    new XElement("Client",
                                    new XElement("FIO", ClientListTry[i].FIO),
                                    new XElement("RegNumber", ClientListTry[i].RegNumber),
                                    new XElement("DiasoftID", ClientListTry[i].DiasoftID),
                                    new XElement("Registrator", ClientListTry[i].Registrator),
                                    new XElement("RegistratorID", ClientListTry[i].RegistratorID)));
                    }

                }
                doc.Save("Clients3.xml");

            }

            //------------------------------------------------------------------


            //Запись данных о регистраторе в отдельный xml
            string path3 = "Registrators.xml";

            bool fileExist3 = File.Exists(path3);
            if (fileExist3)
            {
                Console.WriteLine("Файл уже создан.");
            }
            else
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("Clients3.xml");
                xDoc.GetElementsByTagName("Registrator");
                Console.WriteLine(xDoc.GetElementsByTagName("Registrator")[1].InnerText);

                XDocument docReg = new XDocument();
                XElement Reg = new XElement("Registrators");
                docReg.Add(Reg);
                for (int i = 0; i < xDoc.GetElementsByTagName("Registrator").Count; i++)
                {
                    if (Reg != null)
                    {
                        Reg.Add(
                                    new XElement("Registrator",
                                    new XElement("Registrator", xDoc.GetElementsByTagName("Registrator")[i].InnerText),
                                    new XElement("RegistratorID", xDoc.GetElementsByTagName("RegistratorID")[i].InnerText)));
                    }

                }

                docReg.Save("Registrators.xml");

            }



            //---------------------------------------------
            //Итоговый вариант выявления ошибок

            var TypeErr = new Dictionary<int, string>();

            for (int i = 0; i < output.Length; i++)
            {
                if (output[i].FIO == null)
                {
                    TypeErr.Add(i, "FIO");
                }
                if (output[i].RegNumber == 0)
                {
                    TypeErr.Add(i, "RegNumber");
                }
                if (output[i].DiasoftID == null)
                {
                    TypeErr.Add(i, "DiasoftID");
                }
                if (output[i].Registrator == null)
                {
                    TypeErr.Add(i, "Registrator");
                }
                //if ((output[i].FIO == null)^(output[i].RegNumber == 0)^(output[i].DiasoftID == null)^ (output[i].Registrator == null)) { TypeErr.Add(i, output[i].); }
            }
            var Errors = TypeErr.GroupBy(x => x.Value);
            var TypeError = new Dictionary<string, int>();
            foreach (var err in Errors)
            {
                Console.WriteLine("Не указано {0}:{1} записей", err.Key, err.Count());
                TypeError.Add(err.Key, err.Count());
            }

            StreamWriter report = new StreamWriter("report.txt");
            var ordered = TypeError.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            var sum = TypeError.Sum(x => x.Value);

            Console.WriteLine(sum);

            foreach (var err2 in ordered)
            {

                Console.WriteLine("Не указано {0}:{1} записей", err2.Key, err2.Value);
                report.WriteLine("Не указано {0}:{1} записей", err2.Key, err2.Value);

            }

            report.WriteLine("Всего ошибочных записей: {0}", sum);
            report.Close();


            //--------------------------------------------------------


            //Ошибочная версия

            int FIOError = 0;
            int RegNumberError = 0;
            int DiasoftIDError = 0;
            int RegistratorError = 0;

            for (int i = 0; i < output.Length; i++)
            {
                if (output[i].FIO == null)
                {
                    FIOError = FIOError + 1;
                }
                if (output[i].RegNumber == 0)
                {
                    RegNumberError = RegNumberError + 1;
                }
                if (output[i].DiasoftID == null)
                {
                    DiasoftIDError = DiasoftIDError + 1;
                }
                if (output[i].Registrator == null)
                {
                    RegistratorError = RegistratorError + 1;
                }
            }
            var errors = new Dictionary<string, int>();

            errors.Add("Не указан FIO: ", FIOError);
            errors.Add("Не указан RegNumber: ", RegNumberError);
            errors.Add("Не указан DiasoftID: ", DiasoftIDError);
            errors.Add("Не указан Registrator: ", RegistratorError);
            Console.WriteLine($"Count: {errors.Count}");


            var groups = errors.GroupBy(x => x.Key);

            foreach (var group in groups)
            {
                Console.WriteLine("Key: {0}", group.Key);
                foreach (var key in group)
                {
                    Console.WriteLine("Error: {0}", key);
                }
            }





        }
    }

}
