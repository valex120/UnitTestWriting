using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using UnitTestWriting.Domain;

namespace UnitTestWriting.Tests.Domain;

public class PromoCodeTests
{
    [TestCase(1, false)]
    [TestCase(49, false)]
    [TestCase(50, true)]
    [TestCase(99, true)]
    public void Ctor_ValidDiscount_ReturnsPremiumOnly(int discount,
                                                      bool expectedPremiumOnly)
    {
        // ACT
        var promoCode = new PromoCode(discount, "CODE", TimeSpan.FromDays(10));

        // ASSERT
        promoCode.PremiumOnly.Should().Be(expectedPremiumOnly);
    }

    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(100)]
    public void Ctor_InvalidDiscount_ThrowsArgumentOutOfRangeException(int discount)
    {
        // ACT
        Action act = () => new PromoCode(discount, "CODE", TimeSpan.FromDays(10));

        // ASSERT
        var expectedMessage = new ArgumentOutOfRangeException("discount").Message;
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage(expectedMessage);
    }

    [TestCase(0)]
    [TestCase(59)]
    public void Ctor_InvalidTimeToLive_ThrowsArgumentOutOfRangeException(int timeToLiveSeconds)
    {
        // ACT
        Action act = () => new PromoCode(10, "CODE", TimeSpan.FromSeconds(timeToLiveSeconds));

        // ASSERT
        var expectedMessage = new ArgumentOutOfRangeException("timeToLive").Message;
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage(expectedMessage);
    }

    [Test]
    public void Ctor_ValidParameters_ReturnsNewPromoCode()
    {
        // ARRANGE
        var discount = 10;
        var timeToLive = TimeSpan.FromHours(10);
        var code = "code";

        // ACT
        var actualPromoCode = new PromoCode(discount, code, timeToLive);

        // ASSERT
        actualPromoCode.Should().BeEquivalentTo(
            new
            {
                Discount = discount,
                Code = code.ToUpper(),
                PremiumOnly = false
            });

        actualPromoCode.Id.Should().NotBe(new Guid());

        actualPromoCode.ExpiredAt.Should().BeCloseTo(DateTime.UtcNow.Add(timeToLive), 100.Milliseconds());
    }
}