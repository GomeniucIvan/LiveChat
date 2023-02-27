using Smartstore.Core.Identity;
using Smartstore.Core.Localization;
using Smartstore.Core.Platform.Identity.Dtos;

namespace Smartstore.Core
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Initializes the work context by pre-resolving current customer
        /// working language and working currency asynchronously.
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Gets a value indicating whether the work context was initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        Customer CurrentCustomer { get; set; }

        /// <summary>
        /// Gets or sets the current customer by id
        /// </summary>
        CustomerWebContextDto CurrentCustomerById(int customerId);

        /// <summary>
        /// Gets the original customer (in case the current in <see cref="CurrentCustomer"/> is impersonated)
        /// </summary>
        Customer CurrentImpersonator { get; }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        Language WorkingLanguage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we're in admin area
        /// </summary>
		bool IsAdminArea { get; set; }
    }
}