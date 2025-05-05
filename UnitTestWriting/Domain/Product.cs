namespace UnitTestWriting.Domain;

/// <summary>
///     Товар
/// </summary>
public sealed class Product
{
    /// <summary>
    ///     Идентификатор товара
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Название товара
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Артикул товара
    /// </summary>
    public string Article { get; set; }

    /// <summary>
    ///     Цена товара в копейках за одну штуку
    /// </summary>
    public int Price { get; set; }
}