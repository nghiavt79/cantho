using System.Collections.Generic;
using TechExchangeApp.Domain.Models;

namespace TechExchangeApp.ViewModel
{
    /// <summary>
    /// Response model for AI supplier suggestion endpoint.
    /// </summary>
    public class SuggestResponse
    {
        /// <summary>
        /// List of matched suppliers with their scores and top products
        /// </summary>
        public List<SupplierMatchResult> Suppliers { get; set; } = new List<SupplierMatchResult>();
    }
}
