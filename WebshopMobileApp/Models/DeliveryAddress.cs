namespace WebshopMobileApp.Models
{
    public class DeliveryAddress
    {
        public int Id { get; set; }
        public string DelAddress01 { get; set; } = string.Empty;
        public string DelAddress02 { get; set; } = string.Empty;
        public string DelAddress03 { get; set; } = string.Empty;
        public string DelAddress04 { get; set; } = string.Empty;
        public string DelAddress05 { get; set; } = string.Empty;
        public int RouteId { get; set; }

    }
}
