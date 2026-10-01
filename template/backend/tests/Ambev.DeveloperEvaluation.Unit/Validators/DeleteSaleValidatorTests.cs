using Ambev.DeveloperEvaluation.Application.Sale.DeleteSale;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class DeleteSaleValidatorTests
    {
        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var cmd = new DeleteSaleCommand
            {
                Id = Guid.NewGuid()
            };

            var validator = new DeleteSaleValidator();
            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_MissingId_FailsWithRequiredMessage()
        {
            var cmd = new DeleteSaleCommand
            {
                Id = Guid.Empty
            };

            var validator = new DeleteSaleValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Sale ID is required for deletion.");
        }

        [Fact]
        public void DeleteSaleCommand_Validate_ReturnsValidationResultDetail_WithExpectedErrorForEmptyId()
        {
            var cmd = new DeleteSaleCommand
            {
                Id = Guid.Empty
            };

            var validationDetail = cmd.Validate();

            Assert.False(validationDetail.IsValid);
            Assert.Contains(validationDetail.Errors, e => e.Detail == "Sale ID is required for deletion.");
        }
    }
}