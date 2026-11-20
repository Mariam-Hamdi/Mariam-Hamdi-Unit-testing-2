using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;
using System.Reflection;
using Xunit;

namespace TestProject3.Simulation.Checkout.Tests
{
    public class ArchitectureTests
    { 
        private static readonly Assembly DomainAssembly = typeof(Order).Assembly;
        private static readonly Assembly ApplicationAssembly = typeof(CheckoutService).Assembly;
        private static readonly Assembly InfrastructureAssembly =
            Assembly.Load("Simulation.Checkout.Infrastructure");

        // 5.1
        [Fact]
        public void Domain_ShouldNotDependOn_ApplicationOrInfrastructure()
        {
            // Types.InAssembly  هات كل الكلاسز و الانواع اللي جوا ---> DomainAssembly

            var res = Types.InAssembly(DomainAssembly)
                .ShouldNot()
                .HaveDependencyOnAny("Simulation.Checkout.Application",
                "Simulation.Checkout.Infrastructure")
                .GetResult();

           res.IsSuccessful.Should().BeTrue();
        }

        // 5.2
        [Fact]
        public void Application_ShouldNotDependOn_Infrastructure()
        {

            //Types.InAssembly هات كل انواع الكلاسز اللي في  ---> ApplicationAssembly

            var res = Types.InAssembly(ApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn("Simulation.Checkout.Infrastructure")
                .GetResult();

            res.IsSuccessful.Should().BeTrue();
        }

        //5.3
        [Fact]
        public void All_Interfaces_start_with_I_ShouldBeTrue()
        {
            // Types.InAssembly هات كل ال ---> interfaces ---> in ApplicationAssembly or in any project of three

            //var res = Types.InAssembly(ApplicationAssembly)
            var res = Types.InAssemblies(new[]{ApplicationAssembly,DomainAssembly,InfrastructureAssembly})
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            res.IsSuccessful.Should().BeTrue();
        }


        // 5.4
        [Fact]
        public void Classes_Thatimplement_IOrderRepository_End_With_Repository__ShouldBeTrue()
        {
            // Types.InAssembly هات كل ال ---> classess ---> Those Impelement this interface

            var res = Types.InAssemblies(new[] { ApplicationAssembly, DomainAssembly, InfrastructureAssembly })
                .That()
                .ImplementInterface(typeof(IOrderRepository))
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            res.IsSuccessful.Should().BeTrue();
        }
    }
}
