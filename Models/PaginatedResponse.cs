using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcoMarket.Models
{
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class PaginationParams
    {
        private int _pageNumber;
        private int _pageSize;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value;
        }

        public (int pageNumber, int pageSize) GetValidatedValues()
        {
            // Validate and adjust page number
            var validPageNumber = PageNumber < 1 ? 1 : PageNumber;

            // Validate and adjust page size
            var validPageSize = PageSize switch
            {
                < 1 => 10,   // Default to 10 if less than 1
                > 50 => 50,  // Cap at 50
                _ => PageSize
            };

            return (validPageNumber, validPageSize);
        }
    }
}
