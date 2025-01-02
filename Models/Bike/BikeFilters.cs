using BikeVille.Models.DTO.bike;

namespace BikeVille.Models.Bike
{
    public class BikeFilters
    {
        //parametro che contiene una lista di tipi di biciclette
        public IEnumerable<BikeTypeFilter> BikeTypes { get; set; }
        public IEnumerable<BikeColorFilter> BikeColors { get; set; }
        public IEnumerable<BikeSizeFilter> BikeSizes { get; set; }
        public IEnumerable<BikePriceFilter> BikePrices { get; set; }


    }
}
