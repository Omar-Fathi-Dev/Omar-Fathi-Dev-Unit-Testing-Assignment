using System.Reflection;
using System.Text.RegularExpressions;
using NetArchTest.Rules;

namespace ParkSmart.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = Assembly.Load("ParkSmart.Domain");
    private static readonly Assembly ApplicationAssembly = Assembly.Load("ParkSmart.Application");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load("ParkSmart.Infrastructure");
    private static readonly Assembly UnitTestsAssembly = Assembly.Load("ParkSmart.UnitTests");

    // R1: Domain depends on no other ParkSmart project
    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherParkSmartProject()
    {
        var result = Types.InAssembly(DomainAssembly).ShouldNot().HaveDependencyOnAny("ParkSmart.Application", "ParkSmart.Infrastructure", "ParkSmart.ConsoleApp").GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    // R2: Application must not depend on Infrastructure or ConsoleApp
    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrConsoleApp()
    {
        var result = Types.InAssembly(ApplicationAssembly).ShouldNot().HaveDependencyOnAny("ParkSmart.Infrastructure", "ParkSmart.ConsoleApp").GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    // R3: Infrastructure must not depend on ConsoleApp
    [Fact]
    public void Infrastructure_ShouldNotDependOnConsoleApp()
    {
        var result = Types.InAssembly(InfrastructureAssembly).ShouldNot().HaveDependencyOn("ParkSmart.ConsoleApp").GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    // R4: Interfaces start with 'I'
    [Fact]
    public void Interfaces_Should_StartWithI()
    {
        var result = Types.InAssemblies([DomainAssembly, ApplicationAssembly, InfrastructureAssembly]).That().AreInterfaces().Should().HaveNameStartingWith("I").GetResult();
        result.IsSuccessful.Should().BeTrue();
    }

    // R4: Repositories end with 'Repository' and live in Repositories namespace
    [Fact]
    public void Repositories_ShouldBeInRepositoriesNamespace()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceEndingWith("Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ClassesInRepositoriesNamespace_ShouldEndWithRepository()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ResideInNamespaceEndingWith("Repositories")
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    // R4: Services end with 'Service' and live in Services namespace
    [Fact]
    public void Services_ShouldBeInServicesNamespace()
    {
        var result = Types.InAssemblies(
                [DomainAssembly, ApplicationAssembly])
            .That()
            .HaveNameEndingWith("Service")
            .Should()
            .ResideInNamespaceEndingWith("Services")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ClassesInServicesNamespace_ShouldEndWithService()
    {
        var result = Types.InAssemblies(
                [DomainAssembly, ApplicationAssembly])
            .That()
            .ResideInNamespaceEndingWith("Services")
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    // R4: Exceptions end with 'Exception' and live in Domain.Exceptions
    [Fact]
    public void Exceptions_ShouldBeInExceptionsNamespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .ResideInNamespace("ParkSmart.Domain.Exceptions")
            .And()
            .HaveNameEndingWith("Exception")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ClassesInExceptionsNamespace_ShouldInheritExceptionAndEndWithException()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("ParkSmart.Domain.Exceptions")
            .Should()
            .Inherit(typeof(Exception))
            .And()
            .HaveNameEndingWith("Exception")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    // R5: Test classes must end with "Tests"
    [Fact]
    public void TestClasses_ShouldEndWithTests()
    {
        foreach (Type type in UnitTestsAssembly.GetTypes())
        {
            if (!type.IsClass)
                continue;

            bool containsTestMethod = false;

            foreach (MethodInfo method in type.GetMethods())
            {
                bool hasFact =
                    method.GetCustomAttribute<FactAttribute>() != null;

                bool hasTheory =
                    method.GetCustomAttribute<TheoryAttribute>() != null;

                if (hasFact || hasTheory)
                {
                    containsTestMethod = true;
                    break;
                }
            }

            if (containsTestMethod)
                type.Name.Should().EndWith("Tests");
        }
    }

    // R5: Test methods follow Method_Scenario_ExpectedResult
    [Fact]
    public void TestMethods_ShouldFollowNamingPattern()
    {
        Regex pattern = new Regex(
            @"^[A-Za-z0-9]+_[A-Za-z0-9]+_[A-Za-z0-9]+$");

        foreach (Type type in UnitTestsAssembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods())
            {
                bool hasFact =
                    method.GetCustomAttribute<FactAttribute>() != null;

                bool hasTheory =
                    method.GetCustomAttribute<TheoryAttribute>() != null;

                if (hasFact || hasTheory)
                {
                    pattern.IsMatch(method.Name)
                        .Should()
                        .BeTrue(
                            $"Method '{method.Name}' does not follow the naming pattern");
                }
            }
        }
    }
}