namespace DAL.Entities
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class ApplicationContext : DbContext
    {
        
        public ApplicationContext()
            : base("name=ApplicationContext")
        {
        }

        public DbSet<Contact> Contacts { get; set; }
    }
}


