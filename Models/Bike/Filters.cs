using BikeVille.Models.DTO.bike;

namespace BikeVille.Models.Bike
{
    public class Filters
    {
        //parametro che contiene una lista di tipi di biciclette
        public IEnumerable<TypeFilter> Types { get; set; }
        public IEnumerable<ColorFilter> Colors { get; set; }
        public IEnumerable<SizeFilter> Sizes { get; set; }
        public IEnumerable<PriceFilter> Prices { get; set; }


    }
}
