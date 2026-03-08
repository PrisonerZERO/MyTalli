# MyTalli — Milestone Roadmap (DRAFT)

> **Purpose:** Planning document for waitlist timeline milestones. Once finalized, these will be reflected on the `/waitlist` page and in `WaitlistViewModel.cs`.
>
> **Current mode:** Waitlist Mode — only landing, sign-in, waitlist, and error pages are live.

---

## Beta

Completed items are listed first. Remaining items are not in priority order.

| Milestone | Status | Notes |
|-----------|--------|-------|
| **Authentication** | Complete | OAuth sign-in with Google, Apple, Microsoft. Cookie auth with 30-day sliding expiration. |
| **Landing Page & Waitlist** | Complete | Landing page deployed to Azure SWA. Waitlist page with milestone timeline. Waitlist Mode gating active. |
| **Stripe Integration** | | Connector & Dashboard — connect Stripe account, pull revenue/transaction data, specialized detail view. |
| **Etsy Integration** | | Connector & Dashboard — connect Etsy shop, pull sales/order data, specialized detail view. |
| **Gumroad Integration** | | Connector & Dashboard — connect Gumroad account, pull sales/product data, specialized detail view. |
| **Aggregate Dashboard** | | Unified revenue view across all connected platforms — totals, trends, platform breakdown. |
| **Platforms Page** | | Manage connected platforms — connect/disconnect, connection health, sync status. (Nav: `/platforms`) |
| **Suggestion Box** | | Submit and vote on feature ideas. (Nav: `/suggestions`) |
| **Settings** | | User profile, preferences, notification settings. (Nav: `/settings`) |
| **Subscription & Billing** | | Stripe checkout tied to user records, Free vs Pro tier enforcement, billing portal. |
| **Beta Launch** | | Open dashboard to waitlist members — first users get access. |

---

## Full Launch

| Milestone | Status | Notes |
|-----------|--------|-------|
| **PayPal Integration** | | Connector & Dashboard — connect PayPal account, pull transaction data, specialized detail view. |
| **Shopify Integration** | | Connector & Dashboard — connect Shopify store, pull order/revenue data, specialized detail view. |
| **Goals** | | Set monthly revenue targets, track progress with visual indicators. (Nav: `/goals`) |
| **CSV Export** | | Download revenue data in clean CSV format for tax prep/bookkeeping. (Nav: `/export`) |
| **Weekly Email Summaries** | | Pro tier feature — weekly revenue digest via email. |
| **Admin Page** | | Internal admin for managing waitlist, submissions, users, feature flags. (Not user-facing.) |
| **Full Launch** | | Public launch — open registration, all connectors available, marketing push. |

---

## Open Questions

- **Settings scope:** What goes in Settings for MVP? Just profile/name/email, or notifications too?
- **PayPal & Shopify:** Confirmed post-beta? Or could one of these swap in if a connector proves easier than Etsy/Gumroad?
- **API research needed:** What does each connector's API actually return? This will shape the specialized dashboards. (TODO — research last.)

---

## Conversation Notes

Decisions made during planning:

1. Database persistence is an internal implementation detail — not shown as a user-facing milestone. It's part of general development within each feature.
2. Each connector is listed separately (not grouped as "Platform Connectors").
3. Each connector gets its own specialized dashboard because APIs expose different detail data.
4. Connector + Dashboard are bundled into one user-facing milestone (e.g., "Stripe Integration — Connector & Dashboard").
5. The aggregate dashboard is a separate milestone from the connector integrations.
6. The pre-beta list is NOT ordered by priority — it's the set of features needed before beta.
7. The roadmap is split into two sections: **Beta** (lists first) and **Full Launch** (lists after Beta).
8. Completed items always appear at the top of their section.
9. Goals and CSV Export are Full Launch items, not Beta.
10. Full Launch is the final milestone.
11. Connector API research will be done last to refine the specialized dashboard milestones.
