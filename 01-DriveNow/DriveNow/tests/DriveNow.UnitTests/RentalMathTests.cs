using DriveNow.Domain.Calculations;
using FluentAssertions;
using Xunit;

namespace DriveNow.UnitTests;

public class RentalMathTests
{
    
    [Fact]
    [Trait("Rule", "BR-01")]
    public void CalculateAge_BirthdayHasNotOccurred_ReturnsPreviousAge()
    {
        // Arrange
        DateTime birthDate = new DateTime(2001, 8, 11);
        DateTime onDate = new DateTime(2026, 5, 3);

        // Act
        int result = RentalMath.CalculateAge(birthDate, onDate);

        // Assert
        result.Should().Be(24);
    }
    
    [Theory]
    [Trait("Rule", "BR-01")]
    [InlineData(11 , 25)]
    [InlineData(12 , 25)]
    public void CalculateAge_BirthdayHasOccurred_ReturnsCorrectAge(int day , int expectedAge)
    {
        // Arrange
        DateTime birthDate = new DateTime(2001, 8, 11);
        DateTime onDate = new DateTime(2026, 8, day);

        // Act
        int result = RentalMath.CalculateAge(birthDate, onDate);

        // Assert
        result.Should().Be(expectedAge);
    }
    
    [Fact]
    [Trait("Rule", "BR-01")]
    public void CalculateAge_SameDay_ReturnsCorrectAge()
    {
        // Arrange
        DateTime birthDate = new DateTime(2001, 8, 11);
        DateTime onDate = birthDate.AddYears(25);

        // Act
        int result = RentalMath.CalculateAge(birthDate, onDate);

        // Assert
        result.Should().Be(25);
    }
    
    [Theory]
    [Trait("Rule", "BR-01")]
    [InlineData(28, 23)]
    [InlineData(29, 24)]
    public void CalculateAge_InLeapYear_ReturnsCorrectAge(int day, int expectedAge)
    {
        // Arrange
        DateTime dateOfBirth = new DateTime(2000, 2, 29);
        DateTime onDate = new DateTime(2024, 2, day);

        // Act
        int result = RentalMath.CalculateAge(dateOfBirth, onDate);

        // Assert
        result.Should().Be(expectedAge);
    }
    [Theory]
    [Trait("Rule", "BR-01")]
    [InlineData(27, 22)]
    [InlineData(28, 23)]
    public void CalculateAge_InNonLeapYear_ReturnsCorrectAge(int day, int expectedAge)
    {
        // Arrange
        DateTime dateOfBirth = new DateTime(2000, 2, 29);
        DateTime onDate = new DateTime(2023, 2, day);

        // Act
        int result = RentalMath.CalculateAge(dateOfBirth, onDate);

        // Assert
        result.Should().Be(expectedAge);
    }
    
    
    [Theory]
    [Trait("Rule", "BR-02")]
    [InlineData(2024,5,15, 2020, 5, 14, 4)]
    [InlineData(2024,5,15, 2020, 5, 15, 4)]
    [InlineData(2024,5,15, 2020, 5, 16, 3)]
    [InlineData(2023,2,27, 2020, 2, 29, 2)]
    [InlineData(2023,2,28, 2020, 2, 29, 3)]
    public void CalculateLicenseYears_BoundaryDates_ReturnsCorrectYears(int onYear,int onMonth, int onDay, int year, int month, int day, int expectedYears)
    {
        // Arrange
        DateTime issueDate = new DateTime(year, month, day);
        DateTime onDate = new DateTime(onYear, onMonth, onDay);

        // Act
        int result = RentalMath.CalculateLicenseYears(issueDate, onDate);

        // Assert
        result.Should().Be(expectedYears);
    }

    [Fact]
    [Trait("Rule", "BR-02")]
    public void CalculateLicenseYears_SameDay_ReturnsZeroYears()
    {
        // Arrange
        DateTime issueDate = new DateTime(2024, 5, 15);
        DateTime onDate = issueDate;

        // Act
        int result = RentalMath.CalculateLicenseYears(issueDate, onDate);

        // Assert
        result.Should().Be(0);
    }

    
    [Theory]
    [Trait("Rule", "BR-03")]
    [InlineData(9)]
    [InlineData(10)]
    public void CalculateRentalDays_ReturnAtBeforeOrEqualPickup_ThrowsArgumentException(int hour)
    {
        // Arrange
        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);
        DateTime returnAt = new DateTime(2026, 11, 1, hour, 0, 0);

        // Act
        Action act = () => RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Return must be after pickup");
    }
    
    [Theory]
    [Trait("Rule", "BR-03")]
    [InlineData(3, 0, 1)]
    [InlineData(24, 0, 1)]
    [InlineData(24, 59, 1)]
    [InlineData(25, 0, 1)]
    [InlineData(25, 1, 2)]
    [InlineData(48, 1, 2)]
    [InlineData(72, 61, 4)]
    
    public void CalculateRentalDays_ValidDuration_ReturnsCorrectDays(int hours, int minutes, int expectedDays)
    {
        // Arrange
        DateTime pickup = new DateTime(2026, 11, 1, 10, 0, 0);
        DateTime returnAt = pickup.AddHours(hours).AddMinutes(minutes);

        // Act
        int result = RentalMath.CalculateRentalDays(pickup, returnAt);

        // Assert
        result.Should().Be(expectedDays);
    }

    
    
    
    
    
    
    
}