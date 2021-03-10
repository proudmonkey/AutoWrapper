using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoWrapper.Tests.Models
{
    public class TestModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }

        public StatusType Type { get; set; }

        public enum StatusType
        {
            Unknown,
            Active,
            InActive
        }
    }
}
