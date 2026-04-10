using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TUnit.Sample.Domain;

namespace TUnit.Sample.Infrastructure.Configuration;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.HasOne(b => b.Author)
            .WithMany(b => b.AuthoredBooks)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}