using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Smartstore.Core.Companies.Domain
{
    internal class VisitorMap : IEntityTypeConfiguration<Visitor>
    {
        public void Configure(EntityTypeBuilder<Visitor> builder)
        {
            // Globally exclude soft-deleted entities from all queries.
            builder.HasQueryFilter(c => !c.Deleted);

            builder.HasOne(c => c.Company)
                .WithMany(c => c.Visitors)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public partial class Visitor : BaseEntity, ISoftDeletable
    {
        public Visitor()
        {
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private member.", Justification = "Required for EF lazy loading")]
        private Visitor(ILazyLoader lazyLoader)
            : base(lazyLoader)
        {
        }

        /// <summary>
        /// Gets or sets the Guid
        /// </summary>
        [Required] 
        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets company unique id param
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the guest has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the company identifier.
        /// </summary>
        [Required]
        public int CompanyId { get; set; }

        private Company _company;
        /// <summary>
        /// Gets or sets the company record.
        /// </summary>
        [ForeignKey(nameof(CompanyId))]
        [JsonIgnore]
        public Company Company
        {
            get => _company ?? LazyLoader.Load(this, ref _company);
            set => _company = value;
        }
    }
}
