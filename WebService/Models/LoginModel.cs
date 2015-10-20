using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Data.DataContext;

namespace WebService.Models
{
    public class LoginModel
        : IValidatableObject
    {
        public AuthenticationRole LoginType { get; set; }

        public StudentLoginModel Student { get; set; }
        public TeacherLoginModel Teacher { get; set; }

        private System.Text.RegularExpressions.Regex EmailMatcher = new System.Text.RegularExpressions.Regex(@"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$");

        public LoginModel()
        {
            Student = new StudentLoginModel();
            Teacher = new TeacherLoginModel();
            LoginType = AuthenticationRole.Teacher;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> Result = new List<ValidationResult>();

            if (LoginType == AuthenticationRole.Student)
            {
                if (Student.Password == null)
                    Result.Add(new ValidationResult("You must enter a password.", new string[] { "Student.Password" }));
            }
            else if (LoginType == AuthenticationRole.Teacher || LoginType == AuthenticationRole.Admin)
            {
                if (Teacher.Password == null)
                    Result.Add(new ValidationResult("You must enter a password.", new string[] { "Teacher.Password" }));

                if (Teacher.EmailAddress == null)
                    Result.Add(new ValidationResult("You must enter an email address", new string[] { "Teacher.EmailAddress" }));
                else if (!EmailMatcher.IsMatch(Teacher.EmailAddress))
                    Result.Add(new ValidationResult("Invalid email format.", new string[] { "Teacher.EmailAddress" }));
            }

            return Result;
        }
    }

    public class StudentLoginModel
    {
        [Display(Name="Password")]
        public string Password { get; set; }
    }

    public class TeacherLoginModel
    {
        [Display(Name = "Email address")]
        public string EmailAddress { get; set; }

        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}