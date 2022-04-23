using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Administrator")]
  public class AdministratorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Department(string subject)
    {
      ViewData["subject"] = subject;
      return View();
    }

    public IActionResult Course(string subject, string num)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }

    /*******Begin code to modify********/

    /// <summary>
    /// Returns a JSON array of all the courses in the given department.
    /// Each object in the array should have the following fields:
    /// "number" - The course number (as in 5530)
    /// "name" - The course name (as in "Database Systems")
    /// </summary>
    /// <param name="subject">The department subject abbreviation (as in "CS")</param>
    /// <returns>The JSON result</returns>
    public IActionResult GetCourses(string subject)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from c in db.Courses
                        where c.Department == subject
                        select new
                        {
                            number = c.Number,
                            name = c.Name
                        };

                return Json(query.ToArray());
        }
    }



    /// <summary>
    /// Returns a JSON array of all the professors working in a given department.
    /// Each object in the array should have the following fields:
    /// "lname" - The professor's last name
    /// "fname" - The professor's first name
    /// "uid" - The professor's uid
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <returns>The JSON result</returns>
    public IActionResult GetProfessors(string subject)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from p in db.Professors
                        select new
                        {
                            lname = p.LName,
                            fname = p.FName,
                            uid = p.UId
                        };

            return Json(query.ToArray());
        }
    }



    /// <summary>
    /// Creates a course.
    /// A course is uniquely identified by its number + the subject to which it belongs
    /// </summary>
    /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
    /// <param name="number">The course number</param>
    /// <param name="name">The course name</param>
    /// <returns>A JSON object containing {success = true/false},
	/// false if the Course already exists.</returns>
    public IActionResult CreateCourse(string subject, int number, string name)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from c in db.Courses
                        where c.Number == number
                        && c.Name == name
                        && c.Department == subject
                        select c;
            
            if(query.Count() < 1)
            {
                Courses course = new Courses();
                course.Name = name;
                course.Number = (uint)number;
                course.Department = subject;

                db.Courses.Add(course);

                db.SaveChanges();
                return Json(new { success = true });
            }
            else { return Json(new { success = false }); }

        }
    }



    /// <summary>
    /// Creates a class offering of a given course.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="number">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="start">The start time</param>
    /// <param name="end">The end time</param>
    /// <param name="location">The location</param>
    /// <param name="instructor">The uid of the professor</param>
    /// <returns>A JSON object containing {success = true/false}. 
    /// false if another class occupies the same location during any time 
    /// within the start-end range in the same semester, or if there is already
    /// a Class offering of the same Course in the same Semester.</returns>
    public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            // Query that determines if class information will overlap with any other class times
            var ifSameTime = from c in db.Classes
                                where c.Location == location
                                && c.SemesterSeason == season
                                && c.SemesterYear == year
                                && ((start.TimeOfDay >= c.StartTime && start.TimeOfDay <= c.EndTime)
                                || (end.TimeOfDay >= c.StartTime && end.TimeOfDay <= c.EndTime)
                                || (c.StartTime >= start.TimeOfDay && c.StartTime <= end.TimeOfDay)
                                || (c.EndTime >= start.TimeOfDay && c.EndTime <= end.TimeOfDay))
                                select c;

            // Query that determines if class information will create a duplicate listing
            var sameOffering = from c in db.Classes
                            join courses in db.Courses
                            on c.CourseId equals courses.CourseId
                            into data
                            from all in data.DefaultIfEmpty()

                            where all.Number == number
                            && all.Department == subject
                            && c.SemesterSeason == season
                            && c.SemesterYear == year
                            select c;

            // Gets the courseID for insert
            var courseID = from course in db.Courses
                            where course.Department == subject
                            && course.Number == number
                            select course.CourseId;

            if (ifSameTime.Count() >= 1)
            {
                return Json(new { success = false });
            }
            else if (sameOffering.Count() >= 1)
            {
                return Json(new { success = false });
            }
            else 
            { 
                Classes cl = new Classes();
                cl.ProfessorId = instructor;
                cl.SemesterSeason = season;
                cl.SemesterYear = (uint)year;
                cl.Location = location;
                cl.StartTime = start.TimeOfDay;
                cl.EndTime = end.TimeOfDay;
                cl.CourseId = courseID.ToArray()[0];

                db.Classes.Add(cl);

                db.SaveChanges();
                return Json(new { success = true });

            }
        }
    }


    /*******End code to modify********/

  }
}