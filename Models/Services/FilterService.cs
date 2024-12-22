using Microsoft.EntityFrameworkCore;
using BikeVille.Models.DTO.bike;
using BikeVille.Models.Bike;

namespace BikeVille.Models.Services
{
    public class FilterService
    {
        private readonly AdventureWorksLt2019Context _context;

        public FilterService(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        // Metodo per recuperare i tipi di biciclette
        public async Task<BikeFilters> GetBikeFiltersAsync()
        {
            //recupero i tipi di biciclette
            var bikeTypes = await _context.ProductCategories
                                              .Where(pc => pc.ParentProductCategoryId == 1)
                                              .Select(pc => new BikeTypeFilter
                                              {
                                                  Id = pc.ProductCategoryId,
                                                  Name = pc.Name
                                              })
                                              .ToListAsync();

            //recupero i colori di biciclette
            var bikeColors = await _context.Products
                                               .Join(
                                                    _context.ProductCategories,
                                                    product => product.ProductCategoryId,
                                                    category => category.ProductCategoryId,
                                                    (product, category) => new { Product = product, Category = category }
                                                )
                                                // Filtro per categoria biciclette
                                                .Where(joined => joined.Category.ParentProductCategoryId == 1)
                                                // Seleziono solo i colori
                                                .Select(joined => joined.Product.Color) 
                                                .Where(color => color != null) 
                                                .Distinct() 
        .ToListAsync();

            // Creo i filtri dei colori in una lista di oggetti BikeColorFilter
            var colorFilters = bikeColors.Select(color => new BikeColorFilter
            {
                Color = color
            }).ToList();

            //Creo e popolo e restituisco l'oggetto BikeFilters
            return new BikeFilters
            {
                //Aggiunto i tipi di biciclette
                BikeTypes = bikeTypes,
                BikeColors = colorFilters,
            };

           
        }
    }
}
