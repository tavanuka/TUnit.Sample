using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TUnit.Sample.Domain;

namespace TUnit.Sample.Infrastructure.Configuration;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{

    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(254);
        
        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(254);
        
        builder.Property(p => p.BirthDate)
            .IsRequired();
        
        builder.HasIndex(p => p.BirthDate);
    }
}