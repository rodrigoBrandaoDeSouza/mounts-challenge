
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ambev.DeveloperEvaluation.Application.Services;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Messaging.Events;
using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Application.Tests.Services
{
    public class SaleServiceTests
    {
        [Fact]
        public async Task CreateAsync_AppliesDiscounts_PersistsAndPublishesEvent_ReturnsCreatedSale()
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
                    // Simulate repository assigning Id
                    repositoryReturnSale = new Sale
                    {
                        Id = Guid.NewGuid(),
                        SaleNumber = s.SaleNumber,
                        Date = s.Date,
                        Customer = s.Customer,
                        Branch = s.Branch,
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
                Customer = "Customer A",
                Branch = "Branch X",
                Items = new List<SaleItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
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
            Assert.Equal(result.Customer, capturedEvent.Customer);
            Assert.Equal(result.Branch, capturedEvent.Branch);
            Assert.Single(capturedEvent.Items);
            var eventItem = capturedEvent.Items.Single();
            Assert.Equal(item.ProductName, eventItem.ProductName);
            Assert.Equal(item.Quantity, eventItem.Quantity);
            Assert.Equal(item.UnitPrice, eventItem.UnitPrice);

            // OccurredAt should be recent
            Assert.True((DateTime.UtcNow - capturedEvent.OccurredAt).TotalSeconds < 5);
        }

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
                .ReturnsAsync((Sale s, CancellationToken ct) => {
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
                Customer = "Customer B",
                Branch = "Branch Y",
                Items = new List<SaleItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductName = " Beer ",
                        Quantity = 2,
                        UnitPrice = 5m
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "beer",
                        Quantity = 3,
                        UnitPrice = 5m
                    }
                }
            };

            // Act
            var result = await service.CreateAsync(sale);

            // Assert - captured repository sale exists
            Assert.NotNull(capturedRepositorySale);
            // Both items should have discountPercent 10 (total quantity 5 => 10%)
            foreach (var it in capturedRepositorySale!.Items)
            {
                Assert.Equal(10m, it.DiscountPercent);
                var expectedTotal = it.UnitPrice * it.Quantity * (100m - 10m) / 100m;
                Assert.Equal(expectedTotal, it.TotalPrice);
            }

            // TotalAmount should be sum of item totals
            var expectedTotalAmount = capturedRepositorySale.Items.Sum(i => i.TotalPrice);
            Assert.Equal(expectedTotalAmount, capturedRepositorySale.TotalAmount);
        }

        [Fact]
        public async Task CreateAsync_UsesCreatedSale_WhenBuildingEvent()
        {
            // Arrange
            var repositoryMock = new Mock<ISaleRepository>();
            var publisherMock = new Mock<IMessagePublisher>();

            var createdSale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "S-003",
                Date = DateTime.UtcNow,
                Customer = "Customer C",
                Branch = "Branch Z",
                Items = new List<SaleItem>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "Soda",
                        Quantity = 1,
                        UnitPrice = 3m,
                        DiscountPercent = 0m,
                        TotalPrice = 3m
                    }
                },
                TotalAmount = 3m
            };

            repositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdSale);

            SaleCreatedEvent? capturedEvent = null;
            publisherMock
                .Setup(p => p.PublishAsync(It.IsAny<SaleCreatedEvent>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((msg, ct) => capturedEvent = (SaleCreatedEvent)msg)
                .Returns(Task.CompletedTask);

            var service = new SaleService(repositoryMock.Object, publisherMock.Object);

            var inputSale = new Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "S-003-input",
                Date = DateTime.UtcNow,
                Customer = "Ignorable",
                Branch = "IgnoreBranch",
                Items = new List<SaleItem>()
            };

            // Act
            var result = await service.CreateAsync(inputSale);

            // Assert - event values reflect createdSale returned by repository
            Assert.Same(createdSale, result);
            Assert.NotNull(capturedEvent);
            Assert.Equal(createdSale.Id, capturedEvent!.SaleId);
            Assert.Equal(createdSale.Customer, capturedEvent.Customer);
            Assert.Equal(createdSale.Branch, capturedEvent.Branch);
            Assert.Single(capturedEvent.Items);
            var evItem = capturedEvent.Items.Single();
            Assert.Equal(createdSale.Items.Single().ProductName, evItem.ProductName);
            Assert.Equal(createdSale.Items.Single().Quantity, evItem.Quantity);
            Assert.Equal(createdSale.Items.Single().UnitPrice, evItem.UnitPrice);
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
                Customer = "Customer D",
                Branch = "Branch W",
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
    }
}