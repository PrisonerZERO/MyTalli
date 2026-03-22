# Change Plan Feature

## Current State
- "Change Plan" on `/subscription` goes to Stripe Portal (same as "Manage Billing")
- Upgrade page detects Pro subscriber and current period but only shows "Manage Plan" button
- No way to switch between monthly ↔ yearly from within the app

## Plan

### 1. Update SubscriptionViewModel — Route "Change Plan" to Upgrade page
- Change `HandleChangePlan()` to navigate to `/upgrade` instead of the Stripe portal
- The Upgrade page already has the subscription-aware logic

### 2. Update Upgrade page UI — Smart Pro state
When user is Pro subscriber:
- **Pro card**: Show "CURRENT PLAN" badge (not "RECOMMENDED")
- **Toggle**: Highlight their current period, allow selecting the other
- **CTA button logic**:
  - Same period as current → "Current Plan ✓" (disabled)
  - Different period selected → "Switch to Monthly" or "Switch to Yearly"
- **Free card**: No badge (they're not on Free)

### 3. Add plan switch endpoint
- New endpoint: `GET /api/billing/switch-plan?plan={monthly|yearly}`
- Looks up the user's active Stripe subscription
- Calls Stripe API to update the subscription item's price (`SubscriptionService.UpdateAsync`)
- Stripe prorates automatically
- Redirects back to `/subscription` on success

### 4. Update UpgradeViewModel — Add HandleSwitchPlan method
- New method that navigates to the switch endpoint with `forceLoad: true`
- The CTA button calls this instead of `HandleUpgrade` when switching

### Files Changed
- `ViewModels/Pages/SubscriptionViewModel.cs` — change navigation target
- `ViewModels/Pages/UpgradeViewModel.cs` — add `HandleSwitchPlan()`, refine button state
- `Components/Pages/Upgrade.razor` — conditional CTA text/state
- `Endpoints/BillingEndpoints.cs` — add switch-plan endpoint
- `Services/Billing/StripeBillingService.cs` — add `SwitchPlanAsync()` method
