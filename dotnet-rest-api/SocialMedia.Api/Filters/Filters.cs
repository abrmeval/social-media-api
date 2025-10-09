using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Api.Models
{
    /// <summary>
    /// Validates IFormFile properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class FileAttribute : ValidationAttribute
    {
        private readonly long _maxLength;
        private readonly string[]? _extensions;
        private readonly Mode _mode;

        public FileAttribute(long maxLength = 0)
        {
            _maxLength = maxLength;
            _mode = Mode.Size;
        }
        public FileAttribute(params string[] extensions)
        {
            _extensions = extensions.Select(e => e.ToLower()).ToArray();  // Convert extensions to lowercase
            _mode = Mode.Extension;
        }
        public FileAttribute(long maxLength = 0, params string[] extensions)
        {
            _maxLength = maxLength;
            _extensions = extensions.Select(e => e.ToLower()).ToArray();  // Convert extensions to lowercase
            _mode = Mode.Both;
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || !value.GetType().Equals(typeof(FormFile)))
                return ValidationResult.Success!;

            var file = (IFormFile)value;

            switch (_mode)
            {
                case Mode.Size:
                    return ValidateSize(file, validationContext);
                case Mode.Extension:
                    return ValidateExtension(file, validationContext);
                case Mode.Both:
                    var result = ValidateSize(file, validationContext);

                    if (result != ValidationResult.Success) return result;

                    return ValidateExtension(file, validationContext);
                default:
                    return ValidationResult.Success!;
            }
        }

        private ValidationResult ValidateSize(IFormFile file, ValidationContext validationContext)
        {
            if (file.Length == 0)
                return new ValidationResult($"El campo {validationContext.DisplayName} debe tener un tamaño mayor a 0 bytes.");

            // Check file size
            if (_maxLength > 0 && file.Length > _maxLength)
                return new ValidationResult(ErrorMessage ?? $"El campo {validationContext.DisplayName} debe ser menor o igual a ({_maxLength}) bytes.");

            return ValidationResult.Success!;
        }

        private ValidationResult ValidateExtension(IFormFile file, ValidationContext validationContext)
        {
            // Check file extension
            if (_extensions == null) return ValidationResult.Success!;

            // Convert file extension to lowercase
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_extensions.Contains(fileExtension))
                return new ValidationResult(ErrorMessage ?? $"El campo {validationContext.DisplayName} debe ser de tipo ({string.Join(" , ", _extensions)}).");

            return ValidationResult.Success!;
        }

        private enum Mode
        {
            Size,
            Extension,
            Both
        }
    }
}