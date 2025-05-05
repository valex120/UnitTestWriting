namespace UnitTestWriting.Domain;

/// <summary>
///     Промокод на покупки
/// </summary>
public sealed class PromoCode
{
    /// <summary>
    ///     Идентфикатор промокода
    /// </summary>

    public Guid Id { get; }

    /// <summary>
    ///     Код для ввода
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Дата истечения срока действия
    /// </summary>
    public DateTime ExpiredAt { get; }

    /// <summary>
    ///     Размер скидки в процентах
    /// </summary>

    public int Discount { get; }

    /// <summary>
    ///     True, если доступен только премиум аккаунтам
    /// </summary>

    public bool PremiumOnly { get; }

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="discount">Размер скидки в процентах</param>
    /// <param name="code">Код для ввода</param>
    /// <param name="timeToLive">Дата истечения срока действия</param>
    /// <exception cref="ArgumentOutOfRangeException">discount</exception>
    /// <exception cref="ArgumentOutOfRangeException">timeToLive</exception>
    public PromoCode(int discount, string code, TimeSpan timeToLive)
    {
        if (discount is >= 100 or <= 0)
            throw new ArgumentOutOfRangeException(nameof(discount));

        if (timeToLive < TimeSpan.FromMinutes(1))
            throw new ArgumentOutOfRangeException(nameof(timeToLive));

        Id = Guid.NewGuid();
        Code = code.ToUpper();
        Discount = discount;
        PremiumOnly = discount >= 50;
        ExpiredAt = DateTime.UtcNow.Add(timeToLive);
    }
}