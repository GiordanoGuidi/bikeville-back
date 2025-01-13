namespace BikeVille.Models.DTO
{
    public class UpdateProductDTO
    {
        public string? Name { get; set; }
        public string? ProductNumber { get; set; }
        public string? Color { get; set; }
        public decimal? StandardCost { get; set; }
        public decimal? ListPrice { get; set; }
        public string? Size { get; set; }
        public decimal? Weight { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductModel { get; set; }
        public DateTime? SellStartDate { get; set; }
        public DateTime? SellEndDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
        public string? ThumbnailPhotoFileName { get; set; }
        public string? ThumbnailPhoto { get; set; } // Base64 string
    }
}
