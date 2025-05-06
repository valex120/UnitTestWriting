using FluentAssertions;
using FluentAssertions.Extensions;
using System;
using System.Linq;
using UnitTestWriting.Domain;
using Xunit;

namespace UnitTestWriting.Tests.Domain
{
    public class CartTests
    {
        // Метод GetFullDiscount
        [Theory]
        [InlineData(null, null, 1, 1, 2, 2, 0)]
        [InlineData(null, null, 1, 1, 1, 1, 5)]
        [InlineData(10, null, 1, 1, 2, 2, 10)]
        [InlineData(null, 10, 1, 1, 2, 2, 10)]
        [InlineData(10, 10, 1, 1, 2, 2, 20)]
        [InlineData(10, 10, 1, 1, 1, 1, 25)]
        [InlineData(50, 44, 1, 1, 1, 1, 99)]
        [InlineData(50, 45, 1, 1, 1, 1, 95)]
        [InlineData(50, 46, 1, 1, 1, 1, 96)]
        public void GetFullDiscount_ByDiscountPromocodeAndBirthDate_ReturnsExpectedFullDiscount(
            int? discount,
            int? promoDiscount,
            int purchasedAtDay,
            int purchasedAtMonth,
            int clientBirthDay,
            int clientBirthMonth,
            int expectedFullDiscount)
        {
            // Arrange
            var user = new User
            {
                BirthDate = new DateTime(2000, clientBirthMonth, clientBirthDay)
            };
            var cart = new Cart(user);
            if (discount.HasValue) cart.ApplyDiscount(discount.Value);
            if (promoDiscount.HasValue)
                cart.ApplyPromo(new PromoCode(promoDiscount.Value, "TEST", TimeSpan.FromDays(10)));
            var purchasedAt = new DateTime(2024, purchasedAtMonth, purchasedAtDay);

            // Act
            var actualFullDiscount = cart.GetFullDiscount(purchasedAt);

            // Assert
            actualFullDiscount.Should().Be(expectedFullDiscount);
        }

        // Метод GetFullPrice
        [Fact]
        public void GetFullPrice_EmptyCart_ReturnsZero()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new Cart(new User { BirthDate = now });
            cart.ApplyDiscount(10);
            cart.ApplyPromo(new PromoCode(10, "test", TimeSpan.FromHours(10)));

            // Act
            var actualFullPrice = cart.GetFullPrice(now);

            // Assert
            actualFullPrice.Should().Be(0);
        }

        [Fact]
        public void GetFullPrice_BySeveralProductsWithoutDiscount_ReturnsSumOfProductPrices()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new Cart(new User());
            cart.AddProduct(new Product() { Id = Guid.NewGuid(), Price = 5 }, 5);
            cart.AddProduct(new Product() { Id = Guid.NewGuid(), Price = 6 }, 6);

            // Act
            var actualFullPrice = cart.GetFullPrice(now);

