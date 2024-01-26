using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserEmail { get; set; }

        public string Action {  get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
    }
}
