using Ambev.DeveloperEvaluation.Application.Sale.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using Moq;
using Xunit;
using System.Threading;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class UpdateSaleHandlerTests
    {
        [Fact]
        public async System.Threading.Tasks.Task Handle_ReturnsOk_WhenServiceUpdatesSale()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new UpdateSaleCommand
            {
                Id = System.Guid.NewGuid(),
                SaleNumber = "S-200",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer A",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch X",
                TotalAmount = 150m
            };

            var mappedSale = new Domain.Entities.Sale
            {
                Id = command.Id,
                SaleNumber = command.SaleNumber,
                CustomerName = command.CustomerName,
                BranchName = command.BranchName,
                TotalAmount = command.TotalAmount
            };

            var updatedSale = new Domain.Entities.Sale
            {
                Id = command.Id,
                SaleNumber = mappedSale.SaleNumber,
                CustomerName = mappedSale.CustomerName,
                BranchName = mappedSale.BranchName,
                TotalAmount = mappedSale.TotalAmount
            };

            saleServiceMock
                .Setup(s => s.GetByIdAsync(It.Is<System.Guid>(id => id == command.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Sale { Id = command.Id });

            mapperMock
                .Setup(m => m.Map<Domain.Entities.Sale>(It.Is<UpdateSaleCommand>(c => c == command)))
                .Returns(mappedSale);

            saleServiceMock
                .Setup(s => s.UpdateAsync(It.Is<Domain.Entities.Sale>(x => x == mappedSale), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSale);

            var handler = new UpdateSaleHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Sale updated successfully", result.Message);
            Assert.Same(updatedSale, result.Data);

            saleServiceMock.Verify(s => s.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<Domain.Entities.Sale>(command), Times.Once);
            saleServiceMock.Verify(s => s.UpdateAsync(mappedSale, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task Handle_ReturnsFail_WhenSaleNotFound()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new UpdateSaleCommand
            {
                Id = System.Guid.NewGuid(),
                SaleNumber = "S-404",
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer B",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch Y",
                TotalAmount = 50m
            };

            saleServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<System.Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Sale?)null);

            var handler = new UpdateSaleHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal($"Sale with ID {command.Id} not found", result.Message);
            Assert.Null(result.Data);

            saleServiceMock.Verify(s => s.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }
    }
}