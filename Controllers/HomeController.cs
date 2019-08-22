using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        //Note that MyContext must match the name of your context file
        private MyContext dbContext;
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("add")]
        public IActionResult Add(User user)
        {
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    return RedirectToAction("Dashboard");

                }
            }

            return View("Index");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("loginU")]
        public IActionResult LoginU(Login l_user)
        {

            if (ModelState.IsValid)
            {
                User currentUser = dbContext.Users.FirstOrDefault(s => s.Email == l_user.LEmail);
                if (currentUser == null)
                {
                    ModelState.AddModelError("LEmail", "Invalid Email/Password");
                    return View("Index");
                }

                var hasher = new PasswordHasher<Login>();
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(l_user, currentUser.Password, l_user.LPassword);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                HttpContext.Session.SetInt32("UserId", currentUser.UserId);
                return RedirectToAction("Dashboard");


            }
            return View("Index");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {


            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index");
            }

            var loggedUser = dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
            var allweddings = dbContext.Weddings.Include(w => w.WeddingGuest).ThenInclude(u => u.Planner).ToList();
            foreach (var i in allweddings)
            {
                if ((DateTime.Compare(DateTime.Today, i.WedDate) > 0))
                {
                    dbContext.Weddings.Remove(i);
                    dbContext.SaveChanges();
                }
            }
            var newweddings = dbContext.Weddings.Include(w => w.WeddingGuest).ThenInclude(u => u.Planner).ToList();
            ViewBag.userName = loggedUser.FirstName + loggedUser.LastName;
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");




            return View(newweddings);
        }


        [HttpGet("newWedding")]
        public IActionResult NewWedding()
        {

            return View();
        }

        [HttpPost("addWedding")]
        public IActionResult AddWedding(Wedding wedding)
        {

            if (ModelState.IsValid)
            {

                int result = DateTime.Compare(DateTime.Today, wedding.WedDate);
                if (result < 1)
                {
                    var newWed = wedding;
                    newWed.UserId = (int)HttpContext.Session.GetInt32("UserId");
                    dbContext.Weddings.Add(newWed);
                    dbContext.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("WedDate", "Invalid Wedding Date.  Wedding Date must be past today's date.");
                    return View("NewWedding");

                }
            }
            return View("NewWedding");
        }

        [HttpGet("showInfo/{wedID}")]
        public IActionResult ShowInfo(int wedID)
        {
            var wedding = dbContext.Weddings.Include(w => w.WeddingGuest).ThenInclude(u => u.Planner).FirstOrDefault(w => w.WeddingId == wedID);
            return View(wedding);
        }

        [HttpGet("rsvp/{wedID}")]
        public IActionResult RSVP(int wedID)
        {
            WedUser weduser = new WedUser();
            weduser.UserId = (int)HttpContext.Session.GetInt32("UserId");
            weduser.WeddingId = wedID;
            dbContext.WedUsers.Add(weduser);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("unrsvp/{wedID}")]
        public IActionResult UNSRVP(int wedID)
        {
            WedUser toDelete = dbContext.WedUsers.FirstOrDefault(u => u.WeddingId == wedID);
            if (toDelete == null)
                return RedirectToAction("Dashboard");

            dbContext.WedUsers.Remove(toDelete);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [HttpGet("delete/{wedID}")]
        public IActionResult Delete(int wedID)
        {
            Wedding toDelete = dbContext.Weddings.FirstOrDefault(u => u.WeddingId == wedID);
            if (toDelete == null)
                return RedirectToAction("Dashboard");

            dbContext.Weddings.Remove(toDelete);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }


        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
