using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Unit.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

public class SaleHandlersTests
{
    private readonly ISaleService _saleService = Substitute.For<ISaleService>();
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact(DisplayName = "Create maps the command and returns the created sale")]
    public async Task Create_ReturnsCreatedSale()
    {
        var command = SaleTestData.ValidCreateCommand();
        _saleService.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var sale = call.Arg<Sale>();
                sale.Id = Guid.NewGuid();
                sale.RecalculateTotals();
                return sale;
            });

        var result = await new CreateSaleHandler(_saleService, _mapper).Handle(command, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.CustomerId.Should().Be(command.CustomerId);
        result.CustomerName.Should().Be(command.CustomerName);
        result.BranchId.Should().Be(command.BranchId);
        result.Items.Should().HaveCount(command.Items.Count);
        result.TotalAmount.Should().Be(result.Items.Sum(i => i.TotalPrice));
    }

    [Fact(DisplayName = "Create uses the current UTC date when the date is omitted")]
    public async Task Create_WithoutDate_UsesUtcNow()
    {
        var command = SaleTestData.ValidCreateCommand();
        command.Date = null;
        Sale? captured = null;
        _saleService.CreateAsync(Arg.Do<Sale>(s => captured = s), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Sale>());

        await new CreateSaleHandler(_saleService, _mapper).Handle(command, CancellationToken.None);

        captured!.Date.Kind.Should().Be(DateTimeKind.Utc);
        captured.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact(DisplayName = "Get returns the sale")]
    public async Task Get_ReturnsSale()
    {
        var sale = SaleTestData.Sale();
        _saleService.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await new GetSaleHandler(_saleService, _mapper).Handle(new GetSaleQuery(sale.Id), CancellationToken.None);

        result.Id.Should().Be(sale.Id);
        result.Items.Should().HaveCount(sale.Items.Count);
    }

    [Fact(DisplayName = "Get throws ResourceNotFoundException when the sale does not exist")]
    public async Task Get_NotFound_Throws()
    {
        _saleService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => new GetSaleHandler(_saleService, _mapper).Handle(new GetSaleQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact(DisplayName = "Update changes the sale and keeps the date when it is omitted")]
    public async Task Update_ChangesSale()
    {
        var sale = SaleTestData.Sale();
        var originalDate = sale.Date.AddDays(-3);
        sale.Date = originalDate;
        var command = SaleTestData.ValidUpdateCommand(sale.Id, items: 1);
        command.Date = null;

        _saleService.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleService.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await new UpdateSaleHandler(_saleService, _mapper).Handle(command, CancellationToken.None);

        result.SaleNumber.Should().Be(command.SaleNumber);
        result.CustomerName.Should().Be(command.CustomerName);
        result.Date.Should().Be(originalDate);
        result.Items.Should().ContainSingle().Which.ProductId.Should().Be(command.Items[0].ProductId);
        await _saleService.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update of a cancelled sale is not allowed")]
    public async Task Update_CancelledSale_Throws()
    {
        var sale = SaleTestData.Sale();
        sale.Cancel();
        _saleService.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => new UpdateSaleHandler(_saleService, _mapper).Handle(SaleTestData.ValidUpdateCommand(sale.Id), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        await _saleService.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Update throws ResourceNotFoundException when the sale does not exist")]
    public async Task Update_NotFound_Throws()
    {
        _saleService.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => new UpdateSaleHandler(_saleService, _mapper).Handle(SaleTestData.ValidUpdateCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact(DisplayName = "Delete throws ResourceNotFoundException when the sale does not exist")]
    public async Task Delete_NotFound_Throws()
    {
        _saleService.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        var act = () => new DeleteSaleHandler(_saleService).Handle(new DeleteSaleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact(DisplayName = "Delete succeeds when the sale exists")]
    public async Task Delete_Succeeds()
    {
        var id = Guid.NewGuid();
        _saleService.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        await new DeleteSaleHandler(_saleService).Handle(new DeleteSaleCommand(id), CancellationToken.None);

        await _saleService.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "List parses the query and returns the page")]
    public async Task List_ReturnsPage()
    {
        var sales = new List<Sale> { SaleTestData.Sale(), SaleTestData.Sale() };
        QueryOptions? captured = null;
        _saleService.ListAsync(Arg.Do<QueryOptions>(o => captured = o), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<Sale>(sales, totalItems: 12, currentPage: 2, pageSize: 2));

        var query = new ListSalesQuery { Page = 2, Size = 2, Order = "totalAmount desc" };
        query.Filters["customerName"] = new[] { "Bar*" };

        var result = await new ListSalesHandler(_saleService, _mapper).Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(12);
        result.TotalPages.Should().Be(6);
        captured!.Page.Should().Be(2);
        captured.Orderings.Should().ContainSingle().Which.PropertyPath.Should().Be("TotalAmount");
        captured.FilterGroups.Should().ContainSingle();
    }

    [Fact(DisplayName = "Cancel returns the cancelled sale")]
    public async Task Cancel_ReturnsSale()
    {
        var sale = SaleTestData.Sale();
        sale.Cancel();
        _saleService.CancelAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await new CancelSaleHandler(_saleService, _mapper).Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        result.Cancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "CancelItem returns the sale with the item cancelled")]
    public async Task CancelItem_ReturnsSale()
    {
        var sale = SaleTestData.Sale();
        var itemId = sale.Items[0].Id;
        sale.CancelItem(itemId);
        _saleService.CancelItemAsync(sale.Id, itemId, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await new CancelSaleItemHandler(_saleService, _mapper).Handle(new CancelSaleItemCommand(sale.Id, itemId), CancellationToken.None);

        result.Items.Single(i => i.Id == itemId).Cancelled.Should().BeTrue();
    }
}
