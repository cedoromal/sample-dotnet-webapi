using System.ComponentModel.DataAnnotations;

namespace sample_dotnet_webapi.Models
{
    public class CsvUploadDetails
    {
        [Required]
        public string ObjName { get; set; } = "";

        [Required]
        public string UploadLink { get; set; } = "";
    }
}
