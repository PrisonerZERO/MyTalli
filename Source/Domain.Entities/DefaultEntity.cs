namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class DefaultEntity : AuditableIdentifiableEntity
{
    #region <Properties>

    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; } = true;

	#endregion
}