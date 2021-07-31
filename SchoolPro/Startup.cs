using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using SchoolPro.Models;

[assembly: OwinStartupAttribute(typeof(SchoolPro.Startup))]
namespace SchoolPro
{
    public partial class Startup
    {
      private  ApplicationDbContext db = new ApplicationDbContext();

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            var rolemanegaer = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

        //    CreateRoles();
            //CreateUsers();

        }
        public void CreateUsers()
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            ApplicationUser user = new ApplicationUser();
            user.Email = "Ayaad210@gmail.com";
            user.UserName = "Ayaad210@gmail.com";
            user.Name = "MasterAdmin";
            var check = userManager.Create(user, "As123+");
            if (check.Succeeded)
            {
                userManager.AddToRoles(user.Id, "Admins");
            }



        }


        public void CreateRoles()
        {
            var rolemanegaer = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));

            if (!rolemanegaer.RoleExists("Admins"))
            {
                rolemanegaer.Create(new IdentityRole() { Name = "Admins" });
            }
            if (!rolemanegaer.RoleExists("Teachers"))
            {
                rolemanegaer.Create(new IdentityRole() { Name = "Teachers" });
            }
            if (!rolemanegaer.RoleExists("Perants"))
            {
                rolemanegaer.Create(new IdentityRole() { Name = "Perants" });
            }
            if (!rolemanegaer.RoleExists("Students"))
            {
                rolemanegaer.Create(new IdentityRole() { Name = "Students" });
            }



        }

    }
}
