namespace My.Talli.Domain.Entities.Interfaces;

using System;

/// <summary>Entity</summary>
public interface IAuditable
{
	#region <Properties>

	long CreateByUserId { get; set; }

	DateTime CreatedOnDateTime { get; set; }

	long? UpdatedByUserId { get; set; }

	DateTime? UpdatedOnDate { get; set; }

	#endregion
}