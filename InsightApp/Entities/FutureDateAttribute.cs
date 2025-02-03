using System.ComponentModel.DataAnnotations;

namespace InsightApp.Entities
{
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute()
        {
            ErrorMessage = "The date must be in the future.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateOnly date)
            {
                if (date > DateOnly.FromDateTime(DateTime.Now))
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
