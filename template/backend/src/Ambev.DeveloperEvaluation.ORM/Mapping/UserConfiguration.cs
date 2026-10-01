using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).UseIdentityByDefaultColumn();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Password).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).IsRequired().HasMaxLength(20);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.Firstname).HasColumnName("FirstName").IsRequired().HasMaxLength(50);
            name.Property(n => n.Lastname).HasColumnName("LastName").IsRequired().HasMaxLength(50);
        });

        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(a => a.City).HasColumnName("City").IsRequired().HasMaxLength(100);
            address.Property(a => a.Street).HasColumnName("Street").IsRequired().HasMaxLength(100);
            address.Property(a => a.Number).HasColumnName("Number");
            address.Property(a => a.Zipcode).HasColumnName("Zipcode").IsRequired().HasMaxLength(20);
            address.Property(a => a.Latitude).HasColumnName("Latitude").IsRequired().HasMaxLength(20);
            address.Property(a => a.Longitude).HasColumnName("Longitude").IsRequired().HasMaxLength(20);
        });

        builder.Navigation(u => u.Name).IsRequired();
        builder.Navigation(u => u.Address).IsRequired();

        builder.Ignore(u => u.IsActive);
    }
}
