//using LibraryManagement.Domain.Interfaces;
//using LibraryManagementSystem.Domain.Entities;
//using LibraryManagementSystem.Domain.Entities.Common;
//using LibraryManagementSystem.Domain.Interfaces;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Linq;
//using System.Threading.Tasks;


//namespace LibraryManagementSystem.Infrastructure.Persistence.DataSeed.Migration
//{
//    public static class CountrySeed
//    {
//        public static readonly Country[] Countries =
//        {
//            new Country { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "United States", ISOCode = "US" },
//            new Country { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "United Kingdom", ISOCode = "UK" },
//            new Country { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "United Arab Emirates", ISOCode = "AE" },
//            new Country { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "India", ISOCode = "IN" },
//            new Country { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Pakistan", ISOCode = "PK" }
//        };

//        public static void Seed(ModelBuilder modelBuilder)
//        {
//            foreach (Country country in Countries)
//            {
//                country.CreatedAt = new DateTime(2025, 9, 9, 0, 0, 0, DateTimeKind.Utc);
//                country.IsDeleted = false;

//                modelBuilder.Entity<Country>().HasData(country);
//            }        
//        }

//    }
//}
