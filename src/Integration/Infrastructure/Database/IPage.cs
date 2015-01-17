using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Database
{
    public interface IPage<T>
    {
        /// <summary>
        /// The current page number contained in this page of result set 
        /// </summary>
        long CurrentPage { get; set; }

        /// <summary>
        /// The total number of pages in the full result set
        /// </summary>
        long TotalPages { get; set; }

        /// <summary>
        /// The total number of records in the full result set
        /// </summary>
        long TotalItems { get; set; }

        /// <summary>
        /// The number of items per page
        /// </summary>
        long ItemsPerPage { get; set; }

        /// <summary>
        /// The actual records on this page
        /// </summary>
        List<T> Items { get; set; }

        /// <summary>
        /// User property to hold anything.
        /// </summary>
        object Context { get; set; }
    }
}