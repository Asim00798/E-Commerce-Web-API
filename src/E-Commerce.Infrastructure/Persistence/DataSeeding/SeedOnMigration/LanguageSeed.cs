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
//    public static class LanguageSeed
//    {
//        public static readonly Language[] Languages = 
//        {
//            new Language { Id = 1, Name = "English" },
//            new Language { Id = 2, Name = "Arabic" },
//            new Language { Id = 3, Name = "French" },
//            new Language { Id = 4, Name = "Hindi" },
//            new Language { Id = 5, Name = "Urdu" }
//        };

//        public static void Seed(ModelBuilder modelBuilder)
//        {
//            foreach (Language language in Languages)
//            {
//                language.CreatedAt = new DateTime(2025, 9, 9, 0, 0, 0, DateTimeKind.Utc);
//                language.IsDeleted = false;

//                modelBuilder.Entity<Language>().HasData(language);
//            }
//        }
//    }

//}

