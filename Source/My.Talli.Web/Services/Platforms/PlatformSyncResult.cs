namespace My.Talli.Web.Services.Platforms;

/// <summary>Result</summary>
public class PlatformSyncResult
{
    #region <Properties>

    public int NewRevenueRowCount { get; set; }

    public DateTime? MostRecentTransactionDate { get; set; }

    public int PagesFetched { get; set; }

    public int ReceiptsProcessed { get; set; }

    #endregion
}
