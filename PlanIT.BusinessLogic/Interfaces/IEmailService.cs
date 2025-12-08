// IEmailService.cs

namespace PlanIT.BusinessLogic.Interfaces
{
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
}
}
