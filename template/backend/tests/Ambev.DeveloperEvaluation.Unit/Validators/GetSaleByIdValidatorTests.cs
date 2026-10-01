using Ambev.DeveloperEvaluation.Application.Sale.GetSale;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class GetSaleByIdValidatorTests
    {
        [Fact]
        public void Validate_ValidCommand_IsValid()
        {
            var cmd = new GetSaleByIdCommand
            {
                Id = Guid.NewGuid()
            };

            var validator = new GetSaleByIdValidator();
            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_MissingId_FailsWithRequiredMessage()
        {
            var cmd = new GetSaleByIdCommand
            {
                Id = Guid.Empty
            };

            var validator = new GetSaleByIdValidator();
            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Sale ID is required.");
        }

        [Fact]
        public void GetSaleByIdCommand_Validate_ReturnsValidationResultDetail_IsValid_WithNoErrors()
        {
            var cmd = new GetSaleByIdCommand
            {
                Id = Guid.NewGuid()
            };

            var validationDetail = cmd.Validate();

            Assert.True(validationDetail.IsValid);
            Assert.Empty(validationDetail.Errors);
        }

        [Fact]
        public void GetSaleByIdCommand_Validate_ReturnsValidationResultDetail_WithExpectedErrorForEmptyId()
        {
            var cmd = new GetSaleByIdCommand
            {
                Id = Guid.Empty
            };

            var validationDetail = cmd.Validate();

            Assert.False(validationDetail.IsValid);
            Assert.Contains(validationDetail.Errors, e => e.Detail == "Sale ID is required.");
        }
    }
}