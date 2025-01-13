namespace BikeVille.Models.DTO
{
    public class CreateProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!; 
        public string? ProductNumber { get; set; }
        public string? Color { get; set; }
        public float? StandardCost { get; set; }
        public float? ListPrice { get; set; }
        public string? Size { get; set; }
        public float? Weight { get; set; } 
        public int ProductCategoryId { get; set; } 
        public int ProductModelId { get; set; } 
        public DateTime SellStartDate { get; set; } 
        public DateTime? SellEndDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
        public string? ThumbnailPhoto { get; set; }
        public string? ThumbnailPhotoFileName { get; set; }
        public string? Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

    }
}
