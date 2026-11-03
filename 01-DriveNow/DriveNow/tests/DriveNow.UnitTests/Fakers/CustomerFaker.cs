using Bogus;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Fakers;

public class CustomerFaker : Faker<Customer>
{
    public CustomerFaker()
    {
        UseSeed(123);

        RuleFor(customer => customer.FullName, faker => faker.Name.FullName());
        RuleFor(customer => customer.Email, faker => faker.Internet.Email());
        RuleFor(customer => customer.DateOfBirth,
            faker => faker.Date.Between(
                new DateTime(1970, 1, 1),
                new DateTime(2005, 1, 1)));

        RuleFor(customer => customer.LoyaltyTier,
            faker => faker.PickRandom<LoyaltyTier>());
    }

    public CustomerFaker WithAge(int age)
    {
        RuleFor(customer => customer.DateOfBirth,
            faker => DateTime.Today.AddYears(-age));

        return this;
    }

    public CustomerFaker WithTier(LoyaltyTier tier)
    {
        RuleFor(customer => customer.LoyaltyTier, tier);

        return this;
    }
}