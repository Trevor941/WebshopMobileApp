namespace WebshopMobileApp.Models
{
    public class CartPostRequest
    {
        public OrderHeader OrderHeader { get; set; } = new();
        public List<CartModelToPost> CartItems { get; set; } = new();
    }
}
