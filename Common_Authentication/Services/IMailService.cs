

using Mails_App;

namespace Common_Authentication.Services
{
    public interface IMailService
    {
        bool SendMail(MailData Mail_Data);
        //bool SendOTPMail(MailData Mail_Data);
        
    }

}
