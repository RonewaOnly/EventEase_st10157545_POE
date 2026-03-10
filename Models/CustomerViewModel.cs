using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class CustomerViewModel
    {
        public string CustomerId { get; set; }

        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email {  get; set; }

        [DataType(DataType.PhoneNumber)]
        public string phoneNumber {  get; set; }
    }
}
