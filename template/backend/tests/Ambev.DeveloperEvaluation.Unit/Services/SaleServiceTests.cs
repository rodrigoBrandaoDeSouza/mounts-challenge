using Ambev.DeveloperEvaluation.Application.Services;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Messaging.Events;
using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Services
{
    public class SaleServiceTests
    {
        private static readonly Guid BeerId = Guid.NewGuid();

        [Fact]
        public async Task CreateAsync_AppliesDiscountRules_CaseInsensitiveGroupingAndPercentCalculation()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            Sale? capturedRepositorySale = null;

            repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .Callback<Sale, CancellationToken>((s, ct) => capturedRepositorySale = s)
                .ReturnsAsync((Sale s, CancellationToken ct) =>
                {
                    s.Id = Guid.NewGuid();
                    return s;
                });

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "S-002",
                Date = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer B",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch Y",
                Items = new List<SaleItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = BeerId,
                        ProductName = " Beer ",
                        Quantity = 2,
                        UnitPrice = 5m
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = BeerId,
                        ProductName = "beer",
                        Quantity = 3,
                        UnitPrice = 5m
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = BeerId,
                        ProductName = "beer",
                        Quantity = 4,
                        UnitPrice = 5m
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = BeerId,
                        ProductName = "beer",
                        Quantity = 3,
                        UnitPrice = 5m
                    }
                }
            };

            // Act
            var result = await service.CreateAsync(sale);

            // Asserts
            Assert.NotNull(capturedRepositorySale);

            foreach (var it in capturedRepositorySale!.Items)
            {
                Assert.Equal(20m, it.DiscountPercent);
                var expectedTotal = it.UnitPrice * it.Quantity * (100m - 20m) / 100m;
                Assert.Equal(expectedTotal, it.TotalPrice);
            }

            var expectedTotalAmount = capturedRepositorySale.Items.Sum(i => i.TotalPrice);
            Assert.Equal(expectedTotalAmount, capturedRepositorySale.TotalAmount);
        }

        [Fact]
        public async Task CreateAsync_PersistsAndPublishesEvent_ReturnsCreatedSale()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            Sale? capturedRepositorySale = null;
            Sale? repositoryReturnSale = null;
            SaleCreatedEvent? capturedEvent = null;

            repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .Callback<Sale, CancellationToken>((s, ct) =>
                {
                    capturedRepositorySale = s;
                    // Simulate repository assigning Id and returning a persisted object
                    repositoryReturnSale = new Sale
                    {
                        Id = Guid.NewGuid(),
                        SaleNumber = s.SaleNumber,
                        Date = s.Date,
                        CustomerName = s.CustomerName,
                        BranchName = s.BranchName,
                        Items = s.Items.Select(i => new SaleItem
                        {
                            Id = i.Id,
                            SaleId = i.SaleId,
                            ProductName = i.ProductName,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            DiscountPercent = i.DiscountPercent,
                            TotalPrice = i.TotalPrice,
                            Cancelled = i.Cancelled
                        }).ToList(),
                        TotalAmount = s.TotalAmount,
                        Cancelled = s.Cancelled
                    };
                })
                .ReturnsAsync(() => repositoryReturnSale!);

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => capturedEvent = (SaleCreatedEvent)msg)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "S-001",
                Date = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer A",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch X",
                Items = new List<SaleItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductId = BeerId,
                        ProductName = "Beer",
                        Quantity = 2,
                        UnitPrice = 10m
                    }
                }
            };

            // Act
            var result = await service.CreateAsync(sale);

            // Assert - repository received sale with applied totals/discounts
            Assert.NotNull(capturedRepositorySale);
            var item = capturedRepositorySale!.Items.Single();
            Assert.Equal(0m, item.DiscountPercent); // no discount for quantity 2
            Assert.Equal(20m, item.TotalPrice); // 2 * 10

            // Assert - repository returned value is returned by service
            Assert.Same(repositoryReturnSale, result);

            // Assert - publisher called with event built from repository returned sale
            Assert.NotNull(capturedEvent);
            Assert.Equal(result.Id, capturedEvent!.SaleId);
            Assert.Equal(result.CustomerName, capturedEvent.CustomerName);
            Assert.Equal(result.BranchName, capturedEvent.BranchName);
            Assert.Single(capturedEvent.Items);
            var eventItem = capturedEvent.Items.Single();
            Assert.Equal(item.ProductName, eventItem.ProductName);
            Assert.Equal(item.Quantity, eventItem.Quantity);
            Assert.Equal(item.UnitPrice, eventItem.UnitPrice);

            // OccurredAt should be recent
            Assert.True((DateTime.UtcNow - capturedEvent.OccurredAt).TotalSeconds < 5);
        }

        [Fact]
        public async Task CreateAsync_ForwardsCancellationToken_ToRepositoryAndPublisher()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            CancellationToken? tokenReceivedByRepo = null;
            CancellationToken? tokenReceivedByPublisher = null;

            repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .Callback<Sale, CancellationToken>((s, ct) => tokenReceivedByRepo = ct)
                .ReturnsAsync((Sale s, CancellationToken ct) =>
                {
                    s.Id = Guid.NewGuid();
                    return s;
                });

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => tokenReceivedByPublisher = ct)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            var cts = new CancellationTokenSource();
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "S-004",
                Date = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer D",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch W",
                Items = new List<SaleItem>()
            };

            // Act
            await service.CreateAsync(sale, cts.Token);

            // Assert
            Assert.NotNull(tokenReceivedByRepo);
            Assert.NotNull(tokenReceivedByPublisher);
            Assert.Equal(cts.Token, tokenReceivedByRepo!.Value);
            Assert.Equal(cts.Token, tokenReceivedByPublisher!.Value);
        }

        [Fact]
        public async Task DeleteAsync_DeletesAndPublishesEvent_WhenDeleted()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            var saleId = Guid.NewGuid();
            repositoryMock
                .Setup(r => r.DeleteAsync(saleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            SaleDeletedEvent? capturedEvent = null;
            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleDeletedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => capturedEvent = (SaleDeletedEvent)msg)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            // Act
            var deleted = await service.DeleteAsync(saleId);

            // Assert
            Assert.True(deleted);
            Assert.NotNull(capturedEvent);
            Assert.Equal(saleId, capturedEvent!.SaleId);
            Assert.True((DateTime.UtcNow - capturedEvent.OccurredAt).TotalSeconds < 5);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotPublish_WhenNotDeleted()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            var saleId = Guid.NewGuid();
            repositoryMock
                .Setup(r => r.DeleteAsync(saleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleDeletedEvent>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Should not be called"));

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            // Act
            var deleted = await service.DeleteAsync(saleId);

            // Assert
            Assert.False(deleted);
            publisherMock.Verify(p => p.PublishAsync(It.IsAny<SaleDeletedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ForwardsCancellationToken_ToRepositoryAndPublisher()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            CancellationToken? tokenReceivedByRepo = null;
            CancellationToken? tokenReceivedByPublisher = null;

            var saleId = Guid.NewGuid();
            repositoryMock
                .Setup(r => r.DeleteAsync(saleId, It.IsAny<CancellationToken>()))
                .Callback<Guid, CancellationToken>((id, ct) => tokenReceivedByRepo = ct)
                .ReturnsAsync(true);

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleDeletedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => tokenReceivedByPublisher = ct)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);
            var cts = new CancellationTokenSource();

            // Act
            await service.DeleteAsync(saleId, cts.Token);

            // Assert
            Assert.NotNull(tokenReceivedByRepo);
            Assert.NotNull(tokenReceivedByPublisher);
            Assert.Equal(cts.Token, tokenReceivedByRepo!.Value);
            Assert.Equal(cts.Token, tokenReceivedByPublisher!.Value);
        }

        [Fact]
        public async Task FetchAsync_ForwardsToRepository_ReturnsValues()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            var sales = new List<Sale>
            {
                new Sale { Id = Guid.NewGuid(), SaleNumber = "F-1", Items = new List<SaleItem>() },
                new Sale { Id = Guid.NewGuid(), SaleNumber = "F-2", Items = new List<SaleItem>() }
            };

            repositoryMock
                .Setup(r => r.FetchSales(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sales);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            // Act
            var result = await service.FetchAsync(1, 10);

            // Assert
            Assert.Equal(sales, result);
        }

        [Fact]
        public async Task GetByIdAsync_ForwardsToRepository_ReturnsValueOrNull()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = "G-1", Items = new List<SaleItem>() };

            repositoryMock
                .Setup(r => r.GetByIdAsync(sale.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sale);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            // Act
            var found = await service.GetByIdAsync(sale.Id);
            var notFound = await service.GetByIdAsync(Guid.NewGuid()); // not configured -> null

            // Assert
            Assert.Same(sale, found);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task UpdateAsync_AppliesDiscounts_PersistsAndPublishesEvent_ReturnsUpdatedSale()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            Sale? capturedUpdateArg = null;
            SaleUpdatedEvent? capturedEvent = null;

            repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .Callback<Sale, CancellationToken>((s, ct) => capturedUpdateArg = s)
                .ReturnsAsync((Sale s, CancellationToken ct) =>
                {
                    // Simulate repository persisting and returning same object with Id
                    s.Id = s.Id == Guid.Empty ? Guid.NewGuid() : s.Id;
                    return s;
                });

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleUpdatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => capturedEvent = (SaleUpdatedEvent)msg)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            var saleToUpdate = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "U-2",
                Date = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Cust",
                BranchId = Guid.NewGuid(),
                BranchName = "Branch",
                Items = new List<SaleItem>
                {
                    new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Cola", Quantity = 4, UnitPrice = 2m }
                }
            };

            // Act
            var updated = await service.UpdateAsync(saleToUpdate);

            // Assert - repository update received sale with discount applied (quantity 4 => 10%)
            Assert.NotNull(capturedUpdateArg);
            var updatedItem = capturedUpdateArg!.Items.Single();
            Assert.Equal(10m, updatedItem.DiscountPercent);
            Assert.Equal(updatedItem.UnitPrice * updatedItem.Quantity * (100m - 10m) / 100m, updatedItem.TotalPrice);

            // Assert returned value matches repository return
            Assert.Equal(capturedUpdateArg.Id, updated.Id);

            // Assert event published built from repository returned sale
            Assert.NotNull(capturedEvent);
            Assert.Equal(updated.Id, capturedEvent!.SaleId);
            Assert.Equal(updated.CustomerName, capturedEvent.CustomerName);
            Assert.Equal(updated.BranchName, capturedEvent.BranchName);
            Assert.Single(capturedEvent.Items);
            Assert.Equal(updatedItem.ProductName, capturedEvent.Items.Single().ProductName);
        }

        [Fact]
        public async Task UpdateAsync_UsesUpdatedSale_WhenBuildingEvent()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            var returnedSale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "EX",
                CustomerId = Guid.NewGuid(),
                CustomerName = "UpdatedCustomer",
                BranchId = Guid.NewGuid(),
                BranchName = "UpdatedBranch",
                Items = new List<SaleItem>
                {
                    new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Water", Quantity = 1, UnitPrice = 1m, DiscountPercent = 0m, TotalPrice = 1m }
                },
                TotalAmount = 1m
            };

            repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnedSale);

            SaleUpdatedEvent? capturedEvent = null;
            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleUpdatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => capturedEvent = (SaleUpdatedEvent)msg)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            var input = new Sale { Id = returnedSale.Id, SaleNumber = "EX-input", Items = new List<SaleItem>() };

            // Act
            var result = await service.UpdateAsync(input);

            // Assert
            Assert.Same(returnedSale, result);
            Assert.NotNull(capturedEvent);
            Assert.Equal(returnedSale.Id, capturedEvent!.SaleId);
            Assert.Equal(returnedSale.CustomerName, capturedEvent.CustomerName);
            Assert.Equal(returnedSale.BranchName, capturedEvent.BranchName);
            Assert.Single(capturedEvent.Items);
            Assert.Equal(returnedSale.Items.Single().ProductName, capturedEvent.Items.Single().ProductName);
        }

        [Fact]
        public async Task UpdateAsync_ForwardsCancellationToken_ToRepositoryAndPublisher()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            CancellationToken? tokenReceivedByUpdate = null;
            CancellationToken? tokenReceivedByPublisher = null;

            repositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .Callback<Sale, CancellationToken>((s, ct) => tokenReceivedByUpdate = ct)
                .ReturnsAsync((Sale s, CancellationToken ct) => s);

            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleUpdatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => tokenReceivedByPublisher = ct)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);
            var cts = new CancellationTokenSource();

            var toUpdate = new Sale { Id = Guid.NewGuid(), SaleNumber = "U-CTS", Items = new List<SaleItem>() };

            // Act
            await service.UpdateAsync(toUpdate, cts.Token);

            // Assert
            Assert.NotNull(tokenReceivedByUpdate);
            Assert.NotNull(tokenReceivedByPublisher);
            Assert.Equal(cts.Token, tokenReceivedByUpdate!.Value);
            Assert.Equal(cts.Token, tokenReceivedByPublisher!.Value);
        }
    }
}