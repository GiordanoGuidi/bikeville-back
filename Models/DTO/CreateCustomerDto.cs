namespace BikeVille.Models.DTO
{
    public class CreateCustomerDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? EmailAddress { get; set; }
        public string? CompanyName { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
    }
}
