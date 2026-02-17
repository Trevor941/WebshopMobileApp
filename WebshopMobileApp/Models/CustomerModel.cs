namespace WebshopMobileApp.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public string? CustomerCode { get; set; }
        public decimal WebShopMinValueThreshold { get; set; } = 0;
        public int CustomerRoute { get; set; }
    }
}
