namespace My.Talli.Domain.Entities.Interfaces;

using System;
using System.Collections.Generic;
using System.Text;

public interface IIdentifiable
{
	#region <Properties>

	long Id { get; set; }

	#endregion
}