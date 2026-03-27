namespace My.Talli.Web.ViewModels.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Models;

/// <summary>View Model</summary>
public class PlatformsViewModel : ComponentBase
{
	#region <Variables>

	[CascadingParameter]
	private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;


	#endregion

	#region <Properties>

	public int ConnectedCount => Platforms.Count(p => p.IsConnected);

	public int AvailableCount => Platforms.Count(p => !p.IsConnected);

	public List<PlatformItem> ConnectedPlatforms => Platforms.Where(p => p.IsConnected).ToList();

	public List<PlatformItem> AvailablePlatforms => Platforms.Where(p => !p.IsConnected).ToList();

	public bool IsLoading { get; private set; } = true;

	public int TotalTransactions => Platforms.Where(p => p.IsConnected).Sum(p => p.TransactionCount);


	#endregion

	#region <Events>

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthenticationStateTask;
		var principal = authState.User;

		if (principal.Identity?.IsAuthenticated != true)
		{
			IsLoading = false;
			return;
		}

		// Sample data — will be replaced with real platform connection queries
		Platforms = GetSamplePlatforms();
		IsLoading = false;
	}


	#endregion

	#region <Methods>

	private static List<PlatformItem> GetSamplePlatforms()
	{
		return
		[
			new PlatformItem
			{
				BrandColor = "#635bff",
				Description = "Charges, refunds, subscriptions & payouts",
				Icon = "stripe",
				IsConnected = true,
				LastSyncLabel = "Synced 12 min ago",
				Name = "Stripe",
				Subtitle = "Payment processing",
				TransactionCount = 142,
			},
			new PlatformItem
			{
				BrandColor = "#f56400",
				Description = "Shop receipts, transactions & payments",
				Icon = "etsy",
				IsConnected = true,
				LastSyncLabel = "Synced 3 hours ago",
				Name = "Etsy",
				Subtitle = "Handmade marketplace",
				TransactionCount = 89,
			},
			new PlatformItem
			{
				BrandColor = "#ff90e8",
				Description = "Sales, products & subscriber revenue",
				Icon = "gumroad",
				IsConnected = false,
				Name = "Gumroad",
				Subtitle = "Digital products",
			},
			new PlatformItem
			{
				BrandColor = "#003087",
				Description = "Transactions, invoices & balance",
				Icon = "paypal",
				IsConnected = false,
				Name = "PayPal",
				Subtitle = "Payment service",
			},
			new PlatformItem
			{
				BrandColor = "#96bf48",
				Description = "Orders, products & revenue",
				Icon = "shopify",
				IsConnected = false,
				Name = "Shopify",
				Subtitle = "E-commerce platform",
			},
		];
	}

	private List<PlatformItem> Platforms { get; set; } = [];

	#endregion
}