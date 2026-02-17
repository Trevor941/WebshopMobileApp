namespace WebshopMobileApp.Models
{
    public class OrderHeader
    {
        public int OrderId { get; set; }
        public string OrderNo { get; set; } = "";
        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        public int CustomerId { get; set; }
        public string sCustCode { get; set; } = "";
        public int RouteId { get; set; }
        public string Notes { get; set; } = "";
        public int tblOrders_tblDeliveryAddress { get; set; }
        public string UserCreated { get; set; } = "";
    }
}
