using Ambev.DeveloperEvaluation.Application.Sale.UpdateSale;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class UpdateSaleValidatorTests
    {
        private static readonly Guid BeerId = Guid.NewGuid();

        private static UpdateSaleCommand CreateValidCommand()
        {
            return new UpdateSaleCommand
            {
                SaleNumber = "S-200",
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerName = "Valid Customer",
                BranchId = Guid.NewGuid(),
                BranchName = "Valid Branch",
                TotalAmount = 150m,
                Items = new List<UpdateSaleItemDto>
                {
                    new UpdateSaleItemDto
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
            var validator = new UpdateSaleValidator();

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_MissingId_FailsWithRequiredMessage()
        {
            var cmd = CreateValidCommand();
            cmd.Id = Guid.Empty;

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Sale ID is required for update.");
        }

        [Fact]
        public void Validate_MissingCustomer_FailsWithRequiredMessage()
        {
            var cmd = CreateValidCommand();
            cmd.CustomerName = string.Empty;

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer name is required.");
        }

        [Fact]
        public void Validate_CustomerTooLong_Fails()
        {
            var cmd = CreateValidCommand();
            cmd.CustomerName = new string('C', 101);

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName.EndsWith("CustomerName", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Validate_MissingBranch_FailsWithRequiredMessage()
        {
            var cmd = CreateValidCommand();
            cmd.BranchName = string.Empty;

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Branch name is required.");
        }

        [Fact]
        public void Validate_BranchTooLong_Fails()
        {
            var cmd = CreateValidCommand();
            cmd.BranchName = new string('B', 101);

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName.EndsWith("BranchName", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Validate_EmptyItems_FailsWithAtLeastOneItemMessage()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<UpdateSaleItemDto>();

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one sale item is required.");
        }

        [Fact]
        public void Validate_ItemMissingProductName_FailsWithProductNameRequired()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductName = string.Empty,
                    Quantity = 1,
                    UnitPrice = 1m
                }
            };

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Product name is required.");
        }

        [Fact]
        public void Validate_ItemProductNameTooLong_ReportsProductNameError()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto
                {
                    ProductName = new string('A', 101),
                    Quantity = 1,
                    UnitPrice = 1m
                }
            };

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName.EndsWith("ProductName", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Validate_ItemQuantityOutOfRange_FailsWithQuantityMessages()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 0, UnitPrice = 1m },
                new UpdateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "Y", Quantity = 21, UnitPrice = 1m }
            };

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity must be greater than zero.");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Quantity cannot exceed 20.");
        }

        [Fact]
        public void Validate_ItemUnitPriceLessOrEqualZero_FailsWithUnitPriceMessage()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto { ProductId = Guid.NewGuid(), ProductName = "X", Quantity = 1, UnitPrice = 0m }
            };

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Unit price must be greater than zero.");
        }

        [Fact]
        public void Validate_ProductGroupingExceedsMaximum_FailsWithGroupedMessage()
        {
            var cmd = CreateValidCommand();
            // Two items with the same ProductId whose quantities sum > 20
            cmd.Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto { ProductId = BeerId, ProductName = " Beer ", Quantity = 12, UnitPrice = 1m },
                new UpdateSaleItemDto { ProductId = BeerId, ProductName = "beer", Quantity = 10, UnitPrice = 1m }
            };

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("The product 'Beer' exceeds the maximum allowed quantity of 20 units."));
        }

        [Fact]
        public void Validate_ProductGroupingEqualsMaximum_IsValid()
        {
            var cmd = CreateValidCommand();
            cmd.Items = new List<UpdateSaleItemDto>
            {
                new UpdateSaleItemDto { ProductId = BeerId, ProductName = " Beer ", Quantity = 10, UnitPrice = 1m },
                new UpdateSaleItemDto { ProductId = BeerId, ProductName = "beer", Quantity = 10, UnitPrice = 1m }
            };

            var validator = new UpdateSaleValidator();
            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void UpdateSaleCommand_Validate_ReturnsValidationResultDetail_IsValid_WithNoErrors()
        {
            var cmd = CreateValidCommand();

            var validationDetail = cmd.Validate();

            Assert.True(validationDetail.IsValid);
            Assert.Empty(validationDetail.Errors);
        }

        [Fact]
        public void UpdateSaleCommand_Validate_ReturnsValidationResultDetail_WithExpectedErrorForEmptyId()
        {
            var cmd = CreateValidCommand();
            cmd.Id = Guid.Empty;

            var validationDetail = cmd.Validate();

            Assert.False(validationDetail.IsValid);
            Assert.Contains(validationDetail.Errors, e => e.Detail == "Sale ID is required for update.");
        }
    }
}