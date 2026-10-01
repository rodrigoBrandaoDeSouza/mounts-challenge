using Ambev.DeveloperEvaluation.Application.Sale.GetSale;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class GetSaleByIdHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsOk_WhenServiceReturnsSale()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new GetSaleByIdCommand
            {
                Id = System.Guid.NewGuid()
            };

            var sale = new Domain.Entities.Sale
            {
                Id = command.Id,
                SaleNumber = "S-100",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer A",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch X",
                Date = System.DateTime.UtcNow,
                TotalAmount = 200m,
                Cancelled = false
            };

            saleServiceMock
                .Setup(s => s.GetByIdAsync(It.Is<System.Guid>(id => id == command.Id), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(sale);

            var handler = new GetSaleByIdHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, System.Threading.CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Sale found", result.Message);
            Assert.Same(sale, result.Data);

            saleServiceMock.Verify(s => s.GetByIdAsync(command.Id, It.IsAny<System.Threading.CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenServiceReturnsNull()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new GetSaleByIdCommand
            {
                Id = System.Guid.NewGuid()
            };

            saleServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<System.Guid>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Sale?)null);

            var handler = new GetSaleByIdHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, System.Threading.CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Sale not found", result.Message);
            Assert.Null(result.Data);

            saleServiceMock.Verify(s => s.GetByIdAsync(command.Id, It.IsAny<System.Threading.CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }
    }
}