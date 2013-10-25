using Ais.Internal.Dcm.Web.Models;
using Ais.Internal.Dcm.Web.Service;
using Microsoft.Samples.ServiceHosting.AspProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace Ais.Internal.Dcm.Web.Controllers
{
    public class AccountController : ApiController
    {
        ILoggerService _loggerService = null;

       
        public AccountController(ILoggerService loggerService)
        {
            this._loggerService = loggerService;
        }

        [HttpGet]
        public string Login()
        {
            return "User logged in.";
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<bool> Login(LoginModel model)
        {
            bool status = false;
            try
            {
                if (this.ModelState.IsValid)
                {
                    bool userValid = Membership.ValidateUser(model.UserName, model.Password);
                    if (userValid)
                    {
                        FormsAuthentication.SetAuthCookie(model.UserName, false/*, model.RememberMe*/);

                        var ticketEncryptString = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(0, model.UserName, DateTime.Now,
                                                                                  DateTime.Now.AddSeconds(30), false,
                                                                                  model.UserName));
                        HttpContext.Current.Response.Cookies.Add(new HttpCookie("userNameTicket", ticketEncryptString));
                        status = true;
                    }
                }
            }
            catch (Exception exp)
            {
                _loggerService.LogException("AccountController.LoginPost", exp);
            }
            return status;
        }

        [ActionName("ChangePassword")]
        [HttpPost]
        public object ChangePassword(ChangePasswordModel changePasswordModel)
        {
            if (this.ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    string existingPassword = string.Empty;
                    var user = Membership.GetUser(User.Identity.Name);
                    if (user != null)
                    {
                        existingPassword = user.GetPassword();
                    }
                    else
                    {
                        return new { changeSuccess = false, message = "You should be logged in to change password." };
                    }
                    //check oldpassword with database pass
                    if (!string.IsNullOrWhiteSpace(existingPassword) && string.CompareOrdinal(existingPassword, changePasswordModel.OldPassword) != 0)
                    {
                        return new { changeSuccess = false, message = "Old password provided is wrong." };
                    }  //check oldpassword with newpassword
                    if (String.CompareOrdinal(changePasswordModel.OldPassword, changePasswordModel.NewPassword) == 0)
                    {
                        return new { changeSuccess = false, message = "Old password and new password should not match" };
                    }//check new pass and confirmation pass
                    //if (string.CompareOrdinal(changePasswordModel.NewPassword, changePasswordModel.ConfirmPassword) != 0)
                    //{
                    //    return new { changeSuccess = false, message = "New password and Confirm password should match" };
                    //}

                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, userIsOnline: true);
                    changePasswordSucceeded = currentUser.ChangePassword(changePasswordModel.OldPassword, changePasswordModel.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return new { changeSuccess = changePasswordSucceeded, message = "" };
                }
                else
                {
                    return new { changeSuccess = false, message = "The current password is incorrect or the new password is invalid." };
                    //this.ModelState.AddModelError(string.Empty, "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            var errorMessage = "There is some problem at server.";
            if (this.ModelState.Values.Count > 0)
            {
                var errors = this.ModelState.Values.ToList()[0].Errors;
                if (errors.Count > 0)
                    errorMessage = errors[0].ErrorMessage;
            }
            return new { changeSuccess = false, message = errorMessage };
        }

        [ActionName("AuthorizeUser")]
        [HttpGet]
        public object AuthorizeUser()
        {
            try
            {
                var userNameTicket = HttpContext.Current.Request.Cookies["userNameTicket"].Value;
                var ticket = FormsAuthentication.Decrypt(userNameTicket);
                var userName = ticket.Name;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    if (!Roles.GetRolesForUser(userName).Contains(Constants.ADMIN_ROLE))
                    {
                        return
                            new { user = userName, authorized = false, message = "You are not allowed to create user." };
                    }
                    return new { user = userName, authorized = true, message = string.Empty };
                }
                return new { user = "Invalid", authorized = false, message = "You should be logged in to view this page" };
            }
            catch (Exception exp)
            {
                this._loggerService.LogException("AccountController.AuthorizeGet", exp);
                return new { user = "Invalid", authorized = false, message = "You should be logged in to view this page" };
            }
        }

        [ActionName("CheckUserLogIn")]
        [HttpGet]
        public object CheckForUserLoggedIn()
        {
            try
            {
                var userNameTicket = HttpContext.Current.Request.Cookies["userNameTicket"].Value;
                var ticket = FormsAuthentication.Decrypt(userNameTicket);
                var userName = ticket.Name;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    return new { LoginStatus = true,user=userName, message = string.Empty };
                }
                return new { LoginStatus = false, message = "You should be logged in to view this page" };
            }
            catch (Exception exp)
            {
                this._loggerService.LogException("AccountController.CheckForUserLoggedInGet", exp);
                return new { LoginStatus = false, user = string.Empty, message = "You should be logged in to view this page" };
            }
        }

        [ActionName("CreateUser")]
        [HttpPost]
        public object CreateUser(RegisterModel registerModel)
        {    
            object returnObj = null;
            var rdr = new System.Configuration.AppSettingsReader();
            string companyName = (string)rdr.GetValue("customer_append", typeof(string));
            try
            {
                var errorMessage = "There is some problem at server.";
                if (this.ModelState.Values.Count > 0)
                {
                    var errors = this.ModelState.Values.ToList()[0].Errors;
                    if (errors.Count > 0)
                        errorMessage = errors[0].ErrorMessage;
                    returnObj = new { userCreation = false, message = errorMessage };
                    return returnObj;
                }

                MembershipUserCollection collection = Membership.FindUsersByName(registerModel.UserName);
                if (collection != null && collection.Count > 0)
                    return returnObj = new { userCreation = false, message = "User with this username already exists." };
                var user = Membership.CreateUser( string.Format("{0}_{1}",companyName, registerModel.UserName), registerModel.Password);

                returnObj = new { userCreation = true, message = "User created successfully." };

            }
            catch (Exception exp)
            {
                  this._loggerService.LogException("AccountController.CreateUserPost", exp);
            }
            return returnObj;
        }

        [ActionName("Logout")]
        [HttpPost]
        public bool Logout()
        {
            try
            {
                //HttpContext.Current.Response.Cookies.Clear();
                FormsAuthentication.SignOut();
                HttpCookie cookie = HttpContext.Current.Response.Cookies["userNameTicket"];
                // If found, let the cookie expire
                if (cookie != null)
                    cookie.Expires = DateTime.Now.AddYears(-1);
                return true;
            }
            catch (Exception exp)
            {
                this._loggerService.LogException("AccountController.Logout", exp);
                return false;
            }
        }

        [HttpGet]
        [Authorize(Roles=Constants.ADMIN_ROLE)]
        [ActionName("GetAllUsers")]
        public HttpResponseMessage GetAllUsers(int pageSize=10,int pageNumber=1)
        {
            HttpResponseMessage message = null;
            try
            {
                var rdr = new System.Configuration.AppSettingsReader();
                string companyName = (string)rdr.GetValue("customer_append", typeof(string));
                int totalUsers = 0;
                --pageNumber; // Membership uses 0 based index
                List<UserModel> users = new List<UserModel>();
               // var collection = Membership.GetAllUsers(pageNumber, pageSize, out totalUsers);
                var collection = Membership.FindUsersByName(companyName+"%",pageNumber, pageSize, out totalUsers);
                var userNameTicket = HttpContext.Current.Request.Cookies["userNameTicket"].Value;
                var ticket = FormsAuthentication.Decrypt(userNameTicket);
                var currentUser = ticket.Name;
              
                //  foreach (MembershipUser item in collection.Cast<MembershipUser>().Where(user=> user.UserName.StartsWith(companyName+ "_") ))
                
                foreach (MembershipUser item in collection)
                {
                    var user = new UserModel
                    {
                        Username = item.UserName,
                        IsActive = true,
                        IsAdmin = Roles.IsUserInRole(item.UserName, Constants.ADMIN_ROLE)
                    };

                    if (item.UserName == currentUser)
                    {
                        user.EditNotAllowed = true;
                    }
                    users.Add(user);

                }
                message = Request.CreateResponse(HttpStatusCode.OK, new GetUsersResponse { Users = users, PageNumber = ++pageNumber, PageSize = pageSize, TotalUsers = totalUsers });

            }
            catch (Exception)
            {
                
                throw;
            }
            return message;
        }

        [HttpGet]
        [Authorize]
        [ActionName("ChangeRole")]
        public bool ChangeRole(string userName, bool makeAdmin)
        {
            bool isSuccess = false;
            try
            {
                if (makeAdmin)
                {
                    Roles.AddUserToRole(userName, Constants.ADMIN_ROLE);
                }
                else
                {
                    Roles.RemoveUserFromRole(userName, Constants.ADMIN_ROLE);
                }
                isSuccess = true;
            }
            catch (Exception exp)
            {
                this._loggerService.LogException(string.Format("AccountController.ChangeRole UserName:{0} ",userName), exp);
            }
            return true;
        }

        [HttpGet]
        [Authorize(Roles = Constants.ADMIN_ROLE)]
        [ActionName("DeleteUser")]
        public bool DeleteUser(string userName)
        {
            bool isSuccess = false;
            try
            {
                isSuccess = Membership.DeleteUser(userName);
                
            }
            catch (Exception exp)
            {
                this._loggerService.LogException(string.Format("AccountController.ChangeRole UserName:{0} ", userName), exp);

            }
            return isSuccess;
        }

        private bool ValidateUser(string userName, string password)
        {
            return Membership.ValidateUser(userName, password);
        }
    }



    
}
