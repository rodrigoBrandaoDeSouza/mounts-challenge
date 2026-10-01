using Ambev.DeveloperEvaluation.Application.Sale.FetchSales;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class FetchSalesValidatorTests
    {
        [Fact]
        public void Validate_DefaultCommand_IsValid()
        {
            var cmd = new FetchSalesCommand();

            var validator = new FetchSalesValidator();
            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_WithPagination_IsValid()
        {
            var cmd = new FetchSalesCommand
            {
                Page = 1,
                PageSize = 10
            };

            var validator = new FetchSalesValidator();
            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void FetchSalesCommand_Validate_ReturnsValidationResultDetail_IsValid_WithNoErrors()
        {
            var cmd = new FetchSalesCommand
            {
                Page = 1,
                PageSize = 10
            };

            var validationDetail = cmd.Validate();

            Assert.True(validationDetail.IsValid);
            Assert.Empty(validationDetail.Errors);
        }
    }
}