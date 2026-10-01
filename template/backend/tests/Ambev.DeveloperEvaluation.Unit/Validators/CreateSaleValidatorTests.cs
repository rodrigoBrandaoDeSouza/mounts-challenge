using Ambev.DeveloperEvaluation.Application.Sale.CreateSale;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class CreateSaleValidatorTests
    {
        private static readonly Guid BeerId = Guid.NewGuid();

        private static CreateSaleCommand CreateValidCommand()
        {
            return new CreateSaleCommand
            {
                SaleNumber = "S-100",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Valid Customer",
                BranchId = Guid.NewGuid(),
                BranchName = "Valid Branch",
                TotalAmount = 100m,
                Cancelled = false,
                Items = new List<CreateSaleItemDto>
                {
                    new CreateSaleItemDto
                    {
                        ProductId = BeerId,
                        ProductName = "Beer",
                        Quantity = 2,
                        UnitPrice = 10m
                    }
                }
            };
        }

        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var cmd = CreateValidCommand();
            var validator = new CreateSaleValidator();

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_MissingCustomer_FailsWithRequiredMessage()
        {
            var cmd = CreateValidCommand();
            cmd.CustomerName = string.Empty;

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer is required");
        }

        [Fact]
        public void Validate_MissingBranch_FailsWithRequiredMessage()
        {
            var cmd = CreateValidCommand();
            cmd.BranchName = string.Empty;

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Branch is required");
        }

        [Fact]
        public void Validate_EmptyItems_FailsWithAtLeastOneItemMessage()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<CreateSaleItemDto>();

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one sale item is required");
        }

        [Fact]
        public void Validate_ItemMissingProductName_FailsWithProductNameRequired()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto
                {
                    ProductName = string.Empty,
                    Quantity = 1,
                    UnitPrice = 1m
                }
            };

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required");
        }

        [Fact]
        public void Validate_ItemQuantityOutOfRange_FailsWithQuantityMessages()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 0, UnitPrice = 1m },
                new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "Y", Quantity = 21, UnitPrice = 1m }
            };

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity must be greater than zero");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity cannot exceed 20");
        }

        [Fact]
        public void Validate_ItemUnitPriceLessOrEqualZero_FailsWithUnitPriceMessage()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 1, UnitPrice = 0m }
            };

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Unit price must be greater than zero");
        }

        [Fact]
        public void Validate_ProductGroupingExceedsMaximum_FailsWithGroupedMessage()
        {
            var cmd = CreateValidCommand();
            // Two items with the same ProductId whose quantities sum > 20
            cmd.Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto { ProductId = BeerId, ProductName = " Beer ", Quantity = 12, UnitPrice = 1m },
                new CreateSaleItemDto { ProductId = BeerId, ProductName = "beer", Quantity = 10, UnitPrice = 1m }
            };

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            // Items are grouped by ProductId; the message uses the (trimmed) name of the first item
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("The product 'Beer' exceeds the maximum allowed quantity of 20 units."));
        }

        [Fact]
        public void Validate_ProductGroupingEqualsMaximum_IsValid()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto { ProductId = BeerId, ProductName = " Beer ", Quantity = 10, UnitPrice = 1m },
                new CreateSaleItemDto { ProductId = BeerId, ProductName = "beer", Quantity = 10, UnitPrice = 1m }
            };

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_ProductNameTooLong_ReportsProductNameError()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<CreateSaleItemDto>
            {
                new CreateSaleItemDto
                {
                    ProductName = new string('A', 101),
                    Quantity = 1,
                    UnitPrice = 1m
                }
            };

            var validator = new CreateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            // Maximum length uses default message; assert there is an error for ProductName property to avoid brittle message checks.
            Assert.Contains(result.Errors, e => e.PropertyName.EndsWith("ProductName", StringComparison.OrdinalIgnoreCase));
        }
    }
}