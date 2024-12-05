using Microsoft.EntityFrameworkCore;

namespace BestStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string ClientId { get; set; } = string.Empty;

        public ApplicationUser Client { get; set; } = null!;

        public List<OrderItem> Items { get; set; } = new List<OrderItem> { };

        [Precision(16, 2)]
        public Decimal ShippingFee { get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus {  get; set; } = string.Empty;
        public string PaymentDetails { get; set; } = string.Empty; //Going to store paypal details in this project
        public string OrderStatus { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } 
    }
}
