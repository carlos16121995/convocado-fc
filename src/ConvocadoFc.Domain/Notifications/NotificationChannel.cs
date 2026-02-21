namespace ConvocadoFc.Domain.Notifications;

/// <summary>
/// Canais de envio de notificações.
/// </summary>
public enum ENotificationChannel
{
    /// <summary>
    /// Envio por e-mail.
    /// </summary>
    Email = 1,
    /// <summary>
    /// Envio por push notification.
    /// </summary>
    Push = 2,
    /// <summary>
    /// Notificação dentro do aplicativo.
    /// </summary>
    InApp = 3,
    /// <summary>
    /// Notificação em tempo real.
    /// </summary>
    RealTime = 4
}
