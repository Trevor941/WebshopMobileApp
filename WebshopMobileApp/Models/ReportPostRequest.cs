namespace WebshopMobileApp.Models
{
    public class ReportPostRequest
    {
        public int Id { get; set; }
        public byte[]? Data { get; set; }
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
    }
}
