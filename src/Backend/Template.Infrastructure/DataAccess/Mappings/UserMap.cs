using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities;
using Template.Domain.Validations;

namespace Template.Infrastructure.DataAccess.Mappings;

internal class UserMap : IEntityTypeConfiguration<User>
{
    public const string ActiveFilter = "ActiveUsers";
    public const string ActiveEmailIndex = "IX_users_email_active";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.HasQueryFilter(ActiveFilter, user => user.Active);

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(user => user.Name)
            .HasColumnName("name")
            .HasMaxLength(FieldLimits.UserName)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(FieldLimits.Email)
            .IsRequired();

#if (postgres)
        builder.HasIndex(user => user.Email)
            .HasDatabaseName(ActiveEmailIndex)
            .IsUnique()
            .HasFilter("\"active\" = TRUE");
#elif (sqlserver)
        builder.HasIndex(user => user.Email)
            .HasDatabaseName(ActiveEmailIndex)
            .IsUnique()
            .HasFilter("[active] = 1");
#else
        // O MySQL não tem índice filtrado: a coluna gerada só tem valor para usuários ativos,
        // e o índice único ignora os nulos dos inativos
        builder.Property<string?>("ActiveEmail")
            .HasColumnName("active_email")
            .HasMaxLength(FieldLimits.Email)
            .HasComputedColumnSql("(CASE WHEN active THEN email END)", stored: true);

        builder.HasIndex("ActiveEmail")
            .HasDatabaseName(ActiveEmailIndex)
            .IsUnique();
#endif

        builder.Property(user => user.Password)
            .HasColumnName("password")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(user => user.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(user => user.DeactivatedAt)
            .HasColumnName("deactivated_at");

#if (postgres)
        builder.Property<uint>("Version").IsRowVersion();
#elif (sqlserver)
        builder.Property<byte[]>("Version")
            .HasColumnName("version")
            .IsRowVersion();
#else
        builder.Property<Guid>(TemplateDbContext.VersionProperty)
            .HasColumnName("version")
            .IsConcurrencyToken();
#endif
    }
}
