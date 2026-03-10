namespace My.Talli.Domain.Entities;

using My.Talli.Domain.Entities.Interfaces;

/// <summary>Entity</summary>
public class AuditableIdentifiableEntity : IAuditableIdentifiable
{
    #region <Properties>

    public long CreateByUserId { get; set; }
    
    public DateTime CreatedOnDateTime { get; set; }
    
    public long? UpdatedByUserId { get; set; }
    
    public DateTime? UpdatedOnDate { get; set; }
    
    public long Id { get; set; }

    #endregion
}