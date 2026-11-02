# Naming Conventions

**Name:** Omar Fathy  
**Track:** Backend .NET (Group 3)

---

## R1: Test Projects, Classes, and Files

### 1. Test Projects
- Named as `<ProjectUnderTest>.<TestType>`.
- Examples:
    - `DriveNow.UnitTests`
    - `DriveNow.ArchitectureTests`
    - `ParkSmart.UnitTests`

### 2. Test Classes & Files
- Every test class tests a single production class and must end with `Tests`.
- File name must match the test class name, following this format: `<ClassUnderTest>Tests.cs`.
- Examples:
    - `RentalMathTests.cs`
    - `PricingServiceTests.cs`

---

## R2: Test Method Naming Style

### Comparison of Known Styles

#### 1. Method_Scenario_ExpectedResult

**Format:** `MethodName_Scenario_ExpectedResult`

```csharp
[Fact]
public void Divide_DivisorIsZero_ThrowsDivideByZeroException()
```

**Good:**
- Easy to see which method is tested.
- Tests are grouped by method name, so they are easy to find.
- Very common in C# and .NET.

**Bad:**
- If you rename the method, you may need to rename its tests.
- Underscores make long names harder to read.

#### 2. Should_X_When_Y

**Format:** `Should_ExpectedResult_When_Condition`

```csharp
[Fact]
public void Should_ThrowException_When_DivisorIsZero()
```

**Good:**
- Reads like a normal sentence.
- Starts with the result, so you see the important part first.
- Does not depend on the method name, so renaming is safer.

**Bad:**
- It does not say which method is tested.
- Names can get long.

#### 3. Given_When_Then

**Format:** `Given_Context_When_Action_Then_Result`

```csharp
[Fact]
public void Given_DivisorIsZero_When_Dividing_Then_ThrowsException()
```

**Good:**
- Describes the full story: starting state, action, and result.
- Matches the Arrange / Act / Assert structure of the test body.
- Good for teams that use BDD (Behavior-Driven Development), where business people also read the tests.

**Bad:**
- The longest of the three.
- Too much for simple tests.

### Selected Style

I will use `Method_Scenario_ExpectedResult` because I want a simple and standard style for unit tests.

### Examples from DriveNow

- `CalculateAge_BirthdayHasNotOccurredThisYear_DoesNotCountCurrentYear`
- `CalculateRentalDays_LeftoverMinutesGreaterThanSixty_AddsExtraDay`
- `Calculate_DriverAgeBetween21And24_AddsFifteenPercentYoungDriverSurcharge`

---

## R3: Test Doubles, Bogus, and AutoFixture Naming

### 1. Hand-written Test Doubles

Prefix with the specific test double pattern:

- `Fake<Concept>` (e.g., `FakeBookingRepository`)
- `Stub<Concept>` (e.g., `StubPricingService`)
- `Spy<Concept>` (e.g., `SpySmsSender`)
- `Dummy<Concept>` (e.g., `DummyPaymentGateway`)
- `Mock<Concept>` (e.g., `MockPaymentGateway`)

The `Mock` prefix is reserved for hand-written mocks. Moq-generated mocks follow Moq's own usage and are not named using this convention.

### 2. Bogus Fakers

- Suffix with `Faker`: `<Entity>Faker`.
- Examples: `CustomerFaker`, `VehicleFaker`.

### 3. AutoFixture

- Customizations suffix with `Customization` (e.g., `ValidCustomerCustomization`).
- Data attributes suffix with `Attribute` (e.g., `AutoMoqDataAttribute`).

---

## R4: Production Code Naming & Namespaces

### 1. Interfaces
- Must start with the letter `I` (e.g., `IBookingRepository`, `IPaymentGateway`).
- Must live in the same namespace as their related implementation, following the existing project structure.

### 2. Repositories
- Class name must end with `Repository` (e.g., `BookingRepository`).
- Must live in a `.Repositories` namespace (e.g., `DriveNow.Infrastructure.Repositories`).

### 3. Services
- Class name must end with `Service` (e.g., `PricingService`, `BookingService`).
- Must live in a `.Services` namespace according to their layer (e.g., `DriveNow.Application.Services`, `DriveNow.Domain.Services`).

### 4. Exceptions
- Class name must end with `Exception` (e.g., `RentalRuleException`).
- Must live in `DriveNow.Domain.Exceptions`.