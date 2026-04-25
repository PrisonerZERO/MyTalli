namespace My.Talli.Web.Services.Platforms;

/// <summary>Result</summary>
public class PlatformSyncResult
{
    #region <Properties>

    public int LedgerEntriesProcessed { get; set; }

    public int LedgerPagesFetched { get; set; }

    public DateTime? MostRecentTransactionDate { get; set; }

    public int NewExpenseRowCount { get; set; }

    public int NewPayoutRowCount { get; set; }

    public int NewRevenueRowCount { get; set; }

    public int PagesFetched { get; set; }

    public int ReceiptsProcessed { get; set; }

    #endregion
}
