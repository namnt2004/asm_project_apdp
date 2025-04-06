﻿using ASM_SIMS.DB;
using ASM_SIMS.Helpers;
using ASM_SIMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ASM_SIMS.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SimsDataContext _dbContext;

        public CoursesController(SimsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

// Display course list with CategoryName
       public IActionResult Index() { 
     
            CourseViewModel courseModel = new CourseViewModel
            {
                courseList = _dbContext.Courses
                    .Where(c => c.DeletedAt == null)
                    .Join(
                        _dbContext.Categories,
                        course => course.CategoryId,
                        category => category.Id,
                        (course, category) => new CourseDetail
                        {
                            Id = course.Id,
                            NameCourse = course.NameCourse,
                            Description = course.Description,
                            CategoryId = course.CategoryId,
                            CategoryName = category.NameCategory,
                            StartDate = course.StartDate,
                            EndDate = course.EndDate, //// No need to assign default because Required
                            Vote = course.Vote, // No need to assign default because Required
                            Status = course.Status,
                            CreatedAt = course.CreatedAt,
                            UpdatedAt = course.UpdatedAt
                        })
                    .ToList()
            };
            ViewData["title"] = "Courses";
            return View(courseModel);
        }

        // Display the form to create a new course
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _dbContext.Categories.ToList();
            return View(new CourseDetail
            {
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now),
                Vote = 0
            });
        }
        // Process to create new course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseDetail model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get CategoryId from CategoryName
                    var category = _dbContext.Categories
                        .FirstOrDefault(c => c.NameCategory == model.CategoryName);
                    if (category == null)
                    {
                        ModelState.AddModelError("CategoryName", "Danh mục không tồn tại.");
                        ViewBag.Categories = _dbContext.Categories.ToList();
                        return View(model);
                    }

                    var course = new Courses
                    {
                        NameCourse = model.NameCourse,
                        Description = model.Description,
                        CategoryId = category.Id, // Assign CategoryId from found category
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Vote = model.Vote,
                        Status = model.Status,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null
                    };
                    _dbContext.Courses.Add(course);
                    await _dbContext.SaveChangesAsync();
                    TempData["save"] = true;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    TempData["error"] = $"Failed to create course: {ex.Message}";
                }
            }
            ViewBag.Categories = _dbContext.Categories.ToList();
            return View(model);
        }

        // Display the form to edit a course
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var course = _dbContext.Courses
                .Include(c => c.Category)
                .FirstOrDefault(c => c.Id == id && c.DeletedAt == null);

            if (course == null) return NotFound();

            var model = new CourseDetail
            {
                Id = course.Id,
                NameCourse = course.NameCourse,
                Description = course.Description,
                CategoryId = course.CategoryId,
                CategoryName = course.Category?.NameCategory,
                StartDate = course.StartDate,
                EndDate = course.EndDate, 
                Vote = course.Vote, 
                Status = course.Status,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt
            };
            ViewBag.Categories = _dbContext.Categories.ToList();
            return View(model);
        }

        // Process to edit a course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseDetail model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var course = _dbContext.Courses
                        .FirstOrDefault(c => c.Id == model.Id && c.DeletedAt == null);

                    if (course == null) return NotFound();

                    // Get CategoryId from CategoryName
                    var category = _dbContext.Categories
                        .FirstOrDefault(c => c.NameCategory == model.CategoryName);
                    if (category == null)
                    {
                        ModelState.AddModelError("CategoryName", "Danh mục không tồn tại.");
                        ViewBag.Categories = _dbContext.Categories.ToList();
                        return View(model);
                    }

                    course.NameCourse = model.NameCourse;
                    course.Description = model.Description;
                    course.CategoryId = category.Id; // Assign CategoryId from found category
                    course.StartDate = model.StartDate;
                    course.EndDate = model.EndDate;
                    course.Vote = model.Vote;
                    course.Status = model.Status;
                    course.UpdatedAt = DateTime.Now;

                    _dbContext.Courses.Update(course);
                    await _dbContext.SaveChangesAsync();
                    TempData["save"] = true;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["save"] = false;
                    TempData["error"] = $"Failed to update course: {ex.Message}";
                }
            }
            ViewBag.Categories = _dbContext.Categories.ToList();
            return View(model);
        }

        // Process to delete a course
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var course = _dbContext.Courses
                    .FirstOrDefault(c => c.Id == id && c.DeletedAt == null);

                if (course == null) return NotFound();

                course.DeletedAt = DateTime.Now;
                _dbContext.Courses.Remove(course);

                await _dbContext.SaveChangesAsync();
                TempData["save"] = true;
            }
            catch (Exception ex)
            {
                TempData["save"] = false;
                TempData["error"] = $"Failed to delete course: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}

/*
 SOLID:

SRP: Mỗi controller chỉ xử lý một loại thực thể (Student, Teacher, v.v.).
DIP: Tiêm SimsDataContext qua constructor để giảm phụ thuộc trực tiếp.
OCP (Open/Closed Principle): Có thể mở rộng bằng cách thêm action mới mà không sửa code cũ.

Clean Code:
Sử dụng nameof để tránh lỗi chính tả trong redirect.
Tên biến và phương thức rõ ràng, phản ánh mục đích.
Xử lý lỗi bằng try-catch để đảm bảo robust.
 */
