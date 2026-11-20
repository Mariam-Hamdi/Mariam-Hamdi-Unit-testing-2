using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using Simulation.Checkout.Infrastructure;
using Simulation.Checkout.Application.Abstractions;
using FluentAssertions;  
using Simulation.Checkout.Application;
using System.Threading.Tasks;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using TestProject3.Simulation.Checkout.Tests.TestData;
using Shouldly;

namespace TestProject3.Simulation.Checkout.Tests

{
    public class CheckoutServiceTests
    {
        private readonly IInventoryService inven;
        private readonly INotificationService notif;
        private readonly IOrderRepository orderRepo;
        private readonly CheckoutService chser;
        public CheckoutServiceTests()
        {
            inven = Substitute.For<IInventoryService>();
            notif = Substitute.For<INotificationService>();
            orderRepo = Substitute.For<IOrderRepository>();
            chser = new CheckoutService(orderRepo, inven, notif);
        }

        // BR-01
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
        {
            // Arrange 
           
             var order = new Order
            {
                CustomerName = "",
                CustomerEmail = "",
                Items = new List<OrderItem>()
            };



            // act
         
            // Store the call in a Func to be execute it  later
            // otherwis the exception will be thrown immediately here
            Func<Task> act = () => chser.CheckoutAsync(order);

            // assert
            await act.Should().ThrowAsync<EmptyOrderException>();

            // BR 09
           await  notif.DidNotReceive().SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
           await  orderRepo.DidNotReceive().SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

        }


        // BR-02
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CheckoutAsync_WhenInvalidQuantity_ShouldThrowInvalidQuantityException(int quantity)
        {
            // Arrange 
        
            var orderitem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = 10.0m,
                Quantity = quantity
            };
            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> {orderitem}
            };


            // act

            // Store the call in a Func to be execute it  later
            // otherwis the exception will be thrown immediately here
            Func<Task> act = () => chser.CheckoutAsync(order);

            // assert
            await act.Should().ThrowAsync<InvalidQuantityException>().WithMessage("*Product 1*");

