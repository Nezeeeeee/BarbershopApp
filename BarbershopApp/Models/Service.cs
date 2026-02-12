using System;
using System.Collections.Generic;
using System.Text;

namespace BarbershopApp.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Price:C} ({DurationMinutes} мин)";
        }
    }
}
