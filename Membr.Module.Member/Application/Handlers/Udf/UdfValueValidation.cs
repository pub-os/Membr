namespace Membr.Module.Member.Application.Handlers.Udf;

using System.Text.Json;
using Domain;

internal static class UdfValueValidation
{
    public static string? ValidateDefinition(string name, UdfFieldType type, List<string> options, string? defaultValue)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Name is required.";

        if (type == UdfFieldType.MultiSelect && options.Count == 0)
            return "Options must contain at least one value for a multi-select field.";

        if (type != UdfFieldType.MultiSelect && options.Count > 0)
            return "Options may only be set for a multi-select field.";

        return ValidateValue(type, defaultValue, options);
    }

    public static string? ValidateValue(UdfFieldType type, string? value, List<string> options)
    {
        if (value is null)
            return null;

        switch (type)
        {
            case UdfFieldType.Bool:
                if (!bool.TryParse(value, out _))
                    return "Value must be 'true' or 'false' for a bool field.";
                break;

            case UdfFieldType.Date:
                if (!DateOnly.TryParse(value, out _))
                    return "Value must be a valid date (yyyy-MM-dd) for a date field.";
                break;

            case UdfFieldType.DateTime:
                if (!DateTime.TryParse(value, out _))
                    return "Value must be a valid ISO date-time for a datetime field.";
                break;

            case UdfFieldType.String:
                break;

            case UdfFieldType.MultiSelect:
                List<string>? selected;
                try
                {
                    selected = JsonSerializer.Deserialize<List<string>>(value);
                }
                catch (JsonException)
                {
                    return "Value must be a JSON array of strings for a multi-select field.";
                }

                if (selected is null)
                    return "Value must be a JSON array of strings for a multi-select field.";

                if (selected.Any(s => !options.Contains(s)))
                    return "Value contains a selection that is not one of the field's options.";
                break;

            default:
                return "Unknown field type.";
        }

        return null;
    }
}
