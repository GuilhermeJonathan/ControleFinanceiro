namespace ControleFinanceiro.Domain.Entities;

/// <summary>Vínculo entre um número de WhatsApp e um usuário do sistema.</summary>
public class WhatsAppVinculo
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>Número normalizado — somente dígitos, ex: "5511999990000".</summary>
    public string PhoneNumber { get; private set; } = "";

    public DateTime CreatedAt { get; private set; }

    private WhatsAppVinculo() { }

    public WhatsAppVinculo(Guid userId, string phoneNumber)
    {
        Id          = Guid.NewGuid();
        UserId      = userId;
        PhoneNumber = Normalize(phoneNumber);
        CreatedAt   = DateTime.UtcNow;
    }

    /// <summary>Remove qualquer caractere não-numérico do telefone.</summary>
    public static string Normalize(string phone) =>
        new string(phone.Where(char.IsDigit).ToArray());
}
