using System.ComponentModel.DataAnnotations;

namespace RiceDoctor.WebApp.Models
{
    public class SearchInfor
    {
        [Required]
        public string Keywords { get; set; }

        [Required]
        public bool IsDocumentSearch { get; set; }
    }
}