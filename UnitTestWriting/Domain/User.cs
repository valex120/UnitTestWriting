namespace UnitTestWriting.Domain;

/// <summary>
///     Пользователь
/// </summary>
public sealed class User
{
    /// <summary>
    ///     Идентификатор пользователя
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Имя пользователя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     True, если у пользователя премиум аккаунт
    /// </summary>
    public bool Premium { get; set; }

    /// <summary>
    ///     Дата рождения
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    ///     Дата заведения
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///     Дата дата изменения
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}