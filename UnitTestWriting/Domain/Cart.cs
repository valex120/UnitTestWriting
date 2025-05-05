namespace UnitTestWriting.Domain;

/// <summary>
///     Корзина покупок
/// </summary>
public sealed class Cart
{
    /// <summary>
    ///     Товары в корзине
    /// </summary>
    private List<(Product Product, int Amount)> _products;

    /// <summary>
    ///     Товары в корзине
    /// </summary>
    public (Product Product, int Amount)[] Products => _products.ToArray();

    /// <summary>
    ///     Пользователь покупатель
    /// </summary>
    public User Customer { get; }

    /// <summary>
    ///     Общая скидка на покупку в процентах
    /// </summary>
    public int? Discount { get; private set; }

    /// <summary>
    ///     Промокод со скидкой
    /// </summary>
    public PromoCode? PromoCode { get; private set; }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="customer">Покупатель</param>
    public Cart(User customer)
    {
        Customer = customer;
        _products = new List<(Product Product, int Amount)>();
    }

    /// <summary>
    ///     Возвращает полную скидку по корзине
    /// </summary>
    /// <param name="purchasedAt">Дата покупки</param>
    public int GetFullDiscount(DateTime purchasedAt)
    {
        var discount = (Discount ?? 0) + (PromoCode?.Discount ?? 0);

        var birthDate = Customer.BirthDate;
        var birthDateDiscount = purchasedAt.Day == birthDate.Day && purchasedAt.Month == birthDate.Month ? 5 : 0;

        if ((discount + birthDateDiscount) >= 100)
            return discount;

        return discount + birthDateDiscount;
    }

    /// <summary>
    ///     Возвращает полную цену корзины
    /// </summary>
    /// <param name="purchasedAt">Дата покупки</param>
    public int GetFullPrice(DateTime purchasedAt)
    {
        var price = _products.Sum(p => p.Product.Price * p.Amount);

        var discount = GetFullDiscount(purchasedAt);

        if (discount == 0)
            return price;

        return price - (int)Math.Floor(price * discount / 100.0);
    }

    /// <summary>
    ///     Добавляет товар в корзину
    /// </summary>
    /// <param name="product">Товар</param>
    /// <param name="amount">Количество штук</param>
    public void AddProduct(Product product, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (_products.Any(p => p.Product.Id == product.Id))
        {
            var alreadyAdded = _products.Single(p => p.Product.Id == product.Id);
            _products.Remove(alreadyAdded);
            alreadyAdded.Amount += amount;
            _products.Add(alreadyAdded);
        }
        else
        {
            _products.Add((product, amount));
        }
    }

    /// <summary>
    ///     Применяет общую скидку
    /// </summary>
    /// <param name="discount">Размер скидки в процентах</param>
    public void ApplyDiscount(int discount)
    {
        if (Discount.HasValue)
            throw new Exception("Скидка уже применена");

        if (discount is >= 100 or <= 0)
            throw new ArgumentOutOfRangeException(nameof(discount));

        var fullDiscount = discount + PromoCode?.Discount ?? 0;

        if (fullDiscount >= 100)
            throw new ArgumentException("Общая скидка не может быть больше 100%");

        Discount = discount;
    }

    /// <summary>
    ///     Применяет промокод
    /// </summary>
    /// <param name="promoCode">Промокод</param>
    public void ApplyPromo(PromoCode promoCode)
    {
        if (PromoCode is not null)
            throw new Exception("Промокод уже применён");

        var fullDiscount = (Discount ?? 0) + promoCode.Discount;

        if (fullDiscount >= 100)
            throw new ArgumentException("Общая скидка не может быть больше 100%");

        if(promoCode.PremiumOnly && Customer.Premium is false)
            throw new Exception("Промокод только для пользователей премиальных аккаунтов");

        PromoCode = promoCode;
    }
}