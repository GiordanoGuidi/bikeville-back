namespace BikeVille.Models.DTO
{
    public class CreateProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!; //required field i.e. non-nullable
        public string? ProductNumber { get; set; }
        public string? Color { get; set; }
        public float? StandardCost { get; set; }
        public float? ListPrice { get; set; }
        public string? Size { get; set; }
        public float? Weight { get; set; } 
        public int ProductCategoryId { get; set; } //required field i.e. non-nullable
        public int ProductModelId { get; set; } //required field i.e. non-nullable
        public DateTime SellStartDate { get; set; } //required field i.e. non-nullable
        public DateTime? SellEndDate { get; set; }
        public DateTime? DiscontinuedDate { get; set; }
        public string? ThumbnailPhoto { get; set; }
        public string? ThumbnailPhotoFileName { get; set; }
        public string? Rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }

        //public string? ProductCategory { get; set; }
        //public string? ProductModel { get; set; }
        //public string[]? SalesOrderDetails { get; set; } = [];
    }
}
