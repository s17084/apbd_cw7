using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Cwiczenia3.DAL;
using Cwiczenia3.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cwiczenia3.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private static readonly string sqlConnectionLocal = "Data Source=DESKTOP-7GSCK0U\\SQLEXPRESS;Initial Catalog=s17084;Integrated Security=True";
        //"Data Source=db-mssql;Initial Catalog=s17084;Integrated Security=True";
        private static readonly string sqlConnectionString = sqlConnectionLocal;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudent()
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                List<Student> _students = new List<Student>();
                
                command.Connection = connection;
                command.CommandText = 
                    "SELECT s.FirstName, s.LastName, s.BirthDate, ss.Name, e.Semester " +
                    " FROM Student s " +
                    " JOIN Enrollment e ON e.IdEnrollment = s.IdEnrollment " +
                    " JOIN Studies ss ON ss.IdStudy = e.IdStudy ";

                connection.Open();
                var executeReader = command.ExecuteReader();
                while(executeReader.Read())
                {
                    var student = new Student();
                    student.FirstName = executeReader["FirstName"].ToString();
                    student.LastName = executeReader["LastName"].ToString();
                    student.BirthDate = DateTime.Parse(executeReader["BirthDate"].ToString());
                    student.StudyName = executeReader["Name"].ToString();
                    student.Semester = int.Parse(executeReader["Semester"].ToString());
                    _students.Add(student);
                }

                return Ok(_students);
            }
        }

        [HttpGet("{indexNumber}")]
        public IActionResult GetStudent(string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                List<Enrollment> _enrollments = new List<Enrollment>();

                //command.Connection = connection;
                //command.CommandText =
                //    "SELECT e.* FROM Student s " +
                //    " JOIN Enrollment e ON e.IdEnrollment = s.IdEnrollment " +
                //    " WHERE s.IndexNumber = '" + indexNumber + "'";

                // UWAGA!!! Przy powyższym rozwiązaniu, wywołanie:
                // api/students/';%20DROP%20TABLE%20Student%20--
                // spowoduje usunięcie tabeli "Students"!!!
                // 
                // Lepsze rozwiązanie z wykorzystaniem parametrów:

                command.Connection = connection;
                command.CommandText =
                    "SELECT e.* FROM Student s " +
                    " JOIN Enrollment e ON e.IdEnrollment = s.IdEnrollment " +
                    " WHERE s.IndexNumber = @indexNumber ";
                command.Parameters.AddWithValue("indexNumber", indexNumber);

                connection.Open();
                var executeReader = command.ExecuteReader();
                while (executeReader.Read())
                {
                    var enrollment = new Enrollment();
                    enrollment.IdEnrollment = int.Parse(executeReader["IdEnrollment"].ToString());
                    enrollment.Semester = int.Parse(executeReader["Semester"].ToString());
                    enrollment.IdStudy = int.Parse(executeReader["IdStudy"].ToString());
                    enrollment.StartDate = DateTime.Parse(executeReader["StartDate"].ToString());
                    _enrollments.Add(enrollment);
                }
                if (_enrollments.Count > 0)
                {
                    return Ok(_enrollments);
                }
                else
                {
                    return NotFound("Nie znaleziono studenta");
                }
            }
            
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            //student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id)
        {
            return Ok("Aktualizacja dokończona");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }

    }
}