using System;
using System.Collections.Generic;
using System.Text;

namespace BarbershopApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return $"{FullName} - {Phone}";
        }
    }
}
