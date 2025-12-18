using CampusEats.Api.Enums;

namespace CampusEats.Api.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    // Hashed with PBKDF2 via PasswordHasher<User>
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public LoyaltyAccount? LoyaltyAccount { get; set; }
    
    public string? ProfilePictureUrl { get; set; } //poza de profil
    
    public string? AddressCity { get; set; } //oras
    public string? AddressStreet { get; set; } //strada
    public string? AddressNumber { get; set; } //numar
    
    public string? AddressDetails { get; set; } //detalii suplimentare adresa
    
}