﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Professor")]
  public class ProfessorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Students(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult Class(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult Categories(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      return View();
    }

    public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      ViewData["uid"] = uid;
      return View();
    }

    /*******Begin code to modify********/


    /// <summary>
    /// Returns a JSON array of all the students in a class.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "dob" - date of birth
    /// "grade" - the student's grade in this class
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from e in db.Enrolled
                        join c in db.Classes
                        on e.ClassId equals c.ClassId
                        into ec
                        from classes in ec.DefaultIfEmpty()

                        join cr in db.Courses
                        on classes.CourseId equals cr.CourseId
                        into cour
                        from course in cour.DefaultIfEmpty()

                        join s in db.Students
                        on e.StudentId equals s.UId
                        into all
                        from data in all.DefaultIfEmpty()

                        where course.Department == subject
                        && course.Number == num
                        && classes.SemesterSeason == season
                        && classes.SemesterYear == year

                        select new
                        {
                            fname = data.FName,
                            lname = data.LName,
                            uid = data.UId,
                            dob = data.Dob,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }
    }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from a in db.Assignments
                            join ac in db.AssignmentCategories
                            on a.CategoryId equals ac.CategoryId
                            into c
                            from cat in c.DefaultIfEmpty()

                            join cl in db.Classes
                            on cat.ClassId equals cl.ClassId
                            into cla
                            from classes in cla.DefaultIfEmpty()

                            join cr in db.Courses
                            on classes.CourseId equals cr.CourseId
                            into f
                            from final in f.DefaultIfEmpty()

                            where final.Department == subject
                            && final.Number == num
                            && classes.SemesterSeason == season
                            && classes.SemesterYear == year

                            select new
                            {
                                aname = a.Name,
                                cname = cat.Name,
                                due = a.Due,
                                submissions = (from s in db.Submission where s.AssignmentId == a.AssignmentId select s).Count()
                            };

                var query2 = from x in query
                             where x.cname == category
                             select new
                             {
                                 aname = x.aname,
                                 cname = x.cname,
                                 due = x.due,
                                 submissions = x.submissions
                             };

                if (category == null)
                    return Json(query.ToArray());
                else
                    return Json(query2.ToArray());

            }
        }



        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
    {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from cat in db.AssignmentCategories
                            join cl in db.Classes
                            on cat.ClassId equals cl.ClassId
                            into cla
                            from classes in cla.DefaultIfEmpty()

                            join cr in db.Courses
                            on classes.CourseId equals cr.CourseId
                            into f
                            from final in f.DefaultIfEmpty()

                            where final.Department == subject
                            && final.Number == num
                            && classes.SemesterSeason == season
                            && classes.SemesterYear == year

                            select new
                            {
                                name = cat.Name,
                                weight = cat.Weight
                            };

                return Json(query.ToArray());
            }
    }

    /// <summary>
    /// Creates a new assignment category for the specified class.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false},
    ///	false if an assignment category with the same name already exists in the same class.</returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
    {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var categoryPresent = from course in db.Courses
                                join cl in db.Classes
                                on course.CourseId equals cl.CourseId
                                into cocl

                                from cc in cocl
                                join ac in db.AssignmentCategories
                                on cc.ClassId equals ac.ClassId
                                into ccAssCat

                                from ccac in ccAssCat
                                where course.Department == subject
                                && course.Number == num
                                && cc.SemesterSeason == season
                                && cc.SemesterYear == year
                                && ccac.Name == category
                                select ccac;

                if (categoryPresent.Count() == 0) 
                {
                    var classID = from course in db.Courses
                                  join cl in db.Classes
                                  on course.CourseId equals cl.CourseId
                                  where course.Department == subject
                                  && course.Number == num
                                  && cl.SemesterSeason == season
                                  && cl.SemesterYear == year
                                  select cl.ClassId;


                    if (classID.Count() != 0)
                    {
                        AssignmentCategories assCat = new AssignmentCategories();
                        assCat.Name = category;
                        assCat.Weight = (uint)catweight;
                        assCat.ClassId = classID.ToArray()[0];

                        db.AssignmentCategories.Add(assCat);

                        db.SaveChanges();

                        return Json(new { success = true });
                    }

                    return Json(new { success = false });
                }

            }

            return Json(new { success = false });
    }

    /// <summary>
    /// Creates a new assignment for the given class and category.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="asgpoints">The max point value for the new assignment</param>
    /// <param name="asgdue">The due DateTime for the new assignment</param>
    /// <param name="asgcontents">The contents of the new assignment</param>
    /// <returns>A JSON object containing success = true/false,
	/// false if an assignment with the same name already exists in the same assignment category.</returns>
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var classWithCat = from cat in db.AssignmentCategories
                                   join cl in db.Classes
                                   on cat.ClassId equals cl.ClassId
                                   into cla
                                   from classes in cla.DefaultIfEmpty()

                                   join cr in db.Courses
                                   on classes.CourseId equals cr.CourseId
                                   into f
                                   from final in f.DefaultIfEmpty()

                                   where cat.Name == category
                                   && final.Department == subject
                                   && final.Number == num
                                   && classes.SemesterSeason == season
                                   && classes.SemesterYear == year
                                   select new
                                   {
                                       classID = classes.ClassId,
                                       catID = cat.CategoryId
                                   };

                var isAssignment = from c in classWithCat
                                   join a in db.Assignments
                                   on c.catID equals a.CategoryId
                                   into ca
                                   from assign in ca.DefaultIfEmpty()

                                   where assign.Name == asgname

                                   select assign;

                if (isAssignment.ToArray().Count() > 0)
                {
                    return Json(new { success = false });
                }
                else
                {
                    Assignments a = new Assignments();
                    a.CategoryId = (uint)classWithCat.ToArray()[0].catID;
                    a.Name = asgname;
                    a.Points = (uint)asgpoints;
                    a.Contents = asgcontents;
                    a.Due = asgdue;

                    db.Assignments.Add(a);

                    db.SaveChanges();


                    var updateGrades = from e in db.Enrolled
                                       where e.ClassId == classWithCat.ToArray()[0].classID
                                       select e;

                    foreach (var student in updateGrades.ToArray())
                    {
                        UpdateClassGrade(student.StudentId, classWithCat.ToArray()[0].classID);
                    }

                    return Json(new { success = true });
                }

            }
    }


    /// <summary>
    /// Gets a JSON array of all the submissions to a certain assignment.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "time" - DateTime of the submission
    /// "score" - The score given to the submission
    /// 
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
    {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from s in db.Submission
                            join st in db.Students
                            on s.StudentId equals st.UId
                            into data
                            from all in data.DefaultIfEmpty()

                            join a in db.Assignments
                            on s.AssignmentId equals a.AssignmentId
                            into sa
                            from assignment in sa.DefaultIfEmpty()

                            join ac in db.AssignmentCategories
                            on assignment.CategoryId equals ac.CategoryId
                            into c
                            from cat in c.DefaultIfEmpty()

                            join cl in db.Classes
                            on cat.ClassId equals cl.ClassId
                            into cla
                            from classes in cla.DefaultIfEmpty()

                            join cr in db.Courses
                            on classes.CourseId equals cr.CourseId
                            into f
                            from final in f.DefaultIfEmpty()

                            where final.Department == subject
                            && final.Number == num
                            && classes.SemesterSeason == season
                            && classes.SemesterYear == year
                            && cat.Name == category
                            && assignment.Name == asgname

                            select new
                            {
                                fname = all.FName,
                                lname = all.LName,
                                uid = all.UId,
                                time = s.Time,
                                score = s.Score
                            };

                return Json(query.ToArray());
            }
    }


    /// <summary>
    /// Set the score of an assignment submission
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <param name="uid">The uid of the student who's submission is being graded</param>
    /// <param name="score">The new score for the submission</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from s in db.Submission
                        join st in db.Students
                        on s.StudentId equals st.UId
                        into data
                        from all in data.DefaultIfEmpty()

                        join a in db.Assignments
                        on s.AssignmentId equals a.AssignmentId
                        into sa
                        from assignment in sa.DefaultIfEmpty()

                        join ac in db.AssignmentCategories
                        on assignment.CategoryId equals ac.CategoryId
                        into c
                        from cat in c.DefaultIfEmpty()

                        join cl in db.Classes
                        on cat.ClassId equals cl.ClassId
                        into cla
                        from classes in cla.DefaultIfEmpty()

                        join cr in db.Courses
                        on classes.CourseId equals cr.CourseId
                        into f
                        from final in f.DefaultIfEmpty()

                        where final.Department == subject
                        && final.Number == num
                        && classes.SemesterSeason == season
                        && classes.SemesterYear == year
                        && cat.Name == category
                        && assignment.Name == asgname
                        && all.UId == uid

                        select new
                        {
                            classID = classes.ClassId,
                            s
                        };

            if (query.ToArray().Count() > 0)
            {
                query.ToArray()[0].s.Score = (uint)score;
                db.SaveChanges();

                UpdateClassGrade(uid, query.ToArray()[0].classID);

                return Json(new { success = true });
            }
            else
                return Json(new { success = false });

        }
    }


    /// <summary>
    /// Returns a JSON array of the classes taught by the specified professor
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester in which the class is taught
    /// "year" - The year part of the semester in which the class is taught
    /// </summary>
    /// <param name="uid">The professor's uid</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var query = from cl in db.Classes
                            join c in db.Courses
                            on cl.CourseId equals c.CourseId
                            into data
                            from all in data.DefaultIfEmpty()

                            where cl.ProfessorId == uid

                            select new
                            {
                                subject = all.Department,
                                number = all.Number,
                                name = all.Name,
                                season = cl.SemesterSeason,
                                year = cl.SemesterYear
                            };

                return Json(query.ToArray());
            }
        }


        /*******End code to modify********/

        public void UpdateClassGrade(string uid, uint ClassID)
        {
            using (Team14LMSContext db = new Team14LMSContext())
            {
                var selectedClass = from c in db.Classes
                                    where c.ClassId == ClassID
                                    select new { catCount = c.AssignmentCategories.Count() };


                var getNonEmptyCategories = from ac in db.AssignmentCategories
                                            where ac.ClassId == ClassID
                                            && ac.Assignments.Count() > 0

                                            select new
                                            {
                                                catId = ac.CategoryId,
                                                catWeight = ac.Weight,
                                                assignments = ac.Assignments.ToArray()
                                            };
                double totalCatWeight = 0;
                double totalWeightedVal = 0;
                for (int i = 0; i < selectedClass.ToArray()[0].catCount; i++)
                {

                    double totalPoints = 0;
                    double totalScore = 0;
                    foreach (var x in getNonEmptyCategories.ToArray()[i].assignments)
                    {
                        totalPoints = totalPoints + x.Points;

                        var getSubmissions = from s in db.Submission
                                             where x.AssignmentId == s.AssignmentId
                                             select new { score = s.Score };


                        for (int r = 0; r < getSubmissions.ToArray().Count(); r++)
                        {
                            totalScore = totalScore + getSubmissions.ToArray()[r].score;
                        }
                    }

                    if (getNonEmptyCategories.ToArray()[i].assignments.Count() > 0)
                    {
                        double p = totalScore / totalPoints;

                        double weighted = p * getNonEmptyCategories.ToArray()[i].catWeight;

                        totalWeightedVal = totalWeightedVal + weighted;

                        totalCatWeight = totalCatWeight + getNonEmptyCategories.ToArray()[i].catWeight;
                    }

                }

                double scalingFactor = 100 / totalCatWeight;

                double percent = (scalingFactor * totalWeightedVal) / 100;

                string letterGrade = "";

                if (percent >= .93)
                {
                    letterGrade = "A";
                }
                else if (percent >= .90)
                {
                    letterGrade = "A-";
                }
                else if (percent >= .87)
                {
                    letterGrade = "B+";
                }
                else if (percent >= .83)
                {
                    letterGrade = "B";
                }
                else if (percent >= .80)
                {
                    letterGrade = "B-";
                }
                else if (percent >= .77)
                {
                    letterGrade = "C+";
                }
                else if (percent >= .73)
                {
                    letterGrade = "C";
                }
                else if (percent >= .70)
                {
                    letterGrade = "C-";
                }
                else if (percent >= .67)
                {
                    letterGrade = "D+";
                }
                else if (percent >= .63)
                {
                    letterGrade = "D";
                }
                else if (percent >= .60)
                {
                    letterGrade = "D-";
                }
                else
                {
                    letterGrade = "E";
                }

                var getEnrolled = from e in db.Enrolled
                                  where e.StudentId == uid
                                  && e.ClassId == ClassID
                                  select e;

                getEnrolled.ToArray()[0].Grade = letterGrade;
                db.SaveChanges();
            }
        }
    }
}