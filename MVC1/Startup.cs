using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using BlogMVC.Models;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlogMVC.Startup))]
namespace BlogMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();
        }

        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


              
            if (!roleManager.RoleExists("Admin"))
            {                 
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);
                            
                var user = new ApplicationUser();
                user.UserName = "admin@myemail.com";
                user.Email = "admin@myemail.com";
                string userPWD = "123456";

                var chkUser = UserManager.Create(user, userPWD);
                
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }          
        }
    }
}
