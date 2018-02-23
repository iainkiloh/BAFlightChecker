using System;

namespace FlightCheckerMvcCore.ViewModels
{
    public class PagingInfo
    {
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
     

        public int PageNumber { get; set; }

        //public int TotalPages { get; set; }

        public int TotalPages
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / PageSize); }
        }

    }

}
