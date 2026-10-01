using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseIdentityByDefaultColumn();

        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.Date).IsRequired();

        builder.HasIndex(c => c.UserId);

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Cart)
            .HasForeignKey(p => p.CartId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
