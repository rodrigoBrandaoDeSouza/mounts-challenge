using Ambev.DeveloperEvaluation.Application.Sale.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class CreateSaleHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsOk_WhenServiceCreatesSale()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new CreateSaleCommand
            {
                SaleNumber = "S-123",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer A",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch X",
                TotalAmount = 100m,
                Cancelled = false
            };

            var mappedSale = new Domain.Entities.Sale
            {
                SaleNumber = command.SaleNumber,
                CustomerName = command.CustomerName,
                BranchName = command.BranchName,
                TotalAmount = command.TotalAmount,
                Cancelled = command.Cancelled
            };

            var createdSale = new Domain.Entities.Sale
            {
                Id = System.Guid.NewGuid(),
                SaleNumber = mappedSale.SaleNumber,
                CustomerName = mappedSale.CustomerName,
                BranchName = mappedSale.BranchName,
                TotalAmount = mappedSale.TotalAmount,
                Cancelled = mappedSale.Cancelled
            };

            mapperMock
                .Setup(m => m.Map<Domain.Entities.Sale>(It.Is<CreateSaleCommand>(c => c == command)))
                .Returns(mappedSale);

            saleServiceMock
                .Setup(s => s.CreateAsync(It.Is<Domain.Entities.Sale>(x => x == mappedSale), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdSale);

            var handler = new CreateSaleHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Sale created", result.Message);
            Assert.Same(createdSale, result.Data);

            mapperMock.Verify(m => m.Map<Domain.Entities.Sale>(command), Times.Once);
            saleServiceMock.Verify(s => s.CreateAsync(mappedSale, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenServiceReturnsNull()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new CreateSaleCommand
            {
                SaleNumber = "S-404",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer B",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch Y",
                TotalAmount = 50m,
                Cancelled = false
            };

            var mappedSale = new Domain.Entities.Sale
            {
                SaleNumber = command.SaleNumber,
                CustomerName = command.CustomerName,
                BranchName = command.BranchName,
                TotalAmount = command.TotalAmount,
                Cancelled = command.Cancelled
            };

            mapperMock
                .Setup(m => m.Map<Domain.Entities.Sale>(It.Is<CreateSaleCommand>(c => c == command)))
                .Returns(mappedSale);

            saleServiceMock
                .Setup(s => s.CreateAsync(It.IsAny<Domain.Entities.Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Sale?)null);

            var handler = new CreateSaleHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Fail to create sale", result.Message);
            Assert.Null(result.Data);

            mapperMock.Verify(m => m.Map<Domain.Entities.Sale>(command), Times.Once);
            saleServiceMock.Verify(s => s.CreateAsync(mappedSale, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}