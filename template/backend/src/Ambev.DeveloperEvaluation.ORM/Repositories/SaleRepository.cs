using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Querying;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="ISaleRepository"/>.
    /// </summary>
    public class SaleRepository : ISaleRepository
    {
        private readonly DefaultContext _context;

        public SaleRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            await _context.Sales.AddAsync(sale, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return sale;
        }

        /// <remarks>The sale is tracked, so changes made to the aggregate are persisted by <see cref="UpdateAsync"/>.</remarks>
        public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        public Task<PagedResult<Sale>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default) =>
            _context.Sales
                .Include(s => s.Items)
                .AsNoTracking()
                .ToPagedResultAsync(options, cancellationToken);

        /// <remarks>
        /// Items added to the aggregate are inserted, changed items are updated and items removed
        /// from the aggregate are deleted (orphan removal).
        /// </remarks>
        public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
        {
            if (_context.Entry(sale).State == EntityState.Detached)
                _context.Sales.Update(sale);

            await _context.SaveChangesAsync(cancellationToken);
            return sale;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var sale = await GetByIdAsync(id, cancellationToken);
            if (sale is null)
                return false;

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public Task<bool> SaleNumberExistsAsync(string saleNumber, Guid? ignoreSaleId = null, CancellationToken cancellationToken = default) =>
            _context.Sales.AnyAsync(
                s => s.SaleNumber == saleNumber && (ignoreSaleId == null || s.Id != ignoreSaleId),
                cancellationToken);
    }
}
