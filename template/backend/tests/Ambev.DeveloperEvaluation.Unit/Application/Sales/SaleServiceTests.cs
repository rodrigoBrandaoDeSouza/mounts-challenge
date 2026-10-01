using Ambev.DeveloperEvaluation.Application.Services;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Messaging.Events;
using Ambev.DeveloperEvaluation.Messaging.Interfaces;
using Ambev.DeveloperEvaluation.Unit.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class SaleServiceTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
    private readonly SaleService _service;

    public SaleServiceTests()
    {
        _service = new SaleService(_repository, _publisher);
        _repository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Sale>());
        _repository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Sale>());
    }

    [Fact(DisplayName = "Create applies the discounts, persists and publishes SaleCreated")]
    public async Task Create_AppliesDiscounts_PersistsAndPublishes()
    {
        var beer = Guid.NewGuid();
        var sale = SaleTestData.Sale();
        sale.Items = new List<SaleItem> { SaleTestData.Item(beer, 10, 10m) };
        sale.TotalAmount = 0;

        var created = await _service.CreateAsync(sale);

        created.TotalAmount.Should().Be(80m);
        created.Items[0].DiscountPercent.Should().Be(20m);
        await _repository.Received(1).CreateAsync(sale, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(
            Arg.Is<SaleCreatedEvent>(e => e.SaleId == sale.Id && e.TotalAmount == 80m && e.Items.Count == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "A new sale is never created as cancelled")]
    public async Task Create_ResetsCancellation()
    {
        var sale = SaleTestData.Sale();
        sale.Cancelled = true;
        sale.Items.ForEach(i => i.Cancelled = true);

        var created = await _service.CreateAsync(sale);

        created.Cancelled.Should().BeFalse();
        created.Items.Should().OnlyContain(i => !i.Cancelled);
    }

    [Fact(DisplayName = "Create with a duplicated sale number throws ConflictException")]
    public async Task Create_DuplicatedSaleNumber_Throws()
    {
        var sale = SaleTestData.Sale();
        _repository.SaleNumberExistsAsync(sale.SaleNumber, null, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _service.CreateAsync(sale);

        await act.Should().ThrowAsync<ConflictException>();
        await _repository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update persists and publishes SaleModified")]
    public async Task Update_PublishesSaleModified()
    {
        var sale = SaleTestData.Sale();

        await _service.UpdateAsync(sale);

        await _repository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _publisher.Received(1).PublishAsync(Arg.Is<SaleModifiedEvent>(e => e.SaleId == sale.Id), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Cancel persists and publishes SaleCancelled")]
    public async Task Cancel_PublishesSaleCancelled()
    {
        var sale = SaleTestData.Sale();
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _service.CancelAsync(sale.Id);

        result.Cancelled.Should().BeTrue();
        await _publisher.Received(1).PublishAsync(Arg.Is<SaleCancelledEvent>(e => e.SaleId == sale.Id), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Cancel of an unknown sale throws ResourceNotFoundException")]
    public async Task Cancel_NotFound_Throws()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _service.CancelAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact(DisplayName = "CancelItem persists and publishes ItemCancelled")]
    public async Task CancelItem_PublishesItemCancelled()
    {
        var sale = SaleTestData.Sale(items: 2);
        var item = sale.Items[0];
        _repository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _service.CancelItemAsync(sale.Id, item.Id);

        result.Items[0].Cancelled.Should().BeTrue();
        result.TotalAmount.Should().Be(sale.Items[1].TotalPrice);
        await _publisher.Received(1).PublishAsync(
            Arg.Is<ItemCancelledEvent>(e => e.SaleId == sale.Id && e.ItemId == item.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Delete publishes SaleDeleted only when the sale existed")]
    public async Task Delete_PublishesSaleDeleted()
    {
        var existing = Guid.NewGuid();
        _repository.DeleteAsync(existing, Arg.Any<CancellationToken>()).Returns(true);

        (await _service.DeleteAsync(existing)).Should().BeTrue();
        (await _service.DeleteAsync(Guid.NewGuid())).Should().BeFalse();

        await _publisher.Received(1).PublishAsync(Arg.Is<SaleDeletedEvent>(e => e.SaleId == existing), Arg.Any<CancellationToken>());
    }
}
