using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.ToTable("Sales");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(s => s.SaleNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.Date)
                .IsRequired();

            // External Identities: Customer and Branch live in other bounded contexts.
            builder.Property(s => s.CustomerId)
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(s => s.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.BranchId)
                .HasColumnType("uuid")
                .IsRequired();

            builder.Property(s => s.BranchName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.Cancelled)
                .HasDefaultValue(false);

            builder.HasMany(s => s.Items)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
