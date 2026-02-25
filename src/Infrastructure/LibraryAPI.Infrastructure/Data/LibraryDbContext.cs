using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using LibraryAPI.Application.Interfaces;

namespace LibraryAPI.Infrastructure.Data
{
    public class LibraryDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;

        public LibraryDbContext(
            DbContextOptions<LibraryDbContext> options,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }

        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<BookAuthor> BookAuthors { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<EmailTemplate> EmailTemplates { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Many-to-Many relationship
            modelBuilder.Entity<BookAuthor>()
                .HasKey(ba => new { ba.BookId, ba.AuthorId });

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Book)
                .WithMany(b => b.BookAuthors)
                .HasForeignKey(ba => ba.BookId);

            modelBuilder.Entity<BookAuthor>()
                .HasOne(ba => ba.Author)
                .WithMany(a => a.BookAuthors)
                .HasForeignKey(ba => ba.AuthorId);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fiction", Description = "Fiction books" },
                new Category { Id = 2, Name = "Non-Fiction", Description = "Non-Fiction books" },
                new Category { Id = 3, Name = "Science", Description = "Science books" }
            );

            // Configure Branch relationships
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Branch)
                .WithMany()
                .HasForeignKey(b => b.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global Query Filters
            modelBuilder.Entity<ApplicationUser>().HasQueryFilter(u =>
                _currentUserService.IsSuperAdmin || u.BranchId == _currentUserService.BranchId);

            modelBuilder.Entity<Book>().HasQueryFilter(b =>
                _currentUserService.IsSuperAdmin || b.BranchId == _currentUserService.BranchId);

            modelBuilder.Entity<AuditLog>().HasQueryFilter(a =>
                _currentUserService.IsSuperAdmin || a.BranchId == _currentUserService.BranchId);

            modelBuilder.Entity<EmailTemplate>()
                .HasOne(e => e.Branch)
                .WithMany()
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Only SuperAdmins can see all templates. BranchAdmins/Employees see their branch's templates PLUS the global templates (BranchId == null).
            modelBuilder.Entity<EmailTemplate>().HasQueryFilter(e =>
                _currentUserService.IsSuperAdmin || e.BranchId == null || e.BranchId == _currentUserService.BranchId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChangesAsync(auditEntries, cancellationToken);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            var userId = _currentUserService.UserId;
            var branchId = _currentUserService.BranchId;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // Log solo las entidades de dominio base u otras de interés (ignorar Identity roles/tokens si se quiere, por ahora registramos todo)
                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    UserId = userId,
                    BranchId = branchId
                };
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        // value will be generated by the database, get the value after saving
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified &&
                                (property.OriginalValue != null && !property.OriginalValue.Equals(property.CurrentValue) ||
                                 property.CurrentValue != null && !property.CurrentValue.Equals(property.OriginalValue)))
                            {
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                auditEntry.ChangedColumns.Add(propertyName);
                            }
                            break;
                    }
                }
            }

            // Guardar aquellas entradas de auditoría que no tienen propiedades temporales
            foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            // Retornar aquellas que sí tienen para actualizarlas luego (ej. IDs autogenerados)
            return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChangesAsync(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                // Ahora las propiedades temporales tienen el valor generado por la DB
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Guardar la entidad de auditoría completada
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }

    // Helper interno para manejar los datos temporalmente
    internal class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string? UserId { get; set; }
        public int? BranchId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public Dictionary<string, object?> KeyValues { get; } = new();
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
        public AuditType AuditType { get; set; }
        public List<string> ChangedColumns { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAuditLog()
        {
            var audit = new AuditLog
            {
                UserId = UserId,
                BranchId = BranchId,
                Type = AuditType,
                TableName = TableName,
                DateTime = DateTime.UtcNow,
                PrimaryKey = JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
                AffectedColumns = ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns)
            };

            return audit;
        }
    }
}
