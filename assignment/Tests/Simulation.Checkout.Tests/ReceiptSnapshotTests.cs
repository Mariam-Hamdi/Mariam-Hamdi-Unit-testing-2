using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerifyTests;

namespace TestProject3.Simulation.Checkout.Tests
{
    /// <summary>
            ///  Uses Verify instead of writing many assersts, compares the test output 
            ///  with saved .verified.txt file on every to cach unexpected changes 
    /// </summary>

    public class ReceiptSnapshotTests
    {
        [Fact]
        public async Task ExampleC_Receipt_MatchesSnapshot()
        {

            var itemspip = new List<ReceiptLine>
{
    new()
    {
        ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        ProductName = "Mouse",
        UnitPrice = 500m,
        Quantity = 3,
        LineTotal = 1500m
    }
};
            var fixedReceipt = new CheckoutReceipt
            {
                OrderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CustomerName = "Mariam Hamdi",
                CustomerEmail = "mariam@example.com",
                Items = itemspip,
                Subtotal = 1500m,
                Discount = 100m,
                Total = 1400m,
                CreatedAt = new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero)
            };

            await Verify(fixedReceipt)
            .IgnoreMember<CheckoutReceipt>(x => x.OrderId)
            .IgnoreMember<CheckoutReceipt>(x => x.CreatedAt);
        }
    }
    }


