using AutoFixture;
using AutoFixture.Kernel;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;

namespace DriveNow.UnitTests.Customizations;

public sealed class ValidCustomerCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Customer>(composer => composer
            .With(customer => customer.DateOfBirth, new DateTime(1990, 1, 1))
            .With(customer => customer.LicenseIssueDate, new DateTime(2015, 1, 1))
            .With(customer => customer.LoyaltyTier, LoyaltyTier.None)
            .With(customer => customer.IsBlacklisted, false));
    }
}