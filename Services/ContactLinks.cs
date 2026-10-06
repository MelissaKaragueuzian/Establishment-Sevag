using System.Text.RegularExpressions;
using Website_Idea.Models;

namespace Website_Idea.Services;

public static class ContactLinks
{
    public static string? WhatsApp(string? number, string message)
    {
        var digits = Digits(number);
        if (digits.Length < 8)
            return null;

        return $"https://wa.me/{digits}?text={Uri.EscapeDataString(message)}";
    }

    public static string? Phone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var cleaned = new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return cleaned.Length < 3 ? null : "tel:" + cleaned;
    }

    public static string? Mail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : "mailto:" + email.Trim();

    public static string? OpenMaps(CompanyInfo company)
    {
        if (!string.IsNullOrWhiteSpace(company.GoogleMapsUrl))
            return company.GoogleMapsUrl.Trim();

        if (!string.IsNullOrWhiteSpace(company.Address))
            return "https://www.google.com/maps/search/?api=1&query=" + Uri.EscapeDataString(company.Address.Trim());

        return null;
    }

    public static string? EmbedMaps(CompanyInfo company)
    {
        var query = Coordinates(company.GoogleMapsUrl)
            ?? (!string.IsNullOrWhiteSpace(company.Address)
                ? company.Address.Trim()
                : company.GoogleMapsUrl?.Trim());

        if (string.IsNullOrWhiteSpace(query))
            return null;

        return "https://maps.google.com/maps?q=" + Uri.EscapeDataString(query) + "&z=16&output=embed";
    }

    // The !3d/!4d pair is the place's own pin; the @lat,lng pair is only where the viewport was centered.
    private static string? Coordinates(string? mapsUrl)
    {
        if (string.IsNullOrWhiteSpace(mapsUrl))
            return null;

        var pin = Regex.Match(mapsUrl, @"!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)");
        if (pin.Success)
            return $"{pin.Groups[1].Value},{pin.Groups[2].Value}";

        var view = Regex.Match(mapsUrl, @"@(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)");
        return view.Success ? $"{view.Groups[1].Value},{view.Groups[2].Value}" : null;
    }

    private static string Digits(string? value) =>
        new((value ?? "").Where(char.IsDigit).ToArray());
}
