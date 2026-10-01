using Ambev.DeveloperEvaluation.Application.Sale.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sale.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Services;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using AutoMapper;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Handlers
{
    /// <summary>
    /// Tests covering the sale date resolution and the External Identities rules.
    /// </summary>
    public class SaleDateAndExternalIdentityTests
    {
        [Fact]
        public async Task CreateHandler_WhenDateIsNotInformed_UsesCurrentUtcDate()
        {
            var mapperMock = new Mock<IMapper>();
            var serviceMock = new Mock<ISaleService>();
            var mapped = new Sale();

            mapperMock.Setup(m => m.Map<Sale>(It.IsAny<CreateSaleCommand>())).Returns(mapped);
            serviceMock.Setup(s => s.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sale s, CancellationToken _) => s);

            var before = DateTime.UtcNow;
            var result = await new CreateSaleHandler(serviceMock.Object, mapperMock.Object)
                .Handle(new CreateSaleCommand(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(DateTimeKind.Utc, mapped.Date.Kind);
            Assert.InRange(mapped.Date, before, DateTime.UtcNow);
        }

        [Fact]
        public async Task CreateHandler_WhenDateIsInformed_UsesInformedDateAsUtc()
        {
            var mapperMock = new Mock<IMapper>();
            var serviceMock = new Mock<ISaleService>();
            var mapped = new Sale();
            var informed = new DateTime(2025, 10, 20, 14, 30, 0, DateTimeKind.Utc);

            mapperMock.Setup(m => m.Map<Sale>(It.IsAny<CreateSaleCommand>())).Returns(mapped);
            serviceMock.Setup(s => s.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sale s, CancellationToken _) => s);

            await new CreateSaleHandler(serviceMock.Object, mapperMock.Object)
                .Handle(new CreateSaleCommand { Date = informed }, CancellationToken.None);

            Assert.Equal(informed, mapped.Date);
            Assert.Equal(DateTimeKind.Utc, mapped.Date.Kind);
        }

        [Fact]
        public async Task UpdateHandler_WhenDateIsNotInformed_KeepsOriginalDateAndCancellation()
        {
            var mapperMock = new Mock<IMapper>();
            var serviceMock = new Mock<ISaleService>();
            var id = Guid.NewGuid();
            var originalDate = new DateTime(2025, 1, 10, 8, 0, 0, DateTimeKind.Utc);
            var existing = new Sale { Id = id, Date = originalDate, Cancelled = true };
            var mapped = new Sale();

            serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            mapperMock.Setup(m => m.Map<Sale>(It.IsAny<UpdateSaleCommand>())).Returns(mapped);
            serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sale s, CancellationToken _) => s);

            await new UpdateSaleHandler(serviceMock.Object, mapperMock.Object)
                .Handle(new UpdateSaleCommand { Id = id }, CancellationToken.None);

            Assert.Equal(id, mapped.Id);
            Assert.Equal(originalDate, mapped.Date);
            Assert.True(mapped.Cancelled);
        }

        [Fact]
        public async Task UpdateHandler_WhenDateIsInformed_UsesInformedDate()
        {
            var mapperMock = new Mock<IMapper>();
            var serviceMock = new Mock<ISaleService>();
            var id = Guid.NewGuid();
            var newDate = new DateTime(2025, 2, 1, 12, 0, 0, DateTimeKind.Utc);
            var mapped = new Sale();

            serviceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Sale { Id = id, Date = DateTime.UtcNow.AddDays(-30) });
            mapperMock.Setup(m => m.Map<Sale>(It.IsAny<UpdateSaleCommand>())).Returns(mapped);
            serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sale s, CancellationToken _) => s);

            await new UpdateSaleHandler(serviceMock.Object, mapperMock.Object)
                .Handle(new UpdateSaleCommand { Id = id, Date = newDate }, CancellationToken.None);

            Assert.Equal(newDate, mapped.Date);
        }

        [Fact]
        public void CreateValidator_WithoutExternalIdentities_Fails()
        {
            var command = new CreateSaleCommand
            {
                CustomerName = "Customer",
                BranchName = "Branch",
                Items = new List<CreateSaleItemDto>
                {
                    new() { ProductName = "Beer", Quantity = 1, UnitPrice = 1m }
                }
            };

            var result = new CreateSaleValidator().Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer ID is required");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Branch ID is required");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Product ID is required");
        }

        [Fact]
        public async Task SaleService_GroupsIdenticalItemsByProductId_NotByName()
        {
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();
            repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sale s, CancellationToken _) => s);

            // Same name, different products: 3 + 3 units must NOT be grouped (no discount)
            var sale = new Sale
            {
                Items = new List<SaleItem>
                {
                    new() { ProductId = Guid.NewGuid(), ProductName = "Beer", Quantity = 3, UnitPrice = 10m },
                    new() { ProductId = Guid.NewGuid(), ProductName = "Beer", Quantity = 3, UnitPrice = 10m }
                }
            };

            var result = await new SaleService(repositoryMock.Object, publisherMock.Object).CreateAsync(sale);

            Assert.All(result.Items, i => Assert.Equal(0m, i.DiscountPercent));
            Assert.Equal(60m, result.TotalAmount);
        }
    }
}
