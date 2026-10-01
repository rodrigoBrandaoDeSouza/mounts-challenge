using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories
{
    /// <summary>
    /// Implementation of ISaleRepository using Entity Framework Core
    /// </summary>
    public class SaleRepository : ISaleRepository
    {
        private readonly DefaultContext _context;

        /// <summary>
        /// Initializes a new instance of SaleRepository
        /// </summary>
        /// <param name="context">The database context</param>
        public SaleRepository(DefaultContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new sale in the database
        /// </summary>
        /// <param name="sale">The sale to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created sale</returns>
        public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            await _context.Sales.AddAsync(sale, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return sale;
        }

        /// <summary>
        /// Retrieves a sale by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the sale</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The sale if found, null otherwise</returns>
        public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sales
                .Include(s => s.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }


        /// <summary>
        /// Updates an existing sale in the database
        /// </summary>
        /// <param name="sale">The sale to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated sale</returns>
        /// <remarks>
        /// The sale is loaded (tracked) with its items and synchronized with the received data:
        /// items with a known Id are updated, items without Id (or with an unknown Id) are added,
        /// and existing items that were not sent are removed.
        /// </remarks>
        public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == sale.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Sale with ID {sale.Id} not found");

            // Scalar properties (SaleNumber, Date, Customer*, Branch*, TotalAmount, Cancelled)
            _context.Entry(existing).CurrentValues.SetValues(sale);

            var incomingIds = sale.Items
                .Where(i => i.Id != Guid.Empty)
                .Select(i => i.Id)
                .ToHashSet();

            // Remove items that are no longer part of the sale
            foreach (var removed in existing.Items.Where(i => !incomingIds.Contains(i.Id)).ToList())
            {
                existing.Items.Remove(removed);
                _context.SaleItems.Remove(removed);
            }

            foreach (var item in sale.Items)
            {
                item.SaleId = existing.Id;

                var current = item.Id == Guid.Empty
                    ? null
                    : existing.Items.FirstOrDefault(i => i.Id == item.Id);

                if (current is not null)
                {
                    _context.Entry(current).CurrentValues.SetValues(item);
                }
                else
                {
                    existing.Items.Add(new SaleItem
                    {
                        SaleId = existing.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountPercent = item.DiscountPercent,
                        TotalPrice = item.TotalPrice,
                        Cancelled = item.Cancelled
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        /// <summary>
        /// Deletes a sale from the database
        /// </summary>
        /// <param name="id">The unique identifier of the sale to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the sale was deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var sale = await GetByIdAsync(id, cancellationToken);
            if (sale == null)
                return false;

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Retrieves a paginated list of sales
        /// </summary>
        /// <param name="page">The number of pagination page</param>
        /// <param name="pageSize">The size of pagination page</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The list of sales if found, empty list otherwise</returns>
        public async Task<IEnumerable<Sale>> FetchSales(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            int skip = (page - 1) * pageSize;

            return await _context.Sales
                .Include(s => s.Items)
                .AsNoTracking()
                .OrderByDescending(s => s.Date)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}
