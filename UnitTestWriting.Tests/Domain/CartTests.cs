using FluentAssertions;
using FluentAssertions.Extensions;
using UnitTestWriting.Domain;
using Xunit;

namespace UnitTestWriting.Tests.Domain
{
    public class CartTests
    {
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
        public void GetFullDiscount_ByDiscountPromocodeAndClientBirthDate_ReturnsFullDiscount(
            int? discount,
            int? promocodeDiscount,
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
            if (promocodeDiscount.HasValue) cart.ApplyPromo(new PromoCode(promocodeDiscount.Value, "TEST", 10.Days()));

            var purchasedAt = new DateTime(2024, purchasedAtMonth, purchasedAtDay);

            // Act
            var actualFullDiscount = cart.GetFullDiscount(purchasedAt);

            // Assert
            actualFullDiscount.Should().Be(expectedFullDiscount);
        }

        [Fact]
        public void GetFullPrice_EmptyCart_ReturnsZero()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new Cart(new User {  BirthDate = now });
            cart.ApplyDiscount(10);
            cart.ApplyPromo(new PromoCode(10, "test", 10.Hours()));

            // Act
            var actualFullPrice = cart.GetFullPrice(now);

            // Assert
            actualFullPrice.Should().Be(0);
        }

        [Fact]
        public void GetFullPrice_BySeveralProductsNoDiscount_ReturnsSumByProducts()
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
        public void GetFullPrice_ByAllDiscountsTotal50_ReturnsHalfOfOriginPrice()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var cart = new Cart(new User { BirthDate = now });
            cart.AddProduct(new Product() { Price = 10 }, 10);
            cart.ApplyDiscount(30);
            cart.ApplyPromo(new PromoCode(15, "test", 10.Hours()));

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
        public void GetFullPrice_FractionalDiscount_ReturnsFlooredDiscountedPrice(
            int discount,
            int expectedFullPrice)
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

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void AddProduct_InvalidAmount_ThrowsArgumentOutOfRangeException(int amount)
        {
            // Arrange
            var cart = new Cart(new User());

            // Act
            var exception = cart.Invoking(c => c.AddProduct(new Product(), amount))
                                .Should()
                                .Throw<ArgumentOutOfRangeException>()
                                .Which;

            // Assert
            exception.Message.Should().Contain("'amount'");
        }

        [Fact]
        public void AddProduct_NewProduct_AddedNewProductWithAmount()
        {
            // Arrange
            var cart = new Cart(new User());
            var product = new Product { Id = Guid.NewGuid() };

            // Act
            cart.AddProduct(product, 10);

            // Assert
            cart.Products.Should().HaveCount(1);
            cart.Products.Single().Product.Should().Be(product);
            cart.Products.Single().Amount.Should().Be(10);
        }

        [Fact]
        public void AddProduct_AlreadyAddedProduct_IncreaseProductsAmount()
        {
            // Arrange
            var cart = new Cart(new User());
            var product = new Product { Id = Guid.NewGuid() };
            var sameProduct = new Product { Id = product.Id };
            cart.AddProduct(product, 10);

            // Act
            cart.AddProduct(sameProduct, 10);

            // Assert
            cart.Products.Should().HaveCount(1);
            cart.Products.Single().Product.Should().Be(product);
            cart.Products.Single().Amount.Should().Be(20);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(101)]
        public void ApplyDiscount_InvalidDiscount_ThrowsArgumentOutOfRangeException(int discount)
        {
            // Arrange
            var cart = new Cart(new User());

            // Act
            var exception = cart.Invoking(c => c.ApplyDiscount(discount))
                                .Should()
                                .Throw<ArgumentOutOfRangeException>()
                                .Which;

            // Assert
            exception.Message.Should().Contain("'discount'");
        }

        [Fact]
        public void ApplyDiscount_DiscountAlreadyApplied_ThrowsException()
        {
            // Arrange
            var cart = new Cart(new User());
            cart.ApplyDiscount(1);

            // Act
            var exception = cart.Invoking(c => c.ApplyDiscount(1))
                                .Should()
                                .Throw<Exception>()
                                .Which;

            // Assert
            exception.Message.Should().Be("Скидка уже применена");
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(99, null)]
        [InlineData(50, 49)]
        public void ApplyDiscount_ValidDicountAndPromo_SetDiscount(int discount, int? promo)
        {
            // Arrange
            var cart = new Cart(new User());
            if(promo.HasValue) cart.ApplyPromo(new PromoCode(promo.Value, "test", 10.Hours()));

            // Act
            cart.ApplyDiscount(discount);

            // Assert
            cart.Discount.Should().Be(discount);
        }

        [Theory]
        [InlineData(60, 40)]
        [InlineData(60, 41)]
        public void ApplyDiscount_InvalidDiscountSum_ThrowsException(int discount, int promo)
        {
            // Arrange
            var cart = new Cart(new User());
            cart.ApplyPromo(new PromoCode(promo, "test", 10.Hours()));

            // Act
            var exception = cart.Invoking(c => c.ApplyDiscount(discount))
                                .Should()
                                .Throw<ArgumentException>()
                                .Which;
            // Assert
            exception.Message.Should().Be("Общая скидка не может быть больше 100%");
        }

        [Fact]
        public void ApplyPromo_PromoAlreadyApplied_ThrowsException()
        {
            // Arrange
            var cart = new Cart(new User());
            var promo = new PromoCode(10, "TEST", 10.Hours());
            cart.ApplyPromo(promo);

            // Act
            var exception = cart.Invoking(c => c.ApplyPromo(promo))
                                .Should()
                                .Throw<Exception>()
                                .Which;

            // Assert
            exception.Message.Should().Be("Промокод уже применён");
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(99, null)]
        [InlineData(50, 49)]
        public void ApplyPromo_ValidPromoAndDicount_SetPromo(int promo, int? discount)
        {
            // Arrange
            var cart = new Cart(new User { Premium = true });
            var promoCode = new PromoCode(promo, "TEST", 10.Hours());
            if (discount.HasValue) cart.ApplyDiscount(discount.Value);

            // Act
            cart.ApplyPromo(promoCode);

            // Assert
            cart.PromoCode.Should().Be(promoCode);
        }

        [Theory]
        [InlineData(60, 40)]
        [InlineData(60, 41)]
        public void ApplyPromo_InvalidDiscountSum_ThrowsException(int discount, int promo)
        {
            // Arrange
            var cart = new Cart(new User());
            cart.ApplyDiscount(discount);

            // Act
            var exception = cart.Invoking(c => c.ApplyPromo(new PromoCode(promo, "test", 10.Hours())))
                                .Should()
                                .Throw<ArgumentException>()
                                .Which;
            // Assert
            exception.Message.Should().Be("Общая скидка не может быть больше 100%");
        }

        [Fact]
        public void ApplyPromo_PremiumPromoForNonePremiunCustomer_ThrowsException()
        {
            // Arrange
            var cart = new Cart(new User());

            // Act
            var exception = cart.Invoking(c => c.ApplyPromo(new PromoCode(60, "test", 10.Hours())))
                                .Should()
                                .Throw<Exception>()
                                .Which;
            // Assert
            exception.Message.Should().Be("Промокод только для пользователей премиальных аккаунтов");
        }
    }
}
