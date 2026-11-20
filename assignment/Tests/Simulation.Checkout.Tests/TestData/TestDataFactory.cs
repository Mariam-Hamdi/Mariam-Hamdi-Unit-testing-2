using System;
using System.Collections.Generic;
using System;
using System.Text;
using AutoFixture;
using Simulation.Checkout.Domain;
namespace TestProject3.Simulation.Checkout.Tests.TestData
{
    /// <summary>
    ///     AutoFixture differs from Bogus in generating fake data and setting their values without rules for every property 
    ///     but Bogus without Rules for every property will generate defualt values of them.
    /// </summary>

    public class TestDataFactory
    {
        public static Order CreateValidOrder()
        {
            Fixture fix = new Fixture();
            fix.Customize<OrderItem>(c => c
                .With(item => item.Quantity, 2)
                .With(item => item.UnitPrice, 33));

            return fix.Create<Order>();
        }
    }
}
