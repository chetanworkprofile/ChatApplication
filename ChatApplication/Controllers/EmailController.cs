/*using ChatApplication.Models;
using ChatApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace ChatApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        Response response= new Response();
        [HttpPost]
        [Route("/api/v1/sendEmail")]
        public IActionResult SendEmail(string email)
        {
            try
            {
                //_logger.LogInformation("Register User method started");
                //Response response = authService.CreateUser(inpUser).Result;
                MailMessage message = new MailMessage();

                // set the sender and recipient email addresses
                message.From = new MailAddress("your-email@your-domain.com");
                message.To.Add(new MailAddress(email));

                // set the subject and body of the email
                message.Subject = "Verify your account";
                message.Body = "Please click on the following link to verify your account: http://your-domain.com/verify.aspx?code=" + 123;//verificationCode;

                // create a new SmtpClient object
                SmtpClient client = new SmtpClient();

                // set the SMTP server credentials and port
                client.Credentials = new NetworkCredential("chetan.gupta@chicmic.co.in", "Chicmic@2022");
                client.Host = "mail.chicmic.co.in";
                client.Port = 587;
                client.EnableSsl = true;

                // send the email
                client.Send(message);
                return Ok();
            }
            catch (Exception ex)
            {
                //_logger.LogError("Internal server error something wrong happened ", DateTime.Now);
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.Data = ex.Data;
                return StatusCode(500, response);
            }
        }
    }
}
*/