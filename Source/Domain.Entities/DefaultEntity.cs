namespace My.Talli.Domain.Entities;

/// <summary>Entity</summary>
public class DefaultEntity : AuditableIdentifiableEntity
{
    #region <Properties>

    public bool IsActive { get; set; } = true;

	#endregion
}