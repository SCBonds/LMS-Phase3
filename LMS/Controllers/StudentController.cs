﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Student")]
  public class StudentController : CommonController
  {

    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Catalog()
    {
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


    public IActionResult ClassListings(string subject, string num)
    {
      System.Diagnostics.Debug.WriteLine(subject + num);
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }


    /*******Begin code to modify********/

    /// <summary>
    /// Returns a JSON array of the classes the given student is enrolled in.
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester
    /// "year" - The year part of the semester
    /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from e in db.Enrolled
                        join c in db.Classes
                        on e.ClassId equals c.ClassId
                        into cl
                        from classes in cl.DefaultIfEmpty()

                        join courses in db.Courses
                        on classes.CourseId equals courses.CourseId
                        into data
                        from all in data.DefaultIfEmpty()
                        where e.StudentId == uid
                        select new
                        {
                            subject = all.Department,
                            number = all.Number,
                            name = all.Name,
                            season = classes.SemesterSeason,
                            year = classes.SemesterYear,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }
    }

    /// <summary>
    /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The category name that the assignment belongs to
    /// "due" - The due Date/Time
    /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="uid"></param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            var query = from e in db.Enrolled
                           join c in db.Classes
                           on e.ClassId equals c.ClassId
                           into cl
                           from classes in cl.DefaultIfEmpty()

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

                           join s in db.Submission
                           on new { A = x.AssignmentId, B = uid } equals new { A = s.AssignmentId, B = s.StudentId }
                           into joined
                           from j in joined.DefaultIfEmpty()

                           where e.StudentId == uid
                           && all.Department == subject
                           && all.Number == num
                           && classes.SemesterSeason == season
                           && classes.SemesterYear == year

                            select new
                            {
                                aname = x.Name,
                                cname = categories.Name,
                                due = x.Due,
                                score = j.Score == null ? null : (uint?)j.Score
                            };

            return Json(query.ToArray());
        }
    }



    /// <summary>
    /// Adds a submission to the given assignment for the given student
    /// The submission should use the current time as its DateTime
    /// You can get the current time with DateTime.Now
    /// The score of the submission should start as 0 until a Professor grades it
    /// If a Student submits to an assignment again, it should replace the submission contents
    /// and the submission time (the score should remain the same).
	/// Does *not* automatically reject late submissions.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="uid">The student submitting the assignment</param>
    /// <param name="contents">The text contents of the student's submission</param>
    /// <returns>A JSON object containing {success = true/false}.</returns>
    public IActionResult SubmitAssignmentText(string subject, int num, string season, int year, 
      string category, string asgname, string uid, string contents)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            // query to return the assignmentID
            var getAssignment = from a in db.Assignments
                        join ac in db.AssignmentCategories
                        on a.CategoryId equals ac.CategoryId
                        into cat
                        from categories in cat.DefaultIfEmpty()

                        join c in db.Classes
                        on categories.ClassId equals c.ClassId
                        into cl
                        from all in cl.DefaultIfEmpty()

                        join co in db.Courses
                        on all.CourseId equals co.CourseId
                        into cour
                        from courses in cour.DefaultIfEmpty()

                        where all.SemesterSeason == season
                        && all.SemesterYear == year
                        && categories.Name == category
                        && a.Name == asgname
                        && courses.Number == num
                        && courses.Department == subject

                        select new
                        {
                            aID = a.AssignmentId
                        };

            var submission = from s in db.Submission
                                where s.AssignmentId == Int32.Parse(getAssignment.ToArray()[0].aID.ToString())
                                && s.StudentId == uid
                                select s;

            // If submission is already present
            if (submission.ToArray().Count() > 0)
            {
                submission.ToArray()[0].Contents = contents;
                submission.ToArray()[0].Time = DateTime.Now;

                db.SaveChanges();
            }
            // Else, create a *new* assignment submission
            else
            {
                Submission s = new Submission();
                s.AssignmentId = getAssignment.ToArray()[0].aID;
                s.StudentId = uid;
                s.Contents = contents;
                s.Score = 0;
                s.Time = DateTime.Now;

                db.Submission.Add(s);

                db.SaveChanges();
            }

            return Json(new { success = true });
        }
    }

    
    /// <summary>
    /// Enrolls a student in a class.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="uid">The uid of the student</param>
    /// <returns>A JSON object containing {success = {true/false},
	/// false if the student is already enrolled in the Class.</returns>
    public IActionResult Enroll(string subject, int num, string season, int year, string uid)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
                var e1 = from c in db.Classes
                               join cour in db.Courses
                               on c.CourseId equals cour.CourseId
                               into course
                               from courses in course.DefaultIfEmpty()

                               where c.SemesterSeason == season
                               && c.SemesterYear == year
                               && courses.Department == subject
                               && courses.Number == num
                               
                               select new
                               {
                                   classID = c.ClassId
                               };

                var e2 = from e in db.Enrolled
                         where e.StudentId == uid
                         && e.ClassId == (uint)e1.ToArray()[0].classID

                         select e;


                if (e2.ToArray().Count() > 0)
                {
                    return Json(new { success = false });
                }
                // Else, add the student to enrolled
                else
                {
                    Enrolled e = new Enrolled();
                    e.StudentId = uid;
                    e.ClassId = e1.ToArray()[0].classID;
                    e.Grade = "--";

                    db.Enrolled.Add(e);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
        }
    }



    /// <summary>
    /// Calculates a student's GPA
    /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
    /// Assume all classes are 4 credit hours.
    /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
    /// If a student does not have any grades, they have a GPA of 0.0.
    /// Otherwise, the point-value of a letter grade is determined by the table on this page:
    /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
    public IActionResult GetGPA(string uid)
    {
        using (Team14LMSContext db = new Team14LMSContext())
        {
            double GPA = 0;
            int nonNullCount = 0;
            // Query to get grade
            var query = from e in db.Enrolled
                        where e.StudentId == uid
                        select e.Grade;

            // For each grade returned by the query, check against case statements to determine numerical GPA
            foreach (var item in query.ToArray())
            {
                if (item != null)
                {
                    switch (item.ToString())
                    {
                        case "A":
                            GPA = GPA + 4.0;
                            break;
                        case "A-":
                            GPA = GPA + 3.7;
                            break;
                        case "B+":
                            GPA = GPA + 3.3;
                            break;
                        case "B":
                            GPA = GPA + 3.0;
                            break;
                        case "B-":
                            GPA = GPA + 2.7;
                            break;
                        case "C+":
                            GPA = GPA + 2.3;
                            break;
                        case "C":
                            GPA = GPA + 2.0;
                            break;
                        case "C-":
                            GPA = GPA + 1.7;
                            break;
                        case "D+":
                            GPA = GPA + 1.3;
                            break;
                        case "D":
                            GPA = GPA + 1.0;
                            break;
                        case "D-":
                            GPA = GPA + 0.7;
                            break;
                        case "E":
                            GPA = GPA + 0.0;
                            break;
                    }
                    if (item.ToString() != "" && item.ToString() != "--")
                        nonNullCount++;
                }
            }

            GPA = GPA / nonNullCount;


            return Json(new {gpa = GPA});
        }   
    }

    /*******End code to modify********/

  }
}