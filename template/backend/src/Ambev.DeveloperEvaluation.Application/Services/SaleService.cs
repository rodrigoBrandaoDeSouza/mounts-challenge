using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Messaging.Events;
using Ambev.DeveloperEvaluation.Messaging.Interfaces;

namespace Ambev.DeveloperEvaluation.Application.Services
{
    /// <summary>
    /// Application service of the sale aggregate: applies the business rules, persists the
    /// changes and publishes the domain events.
    /// </summary>
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repository;
        private readonly IMessagePublisher _publisher;

        public SaleService(ISaleRepository repository, IMessagePublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }

        /// <inheritdoc/>
        public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            await EnsureSaleNumberIsUniqueAsync(sale.SaleNumber, null, cancellationToken);

            // A new sale is never cancelled; totals and discounts are always calculated here.
            sale.Cancelled = false;
            sale.Items.ForEach(i => i.Cancelled = false);
            sale.RecalculateTotals();

            var created = await _repository.CreateAsync(sale, cancellationToken);

            await _publisher.PublishAsync(new SaleCreatedEvent
            {
                SaleId = created.Id,
                SaleNumber = created.SaleNumber,
                SaleDate = created.Date,
                CustomerId = created.CustomerId,
                CustomerName = created.CustomerName,
                BranchId = created.BranchId,
                BranchName = created.BranchName,
                TotalAmount = created.TotalAmount,
                Items = MapItems(created)
            }, cancellationToken);

            return created;
        }

        /// <inheritdoc/>
        public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            await EnsureSaleNumberIsUniqueAsync(sale.SaleNumber, sale.Id, cancellationToken);

            sale.EnsureNotCancelled();
            sale.RecalculateTotals();

            var updated = await _repository.UpdateAsync(sale, cancellationToken);

            await _publisher.PublishAsync(new SaleModifiedEvent
            {
                SaleId = updated.Id,
                SaleNumber = updated.SaleNumber,
                SaleDate = updated.Date,
                CustomerId = updated.CustomerId,
                CustomerName = updated.CustomerName,
                BranchId = updated.BranchId,
                BranchName = updated.BranchName,
                TotalAmount = updated.TotalAmount,
                Items = MapItems(updated)
            }, cancellationToken);

            return updated;
        }

        /// <inheritdoc/>
        public async Task<Sale> CancelAsync(Guid saleId, CancellationToken cancellationToken = default)
        {
            var sale = await GetRequiredAsync(saleId, cancellationToken);

            sale.Cancel();
            var updated = await _repository.UpdateAsync(sale, cancellationToken);

            await _publisher.PublishAsync(new SaleCancelledEvent
            {
                SaleId = updated.Id,
                SaleNumber = updated.SaleNumber,
                TotalAmount = updated.TotalAmount
            }, cancellationToken);

            return updated;
        }

        /// <inheritdoc/>
        public async Task<Sale> CancelItemAsync(Guid saleId, Guid itemId, CancellationToken cancellationToken = default)
        {
            var sale = await GetRequiredAsync(saleId, cancellationToken);

            var item = sale.CancelItem(itemId);
            var updated = await _repository.UpdateAsync(sale, cancellationToken);

            await _publisher.PublishAsync(new ItemCancelledEvent
            {
                SaleId = updated.Id,
                SaleNumber = updated.SaleNumber,
                ItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                SaleTotalAmount = updated.TotalAmount
            }, cancellationToken);

            return updated;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid saleId, CancellationToken cancellationToken = default)
        {
            var deleted = await _repository.DeleteAsync(saleId, cancellationToken);

            if (deleted)
                await _publisher.PublishAsync(new SaleDeletedEvent { SaleId = saleId }, cancellationToken);

            return deleted;
        }

        /// <inheritdoc/>
        public Task<Sale?> GetByIdAsync(Guid saleId, CancellationToken cancellationToken = default) =>
            _repository.GetByIdAsync(saleId, cancellationToken);

        /// <inheritdoc/>
        public Task<PagedResult<Sale>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default) =>
            _repository.ListAsync(options, cancellationToken);

        private async Task<Sale> GetRequiredAsync(Guid saleId, CancellationToken cancellationToken) =>
            await _repository.GetByIdAsync(saleId, cancellationToken)
                ?? throw ResourceNotFoundException.For("Sale", saleId);

        private async Task EnsureSaleNumberIsUniqueAsync(string saleNumber, Guid? saleId, CancellationToken cancellationToken)
        {
            if (await _repository.SaleNumberExistsAsync(saleNumber, saleId, cancellationToken))
                throw new ConflictException("Sale number already exists", $"There is already a sale with the number '{saleNumber}'");
        }

        private static List<SaleEventItem> MapItems(Sale sale) =>
            sale.Items.Select(i => new SaleEventItem
            {
                ItemId = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                TotalPrice = i.TotalPrice,
                Cancelled = i.Cancelled
            }).ToList();
    }
}
