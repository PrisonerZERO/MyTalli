# MyTalli Database ERD

## Product Orders

The catalog and ordering foundation. Subscription lifecycle will be layered on top of this in a future phase.

```
┌──────────────────┐
│  ProductVendor   │
├──────────────────┤
│ 🔑 Id            │
│    VendorName    │
└────────┬─────────┘
         │
         │ 1:M
         │
┌──────────────────┐       ┌──────────────────┐
│  ProductType     │       │    Product        │
├──────────────────┤       ├──────────────────-┤
│ 🔑 Id            │ 1:M   │ 🔑 Id             │
│    ProductType-  │◄──────│    VendorId (FK)  │
│    Name          │       │    ProductType-   │
│                  │       │    Id (FK)        │
└──────────────────┘       │    ProductName    │
                           │    VendorPrice    │
                           └────────┬──────────┘
                                    │
                                    │ 1:M
                                    │
┌──────────────────┐       ┌──────────────────┐
│     User         │       │   OrderItem      │
├──────────────────┤       ├──────────────────-┤
│ 🔑 Id            │       │ 🔑 Id             │
│    FullName      │       │    OrderId (FK)   │
└────────┬─────────┘       │    ProductId (FK) │
         │                 │    ProductPrice-  │
         │ 1:M             │    Charged        │
         │                 │    ProductQuantity│
┌────────┴─────────┐       └──────────────────-┘
│     Order        │               ▲
├──────────────────┤               │
│ 🔑 Id            │     1:M       │
│    UserId (FK)   ├───────────────┘
│    OrderDateTime │
│    TaxCharged    │
└──────────────────┘
```

## Tables

### ProductVendor
The company or platform selling products (e.g., MyTalli, Stripe, Etsy, a third-party partner).

| Column     | Type   | Notes       |
|------------|--------|-------------|
| Id         | PK     |             |
| VendorName | string | Vendor name |

### ProductType
A global taxonomy of product categories shared across all vendors (e.g., "Subscription", "Add-on", "Integration Access"). Vendor-agnostic — the relationship between vendors and types is derived through the Product table.

| Column          | Type   | Notes     |
|-----------------|--------|-----------|
| Id              | PK     |           |
| ProductTypeName | string | Type name |

### Product
A specific item in the catalog, belonging to one vendor and one product type.

| Column        | Type    | Notes                        |
|---------------|---------|------------------------------|
| Id            | PK      |                              |
| VendorId      | FK      | → ProductVendor.Id           |
| ProductTypeId | FK      | → ProductType.Id             |
| ProductName   | string  | Display name                 |
| VendorPrice   | decimal | Current price set by vendor  |

### User
A MyTalli user account. **Not finalized** — columns will expand as auth and profile features are built out.

| Column   | Type   | Notes              |
|----------|--------|--------------------|
| Id       | PK     |                    |
| FullName | string | User's full name   |

### Order
A purchase transaction by a user.

| Column        | Type     | Notes            |
|---------------|----------|------------------|
| Id            | PK       |                  |
| UserId        | FK       | → User.Id        |
| OrderDateTime | datetime | When order placed |
| TaxCharged    | decimal  | Tax amount       |

### OrderItem
A line item within an order. Captures price-at-time-of-purchase (not the current VendorPrice).

| Column             | Type    | Notes                             |
|--------------------|---------|-----------------------------------|
| Id                 | PK      |                                   |
| OrderId            | FK      | → Order.Id                        |
| ProductId          | FK      | → Product.Id                      |
| ProductPriceCharged| decimal | Price snapshot at time of purchase |
| ProductQuantity    | int     | Number of units purchased         |

## Relationships

| From         | To            | Cardinality | FK Column       |
|--------------|---------------|-------------|-----------------|
| ProductVendor| Product       | 1:M         | Product.VendorId      |
| ProductType  | Product       | 1:M         | Product.ProductTypeId |
| Product      | OrderItem     | 1:M         | OrderItem.ProductId   |
| User         | Order         | 1:M         | Order.UserId          |
| Order        | OrderItem     | 1:M         | OrderItem.OrderId     |

## Design Decisions

1. **ProductType is vendor-agnostic** — `VendorId` was intentionally removed from `ProductType`. The vendor-to-type relationship is derived through the `Product` table, avoiding duplicate type rows when multiple vendors sell the same category.

2. **Price snapshot on OrderItem** — `ProductPriceCharged` captures the price at time of purchase, independent of the current `VendorPrice` on the Product table. This preserves order history accuracy.

3. **Multi-vendor ready** — The `ProductVendor` table supports MyTalli selling its own subscriptions alongside third-party partner products.

## Future: Subscription Lifecycle

Not yet designed. Will likely include:
- Subscription status (active, cancelled, past_due, trialed)
- Billing period start/end dates
- Renewal date
- Recurrence type (monthly, annual)
- Stripe subscription/customer IDs
