using System;
using System.Collections.Generic;
using Simulation.Checkout.Domain;
using Bogus;
using System.Text;

namespace TestProject3.Simulation.Checkout.Tests.TestData
{
    public static class FakerDataWithBogus
    {

        public static Order BogusValidCustomer()
        {
            var order = TestDataFactory.CreateValidOrder();
            Faker <Order> f = new Faker<Order>()
                .UseSeed(1234)
                .RuleFor(c => c.CustomerName, f => f.Name.FullName())
                .RuleFor(c => c.CustomerEmail, (f, c) => f.Internet.Email(c.CustomerName));
            var generatedCuctomer= f.Generate();

            return new Order
            {
                CustomerName = generatedCuctomer.CustomerName,
                CustomerEmail = generatedCuctomer.CustomerEmail,
                Items = order.Items
            };
        }
    }
}
