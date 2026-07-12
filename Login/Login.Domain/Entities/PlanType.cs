namespace Login.Domain.Entities;

public enum PlanType
{
    None    = 0,
    Trial   = 1,
    Monthly = 2,
    Annual  = 3,
    /// <summary>Plano pago para assessores financeiros — libera carteira de clientes ilimitada.</summary>
    Assessor = 4,
}
