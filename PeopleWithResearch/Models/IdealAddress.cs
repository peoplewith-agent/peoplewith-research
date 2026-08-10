using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeopleWithResearch
{
    public class IdealAddress
    {

        // Basic lines provided by the API
        // Formatted address lines
        public string line_1 { get; set; }
        public string line_2 { get; set; }
        public string line_3 { get; set; }
        public string post_town { get; set; }
        public string Postcode { get; set; }

        // Premise elements
        public string BuildingNumber { get; set; }
        public string BuildingName { get; set; }
        public string SubBuildingName { get; set; }
        public string Premise { get; set; }
        public string PoBox { get; set; }
        public string OrganisationName { get; set; }
        public string DepartmentName { get; set; }

        // Thoroughfare
        public string Thoroughfare { get; set; }
        public string DependantThoroughfare { get; set; }

        // Locality
        public string DependantLocality { get; set; }
        public string DoubleDependantLocality { get; set; }

        // Administrative
        public string County { get; set; }
        public string PostalCounty { get; set; }
        public string AdministrativeCounty { get; set; }
        public string TraditionalCounty { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Country { get; set; }

        // Geolocation
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? Eastings { get; set; }
        public int? Northings { get; set; }

        // Identifiers
        public string Udprn { get; set; }
        public string Uprn { get; set; }

    }

    public class IdealResponse
    {
        public List<IdealAddress> result { get; set; }
    }
}
