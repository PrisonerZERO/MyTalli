using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace My.Talli.Domain.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "commerce");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "ProductType",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductVendor",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVendor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InitialProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreferredProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserPreferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductTypeId = table.Column<long>(type: "bigint", nullable: false),
                    VendorPrice = table.Column<decimal>(type: "money", nullable: false),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_ProductType",
                        column: x => x.ProductTypeId,
                        principalSchema: "commerce",
                        principalTable: "ProductType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_ProductVendor",
                        column: x => x.VendorId,
                        principalSchema: "commerce",
                        principalTable: "ProductVendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaxCharged = table.Column<decimal>(type: "money", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_User",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAuthenticationApple",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppleId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrivateRelay = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthApple", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthApple_User",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAuthenticationGoogle",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GoogleId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthGoogle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthGoogle_User",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAuthenticationMicrosoft",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MicrosoftId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthMicrosoft", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAuthMicrosoft_User",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Billing",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billing_Order",
                        column: x => x.OrderId,
                        principalSchema: "commerce",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Billing_User",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProductPriceCharged = table.Column<decimal>(type: "money", nullable: false),
                    ProductQuantity = table.Column<int>(type: "int", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_Order",
                        column: x => x.OrderId,
                        principalSchema: "commerce",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItem_Product",
                        column: x => x.ProductId,
                        principalSchema: "commerce",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingStripe",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillingId = table.Column<long>(type: "bigint", nullable: false),
                    CardBrand = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CardLastFour = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingStripe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingStripe_Billing",
                        column: x => x.BillingId,
                        principalSchema: "commerce",
                        principalTable: "Billing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscription_OrderItem",
                        column: x => x.OrderItemId,
                        principalSchema: "commerce",
                        principalTable: "OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscription_Product",
                        column: x => x.ProductId,
                        principalSchema: "commerce",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscription_User",
                        column: x => x.UserId,
                        principalSchema: "auth",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStripe",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StripePriceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SubscriptionId = table.Column<long>(type: "bigint", nullable: false),
                    CreateByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOnDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedOnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStripe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionStripe_Subscription",
                        column: x => x.SubscriptionId,
                        principalSchema: "commerce",
                        principalTable: "Subscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billing_OrderId",
                schema: "commerce",
                table: "Billing",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Billing_UserId",
                schema: "commerce",
                table: "Billing",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_BillingStripe_BillingId",
                schema: "commerce",
                table: "BillingStripe",
                column: "BillingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserId",
                schema: "commerce",
                table: "Order",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                schema: "commerce",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductId",
                schema: "commerce",
                table: "OrderItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductTypeId",
                schema: "commerce",
                table: "Product",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_VendorId",
                schema: "commerce",
                table: "Product",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_OrderItemId",
                schema: "commerce",
                table: "Subscription",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_ProductId",
                schema: "commerce",
                table: "Subscription",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_UserId",
                schema: "commerce",
                table: "Subscription",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_SubscriptionStripe_SubscriptionId",
                schema: "commerce",
                table: "SubscriptionStripe",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserAuthApple_AppleId",
                schema: "auth",
                table: "UserAuthenticationApple",
                column: "AppleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserAuthApple_UserId",
                schema: "auth",
                table: "UserAuthenticationApple",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserAuthGoogle_GoogleId",
                schema: "auth",
                table: "UserAuthenticationGoogle",
                column: "GoogleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserAuthGoogle_UserId",
                schema: "auth",
                table: "UserAuthenticationGoogle",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserAuthMicrosoft_MicrosoftId",
                schema: "auth",
                table: "UserAuthenticationMicrosoft",
                column: "MicrosoftId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserAuthMicrosoft_UserId",
                schema: "auth",
                table: "UserAuthenticationMicrosoft",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingStripe",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "SubscriptionStripe",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "UserAuthenticationApple",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserAuthenticationGoogle",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserAuthenticationMicrosoft",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Billing",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "Subscription",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "OrderItem",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "Order",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "User",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ProductType",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "ProductVendor",
                schema: "commerce");
        }
    }
}
