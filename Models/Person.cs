using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CsvHelper.Configuration;

namespace sample_dotnet_webapi.Models
{
    public class Person
    {
        public Guid PersonId { get; set; }

        [MinLength(1)]
        public string FirstName { get; set; } = "";

        [MinLength(1)]
        public string LastName { get; set; } = "";

        [CustomValidation(typeof(PersonValidator), nameof(PersonValidator.ValidateBirthDate))]
        public DateOnly BirthDate { get; set; }

        [CustomValidation(
            typeof(PersonValidator),
            nameof(PersonValidator.ValidateNonNegativeDecimal)
        )]
        public decimal Income { get; set; }

        [CustomValidation(
            typeof(PersonValidator),
            nameof(PersonValidator.ValidateNonNegativeDecimal)
        )]
        public decimal Balance { get; set; }
    }

    public static class PersonValidator
    {
        public static ValidationResult? ValidateBirthDate(
            Object? birthDate,
            ValidationContext context
        )
        {
            if (birthDate == null)
                return new ValidationResult("Object cannot be null");

            if (birthDate is not DateOnly)
                return new ValidationResult("Object must be DateOnly");

            var Date18YearsAgo = DateOnly.FromDateTime(DateTime.Now.AddYears(-18));
            if (((DateOnly)birthDate).CompareTo(Date18YearsAgo) > 0)
                return new ValidationResult("Age must be greater than 18 years");

            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateNonNegativeDecimal(
            Object? number,
            ValidationContext context
        )
        {
            if (number == null)
                return new ValidationResult("Object cannot be null");

            if (number is not decimal)
                return new ValidationResult("Object must be decimal");

            if ((decimal)number < 0)
                return new ValidationResult("Number must be at least 0");

            return ValidationResult.Success;
        }
    }

    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.PersonId).Ignore();
        }
    }
}
