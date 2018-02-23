using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FlightChecker.Domain.Models
{

    public class ApiKey
    {
        [Key]
        public int Id { get; set; }
        public string ApiName { get; set; }
        public string ApiKeyValue { get; set; }
    
    }
}
