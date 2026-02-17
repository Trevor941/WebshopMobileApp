namespace WebshopMobileApp.Models
{
    public class DeliverTo
    {
        public string? OnlineEmailAddress { get; set; }
        public string? Firstname { get; set; }
        public string? Surname { get; set; }
        public string? Password { get; set; }
        public int tblCustomer_tblRoutesSetItem { get; set; } = 0;
        public int DeliveryAddressesId { get; set; } = 0;
        public bool? UniqueDeliveryAddresses { get; set; } = false;
    }
}
