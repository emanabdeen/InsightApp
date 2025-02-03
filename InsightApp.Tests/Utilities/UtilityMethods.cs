using InsightApp.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InsightApp.Tests.Utilities
{
    public static class UtilityMethods
    {
        //public static DbContextOptions<InsightUpdateCvgs2Context> TestDbContextOptions()
        //{
        //    //create new service provider to use in-memory database
        //    var serviceProvider = new ServiceCollection().Add
                
        //}
        public static SqliteConnection _connection = new SqliteConnection("FileName=:memory:");

        public static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);

            Validator.TryValidateObject(model, validationContext, results, true);
            
            return results;
        }
    }
}