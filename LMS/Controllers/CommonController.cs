using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/
        protected Team14LMSContext db;

        public CommonController()
        {
            db = new Team14LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        public void UseLMSContext(Team14LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from d in db.Departments
                            select new
                            {
                                name = d.Name,
                                subject = d.Subject
                            };

                return Json(query.ToArray());
            }
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from d in db.Departments
                            select new
                            {
                                subject = d.Subject,
                                dname = d.Name,
                                // using nested query to return array of courses
                                courses = (from c in db.Courses where c.Department == d.Subject select new { number = c.Number, cname = c.Name })
                            };

                return Json(query.ToArray());
            }
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from cc in db.Classes
                            join p in db.Professors
                            on cc.ProfessorId equals p.UId
                            into clp
                            from classProf in clp.DefaultIfEmpty()

                            join cl in db.Courses
                            on cc.CourseId equals cl.CourseId
                            into coCl
                            from co in coCl.DefaultIfEmpty()


                            where co.Number == number
                            && co.Department == subject

                            select new
                            {
                                season = cc.SemesterSeason,
                                year = cc.SemesterYear,
                                location = cc.Location,
                                start = cc.StartTime,
                                end = cc.EndTime,
                                fname = classProf.FName, 
                                lname = classProf.LName
                            };

                return Json(query.ToArray());
            }
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            // Will need courses
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from classes in db.Classes
                            join courses in db.Courses
                            on classes.CourseId equals courses.CourseId
                            into data
                            from all in data.DefaultIfEmpty()

                            join ac in db.AssignmentCategories
                            on classes.ClassId equals ac.ClassId
                            into cat
                            from categories in cat.DefaultIfEmpty()

                            join a in db.Assignments
                            on categories.CategoryId equals a.CategoryId
                            into assignments
                            from x in assignments.DefaultIfEmpty()

                            where all.Department == subject
                            && all.Number == num
                            && classes.SemesterSeason == season
                            && classes.SemesterYear == year
                            && categories.Name == category
                            && x.Name == asgname

                            select new
                            {
                                content = x.Contents
                            };

                return Content(query.ToArray()[0].content.ToString());
            }
            
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
             using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from co in db.Courses
                            join cl in db.Classes
                            on co.CourseId equals cl.CourseId
                            into coCl

                            from j in coCl.DefaultIfEmpty()
                            join ac in db.AssignmentCategories
                            on j.ClassId equals ac.ClassId
                            into ccac

                            from j1 in ccac.DefaultIfEmpty()
                            join a in db.Assignments
                            on j1.CategoryId equals a.CategoryId
                            into acas

                            from j2 in acas.DefaultIfEmpty()
                            join s in db.Submission
                            on j2.AssignmentId equals s.AssignmentId
                            into all

                            from j3 in all.DefaultIfEmpty()
                            where co.Department == subject
                            && co.Number == num
                            && j.SemesterSeason == season 
                            && j.SemesterYear == year
                            && j1.Name == category
                            && j2.Name == asgname
                            && j3.StudentId == uid

                            select new
                            {
                                Submission = j3.Contents
                            };

                if (query.ToArray().Count() > 0)
                    return Content(query.ToArray()[0].ToString());
                else
                    return Content("");

            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                // Three seperate queries to determine what position the uid corresponds to

                var stud = from stu in db.Students
                           where stu.UId == uid
                           select new
                           {
                               fname = stu.FName,
                               lname = stu.LName,
                               uid = uid,
                               department = stu.Major
                           };

                var admin = from ad in db.Administrators
                           where ad.UId == uid
                           select new
                           {
                               fname = ad.FName,
                               lname = ad.LName,
                               uid = uid,
                           };

                var prof = from pro in db.Professors
                           where pro.UId == uid
                           select new
                           {
                               fname = pro.FName,
                               lname = pro.LName,
                               uid = uid,
                               department = pro.Department
                           };

                // If student return
                if (stud.ToArray().Count() > 0) 
                {
                    return Json(stud.ToArray()[0]);
                }
                // If admin return
                if (admin.ToArray().Count() > 0)
                {
                    return Json(admin.ToArray()[0]);
                }
                // If professor return
                if (prof.ToArray().Count() > 0)
                {
                    return Json(prof.ToArray()[0]);
                }
                // else, unsuccesful
                return Json(new { success = false });
            }
        }


        /*******End code to modify********/

    }
}