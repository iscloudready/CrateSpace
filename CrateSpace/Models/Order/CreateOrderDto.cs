// Models/Order/CreateOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace CrateSpace.Models.Order
{
    public class CreateOrderDto
    {
        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}