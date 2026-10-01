using Ambev.DeveloperEvaluation.Application.Sale.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Sale
{
    public class DeleteSaleHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsOk_WhenServiceDeletesSale()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new DeleteSaleCommand
            {
                Id = Guid.NewGuid()
            };

            saleServiceMock
                .Setup(s => s.DeleteAsync(It.Is<Guid>(id => id == command.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new DeleteSaleHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Sale deleted.", result.Message);
            Assert.Null(result.Data);

            saleServiceMock.Verify(s => s.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenServiceFailsToDelete()
        {
            // Arrange
            var mapperMock = new Mock<IMapper>();
            var saleServiceMock = new Mock<ISaleService>();

            var command = new DeleteSaleCommand
            {
                Id = Guid.NewGuid()
            };

            saleServiceMock
                .Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handler = new DeleteSaleHandler(saleServiceMock.Object, mapperMock.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal("Fail to delete Sale.", result.Message);
            Assert.Null(result.Data);

            saleServiceMock.Verify(s => s.DeleteAsync(command.Id, It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.VerifyNoOtherCalls();
        }
    }
}