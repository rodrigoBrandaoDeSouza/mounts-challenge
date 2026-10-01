using Ambev.DeveloperEvaluation.Application.Sale.FetchSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class FetchSalesHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsOk_WhenServiceReturnsSales()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new FetchSalesCommand
            {
                Page = 1,
                PageSize = 10
            };

            var sale = new Domain.Entities.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "S-001",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer A",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch X",
                Date = DateTime.UtcNow,
                TotalAmount = 123.45m,
                Cancelled = false
            };

            var salesEnumerable = new List<Domain.Entities.Sale> { sale };

            saleServiceMock
                .Setup(s => s.FetchAsync(It.Is<int>(p => p == command.Page),
                                         It.Is<int>(ps => ps == command.PageSize),
                                         It.IsAny<CancellationToken>()))
                .ReturnsAsync(salesEnumerable);

            var handler = new FetchSalesHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Sales found", result.Message);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(sale.Id, result.Data.First().Id);

            saleServiceMock.Verify(s => s.FetchAsync(command.Page, command.PageSize, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenServiceReturnsEmptyList()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new FetchSalesCommand
            {
                Page = 1,
                PageSize = 10
            };

            saleServiceMock
                .Setup(s => s.FetchAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Domain.Entities.Sale>());

            var handler = new FetchSalesHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("No Sales found", result.Message);
            Assert.Null(result.Data);

            saleServiceMock.Verify(s => s.FetchAsync(command.Page, command.PageSize, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenServiceReturnsNull()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new FetchSalesCommand
            {
                Page = 2,
                PageSize = 5
            };

            saleServiceMock
                .Setup(s => s.FetchAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<Domain.Entities.Sale>?)null);

            var handler = new FetchSalesHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("No Sales found", result.Message);
            Assert.Null(result.Data);

            saleServiceMock.Verify(s => s.FetchAsync(command.Page, command.PageSize, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }
    }
}