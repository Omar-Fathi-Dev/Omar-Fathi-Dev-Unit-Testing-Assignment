using DriveNow.Infrastructure.Persistence;
using DriveNow.Infrastructure.Repositories;
using Xunit;

namespace DriveNow.UnitTests;

public class PromoCodeRepositoryTests
{
    [Fact]
    public void GetByCode_LowercaseCode_ReturnsMatchingPromoCode()
    {
        // Arrange
        TimeProvider timeProvider = TimeProvider.System;
        InMemoryDatabase database = new InMemoryDatabase(timeProvider);
        PromoCodeRepository sut = new PromoCodeRepository(database);

        // Act
        var result = sut.GetByCode("save15");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SAVE15", result.Code);
    }
}