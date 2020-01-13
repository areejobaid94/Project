using MyProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MyProject.Controllers
{
    public class UserServiceController : Controller
    {
        // GET: UserService
        ProjectDBEntities db = new ProjectDBEntities();
        Dictionary<int, string> InterstedValue = new Dictionary<int, string>()
        {
            { 1 , "Very Intersted" }, { 2 , "Intersted" },
            { 3 , "Not Intersted" }
        };

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetAllService()
        {
            List<ServiceModel> ServiceModelList = db.Services.Select(x => new ServiceModel()
            {
                ServiceID = x.ServiceID,
                ServiceName = x.ServiceName
            }).ToList();
            return Json(ServiceModelList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveUser(UserModel model)
        {             
            try
            {
                if (model.UserID > 0)
                {
                    User user = db.Users.SingleOrDefault(u => u.UserID == model.UserID);
                    user.Name = model.Name;
                    user.Mobile = model.Mobile;
                    user.Email = model.Email;
                    db.SaveChanges();
                }
                else
                {
                    User user = new User();
                    user.Name = model.Name;
                    user.Mobile = model.Mobile;
                    user.Email = model.Email;
                    db.Users.Add(user);
                    db.SaveChanges();
                    model.UserID = user.UserID;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveUserService(UserServiceModel model)
        {
            try
            {
                var userService = db.UsersServices.Where(x => x.ServiceID == model.ServiceID && x.UserID == model.UserID).FirstOrDefault();
                
                if (userService != null)
                {
                    userService.InterestedValue = model.InterestedValue;
                    userService.UserID = model.UserID;
                    userService.ServiceID = model.ServiceID;
                    db.SaveChanges();
                }
                else
                {
                    userService = new UsersService();
                    userService.InterestedValue = model.InterestedValue;
                    userService.UserID = model.UserID;
                    userService.ServiceID = model.ServiceID;
                    db.UsersServices.Add(userService);
                    db.SaveChanges();
                    model.UsersServicesID = userService.UsersServicesID;
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonResult SendEmailToUser(UserModel model )
        {
            string htmlString = "";
            if (model != null)
            {
                htmlString = "<p> Hi " + model.Name + " Thank you for your Submission,</p></br><p> Your Email: "
                    + model.Email + ",</p></br><p> Your Phone Namber: " + model.Mobile + "</br></p>";

            }
            var count = 0;
            if (model?.UsersServices.Count > 0)
            {
                foreach (var UserService in model.UsersServices)
                {
                    count++;
                    var SericesName = db.Services.Where(x => x.ServiceID == UserService.ServiceID).FirstOrDefault().ServiceName;
                    htmlString += "<p>You Have Selected: " + count.ToString() + "- " + SericesName + " and you are " + InterstedValue[Convert.ToInt32(UserService.InterestedValue)] + " on it</pr><br/>";

                }
            }
            var result = true;
            result = SendEmail( model.Email, "You Have Submitted Successfully", htmlString);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public bool SendEmail(string toEmail, string subject, string emailBody)
        {
            try
            {
                string SenderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"].ToString();
                string SenderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"].ToString();
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 100000;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(SenderEmail, SenderPassword);

                MailMessage mailMessage = new MailMessage(SenderEmail, toEmail, subject, emailBody);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            } 
        }
        public JsonResult GetUserData(int UserId)
        {
            User user = db.Users.Include("UsersServices").Where(x => x.UserID == UserId).FirstOrDefault();
            string value = string.Empty;
            var model = new UserModel();
            if (user == null)
                return null;
            model.UserID = user.UserID;
            model.Email = user.Email;
            model.Name = user.Name;
            model.Mobile = user.Mobile;
            model.UsersServices = user.UsersServices;

            SendEmailToUser(model);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}