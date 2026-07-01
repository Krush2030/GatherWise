using System.Collections.Generic;
using GatherWise.Domain.Entities;

namespace GatherWise.Web.Models
{
    public class VenueDetailsSelectionViewModel
    {
        public Venue Venue { get; set; } = new Venue();

        // Categorized Vendor Lists
        public IEnumerable<Vendor> Caterers { get; set; } = new List<Vendor>();
        public IEnumerable<Vendor> Decorators { get; set; } = new List<Vendor>();
        public IEnumerable<Vendor> Photographers { get; set; } = new List<Vendor>();
        public IEnumerable<Vendor> DJs { get; set; } = new List<Vendor>();
    }
}