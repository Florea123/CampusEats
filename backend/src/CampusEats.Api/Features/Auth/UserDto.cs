namespace CampusEats.Api.Features.Auth;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    string? ProfilePictureUrl, //poza de profil
    string? AddressCity,  //oras
    string? AddressStreet,  //strada
    string? AddressNumber,  //numar
    string? AddressDetails,  //detalii suplimentare adresa
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);