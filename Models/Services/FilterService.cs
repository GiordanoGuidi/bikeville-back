using Microsoft.EntityFrameworkCore;
using BikeVille.Models.DTO.bike;
using BikeVille.Models.DTO.filters;

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
        public async Task<Filters> GetFiltersAsync(int parentCategoryId)
        {
            //recupero i tipi di biciclette
            var types = await _context.ProductCategories
                                              .Where(pc => pc.ParentProductCategoryId == parentCategoryId)
                                              .Select(pc => new TypeFilter
                                              {
                                                  Id = pc.ProductCategoryId,
                                                  Name = pc.Name
                                              })
                                              .ToListAsync();

            //recupero i colori di biciclette
            var colors = await _context.Products
                                               .Join(
                                                    _context.ProductCategories,
                                                    product => product.ProductCategoryId,
                                                    category => category.ProductCategoryId,
                                                    (product, category) => new { Product = product, Category = category }
                                                )
                                                // Filtro per categoria biciclette
                                                .Where(joined => joined.Category.ParentProductCategoryId == parentCategoryId)
                                                // Seleziono solo i colori
                                                .Select(joined => joined.Product.Color) 
                                                .Where(color => color != null) 
                                                .Distinct() 
                                                .ToListAsync();

            //Recupero le taglie delle biciclette
            var sizes = await _context.Products
                                                  .Join(
                                                    _context.ProductCategories,
                                                    product => product.ProductCategoryId,
                                                    category => category.ProductCategoryId,
                                                    (product, category) => new { Product = product, Category = category }
                                                  )
                                                // Filtro per categoria biciclette
                                                .Where(joined => joined.Category.ParentProductCategoryId == parentCategoryId)
                                                .Select(joined => joined.Product.Size)
                                                .Where(size => size != null)
                                                .Distinct()
                                                .ToListAsync();
            // Configuro fasce di prezzo in base alla categoria
            var priceRanges = GetPriceRangesByCategory(parentCategoryId);

            // Creo i filtri dei colori in una lista di oggetti BikeColorFilter
            var colorFilters = colors.Select(color => new ColorFilter
            {
                Color = color
            }).ToList();

            // Creo i filtri delle taglie in una lista di oggetti BikeSizeFilter
            var sizeFilters = sizes.Select(size=>new SizeFilter
            {
                Size= size
            }).ToList();

            //Creo e popolo e restituisco l'oggetto Filters
            return new Filters
            {
                //Aggiunto i tipi di biciclette
                Types = types,
                Colors = colorFilters,
                Sizes = sizeFilters,
                Prices= priceRanges,
            };
        }


        // Metodo per configurare le fasce di prezzo in base alla categoria
        private List<PriceFilter> GetPriceRangesByCategory(int categoryId)
        {
            return categoryId switch
            {
                1 => new List<PriceFilter>
            {
                new PriceFilter { Id = 1, Label = "Up to 700€" },
                new PriceFilter { Id = 2, Label = "700-1500€" },
                new PriceFilter { Id = 3, Label = "1500-2500€" },
                new PriceFilter { Id = 4, Label = "2500€ and more" }
            },
                2 => new List<PriceFilter>
            {
                new PriceFilter { Id = 1, Label = "Up to 100€" },
                new PriceFilter { Id = 2, Label = "100-500€" },
                new PriceFilter { Id = 3, Label = "500-1000€" },
                new PriceFilter { Id = 4, Label = "1000€ and more" }
            },
                3 => new List<PriceFilter>
            {
                new PriceFilter { Id = 1, Label = "Up to 10€" },
                new PriceFilter { Id = 2, Label = "10-30€" },
                new PriceFilter { Id = 3, Label = "30-50€" },
                new PriceFilter { Id = 4, Label = "50€ and more" }
            },
                4 => new List<PriceFilter>
            {
                 new PriceFilter { Id = 1, Label = "Up to 10€" },
                new PriceFilter { Id = 2, Label = "10-30€" },
                new PriceFilter { Id = 3, Label = "30-50€" },
                new PriceFilter { Id = 4, Label = "50€ and more" }
            },
            };
        }
    }
}
