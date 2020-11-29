using ContactDLL;
using DAL.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DAL
{
    public class DbHelper
    {
        private readonly ApplicationContext context;

        public DbHelper()
        {
            context = new ApplicationContext();
        }

        public void AddContact(Contact contact)
        {
            context.Contacts.Add(contact);
            context.SaveChanges();
        }

        public List<ContactDTO> ReadContacts()
        {
            List<ContactDTO> dTOs = new List<ContactDTO>();
            foreach (var item in context.Contacts)
            {
                dTOs.Add(new ContactDTO { Name = item.Name, Phone = item.Phone, Email = item.Email });
            }

            return dTOs;
        }

        //public void UpdateContact(Contact contact)
        //{
        //    var tmp = context.Contacts.Where(x => x.Name == contact.Name && x.Phone == contact.Phone && x.Email == contact.Email).FirstOrDefault();
        //    if (tmp.Name != contact.Name)
        //        tmp.Name = contact.Name;
        //    if (tmp.Phone != contact.Phone)
        //        tmp.Phone = contact.Phone;
        //    if (tmp.Email != contact.Email)
        //        tmp.Email = contact.Email;

        //}

        public void DelContact(Contact contact)
        {
            context.Contacts.Remove(context.Contacts.Where(x => x.Name == contact.Name && x.Phone == contact.Phone && x.Email == contact.Email).First());
            context.SaveChanges();
        }
    }
}
