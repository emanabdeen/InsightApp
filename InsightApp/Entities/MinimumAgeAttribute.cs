using System.ComponentModel.DataAnnotations;

namespace InsightApp.Entities
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minimumAge = 16;

        public MinimumAgeAttribute()
        {
            ErrorMessage = "You must be at least 16 years old.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateOnly date )
            {
                var today = DateOnly.FromDateTime(DateTime.Now);

                // Calculate the age based on the year difference
                int age = today.Year - date.Year;

                // If the birthday has not occurred this year yet, subtract 1 from the age
                if (date > today.AddYears(-age))
                {
                    age--;
                }

                if (age >= _minimumAge)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            // If the value is null or not a valid date, we assume other validation rules (like [Required]) will handle it
            return ValidationResult.Success;
        }
    }
}

