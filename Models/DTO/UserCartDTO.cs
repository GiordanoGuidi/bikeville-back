using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeVille.Models.DTO
{
    public class UserCartDTO
    {
        public int CustomerId {get;set;}
        public string Name {get;set;}
        public int OrderQty{get;set;}
        public int ProductId {get;set;}
        public decimal UnitPrice {get;set;}
        public DateTime AddedAt {get;set;}
    }
}