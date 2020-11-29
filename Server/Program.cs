using DAL;
using System;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using ContactDLL;
using System.Runtime.Serialization.Formatters.Binary;
using DAL.Entities;

namespace Server
{
    class Program
    {
        private const int port = 2020;
        private static IPAddress ip;
        private static TcpListener server;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            StartServer();
        }

        private static void StartServer()
        {
            var dbHelper = new DbHelper();
            ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];

            server = new TcpListener(ip, port);

            server.Start();

            while (true)
            {
                Console.WriteLine("Wait for connection...");
                try
                {
                    var client = server.AcceptTcpClient();
                    Console.WriteLine("Connected. ");

                    using (var stream = client.GetStream())
                    {
                        var serializer = new XmlSerializer(typeof(ContactDTO));
                        var contact = (ContactDTO)serializer.Deserialize(stream);

                        if (contact.Tag == "Create")
                        {
                            dbHelper.AddContact(new DAL.Entities.Contact
                            {
                                Email = contact.Email,
                                Name = contact.Name,
                                Phone = contact.Phone
                            });

                            Console.WriteLine("Contact was created.");
                        }
                        //else if (contact.Tag == "Update")
                        //{
                        //    dbHelper.UpdateContact(new DAL.Entities.Contact
                        //    {
                        //        Email = contact.Email,
                        //        Name = contact.Name,
                        //        Phone = contact.Phone
                        //    });

                        //    Console.WriteLine("Contact was updated.");
                        //}
                        else if (contact.Tag == "Read")
                        {
                            var serverSend = new TcpClient(Dns.GetHostName(), port + 1);

                            var bf = new BinaryFormatter();

                            using (var streamSend = serverSend.GetStream())
                            {
                                bf.Serialize(streamSend, dbHelper.ReadContacts());
                            }

                            serverSend.Close();

                            Console.WriteLine("Contacts was sent.");
                        }
                        else if (contact.Tag == "Delete")
                        {
                            dbHelper.DelContact(new Contact 
                            {
                                Email = contact.Email,
                                Name = contact.Name,
                                Phone = contact.Phone
                            });

                            Console.WriteLine("Contact was deleted.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}