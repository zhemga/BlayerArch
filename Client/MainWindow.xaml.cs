using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using ContactDLL;

namespace Client
{
    public partial class MainWindow : Window
    {
        ObservableCollection<ContactDTO> contactDTOs;

        private const int port = 2020;

        public MainWindow()
        {
            InitializeComponent();

            RefreshList();
            contactDTOs = new ObservableCollection<ContactDTO>();
            listBox.ItemsSource = contactDTOs;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var contact = new ContactDTO
            {
                Email = tbEmail.Text,
                Name = tbName.Text,
                Phone = tbPhone.Text,
                Tag = "Create"
            };

            if (contactDTOs.Where(x => x.Name == contact.Name && x.Phone == contact.Phone && x.Email == contact.Email).FirstOrDefault() == null)
            {
                var client = new TcpClient(Dns.GetHostName(), port);
                try
                {
                    using (var stream = client.GetStream())
                    {
                        var serializer = new XmlSerializer(contact.GetType());
                        serializer.Serialize(stream, contact);
                    }

                    client.Close();

                    RefreshList();
                }
                catch (SocketException ex)
                {
                    client.Close();
                    MessageBox.Show(ex.Message);
                }
                catch (System.InvalidOperationException ex)
                {
                    client.Close();
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Error: Duplicate contact.");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            var contact = new ContactDTO
            {
                Tag = "Read"
            };


            var client = new TcpClient(Dns.GetHostName(), port);
            try
            {
                using (var stream = client.GetStream())
                {
                    var serializer = new XmlSerializer(contact.GetType());
                    serializer.Serialize(stream, contact);
                }

                client.Close();

                Thread thread = new Thread(ListenServer);
                thread.Start();
            }
            catch (SocketException ex)
            {
                client.Close();
                MessageBox.Show(ex.Message);
            }
            catch (ThreadStateException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (System.OutOfMemoryException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (System.InvalidOperationException ex)
            {
                client.Close();
                MessageBox.Show(ex.Message);
            }
        }

        private void ListenServer()
        {
            List<ContactDTO> list;
            IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
            TcpListener clientListener = new TcpListener(ip, port + 1);
            try
            {
                clientListener.Start();

                while (true)
                {
                    try
                    {
                        var server = clientListener.AcceptTcpClient();

                        var bf = new BinaryFormatter();

                        using (var stream = server.GetStream())
                        {
                            list = (List<ContactDTO>)bf.Deserialize(stream);
                            break;
                        }
                    }
                    catch (System.Exception) { }
                }

                clientListener.Stop();

                App.Current.Dispatcher.Invoke(() =>
                {
                    contactDTOs.Clear();
                    foreach (var item in list)
                    {
                        contactDTOs.Add(item);
                    }
                });
            }
            catch (SocketException ex)
            {
                clientListener.Stop();
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var contact = (ContactDTO)listBox.SelectedItem;

            contact.Tag = "Delete";

            var client = new TcpClient(Dns.GetHostName(), port);
            try
            {
                using (var stream = client.GetStream())
                {
                    var serializer = new XmlSerializer(contact.GetType());
                    serializer.Serialize(stream, contact);
                }

                client.Close();

                RefreshList();
            }
            catch (System.InvalidOperationException ex)
            {
                client.Close();
                MessageBox.Show(ex.Message);
            }
        }

        private void listBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                var tmp = (ContactDTO)listBox.SelectedItem;

                MessageBox.Show("Name: " + tmp.Name + "\nPhone: " + tmp.Phone + "\nEmail: " + tmp.Email);
            }
        }
    }
}