            // BR 09
            await notif.DidNotReceive().SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
            await orderRepo.DidNotReceive().SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

        }

        //BR-03
        [Theory]
        [InlineData(-1)]
        public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException(decimal price)
        {
            // Arrange 

            var orderitem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = price,
                Quantity = 2
            };
            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> { orderitem }
            };


            // act

            // Store the call in a Func to be execuvte it  later
            // otherwis the exception will be thrown immediately here
            Func<Task> act = () => chser.CheckoutAsync(order);

            // assert
            await act.Should().ThrowAsync<InvalidPriceException>().WithMessage("*Product 1*");

            // BR 09
            await notif.DidNotReceive().SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
            await orderRepo.DidNotReceive().SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

        }
        [Fact]
        public async Task CheckoutAsync_WhenPriceIsZero_ShouldSucceedWithZeroTotal()
        {
            // Arrange 

            var orderitem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = 0,
                Quantity = 2
            };
            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> { orderitem }
            };

           inven.IsAvailableAsync(orderitem.ProductId, orderitem.Quantity, Arg.Any<CancellationToken>()).Returns(true);


            // act

            var result = await chser.CheckoutAsync(order);


            // assert
            result.Total.Should().Be(0);


        }

        //BR-04
        [Fact]
        public async Task CheckoutAsync_WhenAnItemIsOutOfStock_ShouldThrowInsufficientInventoryException()
        {
            // Arrange 

            var orderitem1 = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = 0,
                Quantity = 2
            };
            var orderitem2 = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 2",
                UnitPrice = 30m,
                Quantity = 4
            };
            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> { orderitem1,orderitem2 }
            };

            inven.IsAvailableAsync(orderitem1.ProductId, orderitem1.Quantity, Arg.Any<CancellationToken>()).Returns(true);
            inven.IsAvailableAsync(orderitem2.ProductId, orderitem2.Quantity, Arg.Any<CancellationToken>()).Returns(false);


            // act

            Func<Task> act = () =>  chser.CheckoutAsync(order);


            // assert
            await act.Should().ThrowAsync<InsufficientInventoryException>().WithMessage("*Product 2*");

            // BR 09
            await notif.DidNotReceive().SendOrderConfirmationAsync(Arg.Any<CheckoutReceipt>(), Arg.Any<CancellationToken>());
            await orderRepo.DidNotReceive().SaveAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());

        }

        // BR-05
        [Fact]
        public async Task CheckoutAsync_SubTotal_ShouldbeEqual_Multiply_Price_and_Quantity()
        {
            // Arrange 

            var orderitem1 = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = 30m,
                Quantity = 2
            };
            var orderitem2 = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 2",
                UnitPrice = 40m,
                Quantity = 3
            };
            
            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> { orderitem1, orderitem2 }
            };
            inven.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

            // act
            var res=await chser.CheckoutAsync(order);


            // assert
            res.Subtotal.Should().Be(180m);
        }

        //BR-06 , BR-07
        [Theory]
        [InlineData(999.99, 1, 0)]
        [InlineData(1000,1,100)]
        [InlineData(1500,1,100)]
        public async Task CheckoutAsync_SubTotal_ExecuteDiscount(decimal price,int quantity,decimal discountexpec)
        {
            // Arrange 

            var orderitem1 = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = price,
                Quantity = quantity
            };
          

            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> { orderitem1}
            };
            inven.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

            // act
            var res = await chser.CheckoutAsync(order);



            // assert
            res.Discount.Should().Be(discountexpec);
            //BR-07
            res.Total.Should().Be(res.Subtotal - res.Discount);
        }

        //BR-08
        [Fact]
        public async Task CheckoutAsync_ShouldCall_SaveAsync_And_SendOrderConfirmationAsync()
        {
            // Arrange 
            var orderitem1 = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product 1",
                UnitPrice = 30m,
                Quantity = 2
            };
            
            var order = new Order
            {
                CustomerName = "Mariam",
                CustomerEmail = "mariam@.com",
                Items = new List<OrderItem> { orderitem1 }
            };
            inven.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);
            

            // act
            var res = await chser.CheckoutAsync(order);


            // assert
            await orderRepo.Received(1).SaveAsync(order);
            await notif.Received(1).SendOrderConfirmationAsync(res);
        }

        // BR-09 :: Executed in BR-04 && BR-03 && BR-02 && BR-01


        // Task 3 — Test data (AutoFixture)
        [Fact]
       public  async Task WhenUsingValidOrderFromFactory_ShouldSucceed()
        {
            // arrange
            var res = TestDataFactory.CreateValidOrder();
            inven.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

            // act

            var receipt = await chser.CheckoutAsync(res);

            
            // Assert
            receipt.Should().NotBeNull();
            receipt.Items[0].Quantity.Should().Be(res.Items[0].Quantity);
            receipt.Items[0].UnitPrice.Should().Be(res.Items[0].UnitPrice);
            receipt.Total.Should().BeGreaterThan(0);
             await orderRepo.Received(1).SaveAsync(res, Arg.Any<CancellationToken>());
             await notif.Received(1).SendOrderConfirmationAsync(receipt, Arg.Any<CancellationToken>());


        }

        // Task 3 — Test data (Bogus)
        [Fact]
       public  async Task WhenUsingValidCustomerFromFactory_ShouldSucceed()
        {
            // arrange
            var res = FakerDataWithBogus.BogusValidCustomer();
            inven.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

            // act

            var receipt = await chser.CheckoutAsync(res);

            
            // Assert
            receipt.Should().NotBeNull();
            receipt.CustomerName.Should().Be(res.CustomerName);
            receipt.CustomerEmail.Should().Be(res.CustomerEmail);
            receipt.Items[0].Quantity.Should().Be(res.Items[0].Quantity);
            receipt.Items[0].UnitPrice.Should().Be(res.Items[0].UnitPrice);
            receipt.Total.Should().BeGreaterThan(0);
             await orderRepo.Received(1).SaveAsync(res, Arg.Any<CancellationToken>());
             await notif.Received(1).SendOrderConfirmationAsync(receipt, Arg.Any<CancellationToken>());


        }
    }
}