            // Assert
            actualFullPrice.Should().Be(61);
        }

        [Fact]
        public void GetFullPrice_WithAllDiscountsTotal50Percent_ReturnsHalfOfOriginalPrice()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new Cart(new User { BirthDate = now });
            cart.AddProduct(new Product() { Price = 10 }, 10);
            cart.ApplyDiscount(30);
            cart.ApplyPromo(new PromoCode(15, "test", TimeSpan.FromHours(10)));

            // Act
            var actualFullPrice = cart.GetFullPrice(now);

            // Assert
            actualFullPrice.Should().Be(50);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(9, 10)]
        [InlineData(10, 9)]
        [InlineData(11, 9)]
        [InlineData(99, 1)]
        public void GetFullPrice_FractionalDiscount_PricesAreFlooredCorrectly(int discount, int expectedFullPrice)
        {
            // Arrange
            var cart = new Cart(new User());
            cart.AddProduct(new Product() { Price = 10 }, 1);
            cart.ApplyDiscount(discount);

            // Act
            var actualFullPrice = cart.GetFullPrice(DateTime.Now);

            // Assert
            actualFullPrice.Should().Be(expectedFullPrice);
        }

        // Метод AddProduct
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddProduct_InvalidAmount_ThrowsArgumentOutOfRangeException(int amount)
        {
            // Arrange
            var cart = new Cart(new User());

            // Act
            Action act = () => cart.AddProduct(new Product(), amount);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*amount*");
        }

        [Fact]
        public void AddProduct_ValidLowerBoundaryAmount_AddsProduct()
        {
            // Arrange
            var cart = new Cart(new User());
            var product = new Product { Id = Guid.NewGuid() };

            // Act
            cart.AddProduct(product, 1); // минимальное корректное значение

            // Assert
            cart.Products.Should().ContainSingle()
                .Which.Product.Should().Be(product);
            cart.Products.Single().Amount.Should().Be(1);
        }

        [Fact]
        public void AddProduct_NewProduct_IsAddedWithCorrectAmount()
        {
            // Arrange
            var cart = new Cart(new User());
            var product = new Product { Id = Guid.NewGuid() };

            // Act
            cart.AddProduct(product, 10);

            // Assert
            cart.Products.Should().ContainSingle()
                .Which.Product.Should().Be(product);
            cart.Products.Single().Amount.Should().Be(10);
        }

        [Fact]
        public void AddProduct_AlreadyAddedProduct_IncreasesProductAmount()
        {
            // Arrange
            var cart = new Cart(new User());
            var product = new Product { Id = Guid.NewGuid() };
            var sameProduct = new Product { Id = product.Id };
            cart.AddProduct(product, 10);

            // Act
            cart.AddProduct(sameProduct, 10);

            // Assert
            cart.Products.Should().ContainSingle();
            cart.Products.Single().Product.Should().Be(product);
            cart.Products.Single().Amount.Should().Be(20);
        }

        // Метод ApplyDiscount
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(101)]
        public void ApplyDiscount_InvalidDiscountValue_ThrowsArgumentOutOfRangeException(int discount)
        {
            // Arrange
            var cart = new Cart(new User());

            // Act
            Action act = () => cart.ApplyDiscount(discount);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*discount*");
        }

        [Fact]
        public void ApplyDiscount_WhenDiscountAlreadyApplied_ThrowsException()
        {
            // Arrange
            var cart = new Cart(new User());
            cart.ApplyDiscount(1);

            // Act
            Action act = () => cart.ApplyDiscount(1);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Скидка уже применена");
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(99, null)]
        [InlineData(50, 49)]
        public void ApplyDiscount_ValidDiscountAndOptionalPromo_SetsDiscount(int discount, int? promoDiscount)
        {
            // Arrange
            var cart = new Cart(new User());
            if (promoDiscount.HasValue)
            {
                cart.ApplyPromo(new PromoCode(promoDiscount.Value, "test", TimeSpan.FromHours(10)));
            }

            // Act
            cart.ApplyDiscount(discount);

            // Assert
            cart.Discount.Should().Be(discount);
        }

        [Theory]
        [InlineData(60, 40)]
        [InlineData(60, 41)]
        public void ApplyDiscount_WhenTotalDiscountExceedsLimit_ThrowsArgumentException(int discount, int promoDiscount)
        {
            // Arrange
            var cart = new Cart(new User());
            cart.ApplyPromo(new PromoCode(promoDiscount, "test", TimeSpan.FromHours(10)));

            // Act
            Action act = () => cart.ApplyDiscount(discount);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Общая скидка не может быть больше 100%");
        }

        // Метод ApplyPromo
        [Fact]
        public void ApplyPromo_WhenPromoAlreadyApplied_ThrowsException()
        {
            // Arrange
            var cart = new Cart(new User());
            var promo = new PromoCode(10, "TEST", TimeSpan.FromHours(10));
            cart.ApplyPromo(promo);

            // Act
            Action act = () => cart.ApplyPromo(promo);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Промокод уже применён");
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(99, null)]
        [InlineData(50, 49)]
        public void ApplyPromo_ValidPromoAndOptionalDiscount_SetsPromo(int promoDiscount, int? discount)
        {
            // Arrange
            var cart = new Cart(new User { Premium = true });
            var promoCode = new PromoCode(promoDiscount, "TEST", TimeSpan.FromHours(10));
            if (discount.HasValue)
                cart.ApplyDiscount(discount.Value);

            // Act
            cart.ApplyPromo(promoCode);

            // Assert
            cart.PromoCode.Should().Be(promoCode);
        }

        [Theory]
        [InlineData(60, 40)]
        [InlineData(60, 41)]
        public void ApplyPromo_WhenTotalDiscountExceedsLimit_ThrowsArgumentException(int discount, int promoDiscount)
        {
            // Arrange
            var cart = new Cart(new User());
            cart.ApplyDiscount(discount);

            // Act
            Action act = () => cart.ApplyPromo(new PromoCode(promoDiscount, "test", TimeSpan.FromHours(10)));

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Общая скидка не может быть больше 100%");
        }

        [Fact]
        public void ApplyPromo_WhenPremiumPromoForNonPremiumUser_ThrowsException()
        {
            // Arrange
            var cart = new Cart(new User());

            // Act
            Action act = () => cart.ApplyPromo(new PromoCode(60, "test", TimeSpan.FromHours(10)));

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("Промокод только для пользователей премиальных аккаунтов");
        }
    }
}
