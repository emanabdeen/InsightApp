using System.ComponentModel.DataAnnotations;

namespace InsightApp.Attributes
{
    public class MustHaveOneItemAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var list = value as ICollection<int?>;

            // Check if the list is null or empty
            if (list == null || !list.Any())
            {
                return new ValidationResult(ErrorMessage ?? "At least one item must be selected.");
            }

            return ValidationResult.Success;
        }
    }
}
