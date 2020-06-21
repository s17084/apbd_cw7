using Cwiczenia10.DTOs.Requests;
using Cwiczenia10.Models.CreatedByScaffold;
using Cwiczenia5.DTOs.Requests;
using Cwiczenia5.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Cwiczenia5.Services
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        private static readonly string sqlConnectionString = "Data Source=db-mssql;Initial Catalog=s17084;Integrated Security=True";

        private readonly s17084Context _context;

        public SqlServerStudentDbService(s17084Context context)
        {
            _context = context;
        }

        public string DeleteStudent(string indexNumber)
        {
            var student = _context.Student.Find(indexNumber);
            if (student == null)
            {
                return "NO_SUCH_STUDENT";
            }

            var studentRoles = _context.StudentRole.Where(e => e.IndexNumber == indexNumber);
            foreach (var studentRole in studentRoles)
            {
                _context.Remove(studentRole);
            }
            var enrollment = _context.Enrollment.Where(e => e.IdEnrollment == student.IdEnrollment).Single();
            enrollment.Student.Remove(student);

            _context.Remove(student);
            _context.SaveChanges();
            return "OK";
        }

        public string UpdateStudent(UpdateStudentRequest req, string indexNumber)
        {
            var student = _context.Student.Find(indexNumber);
            if (student == null)
            {
                return "NO_SUCH_STUDENT";
            }

            _context.Entry(student).CurrentValues.SetValues(req);
            _context.SaveChanges();
            return "OK";
        }

        public IEnumerable<Student> GetStudents()
        {
            return _context.Student.ToList();
        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            var response = new EnrollStudentResponse();
            var semester = 1;

            if(!_context.Studies.Where(e => e.Name == request.Studies).Any())
            {
                response.Message = "The is no such studies in database";
                return response;
            }

            var studies = _context.Studies.Where(e => e.Name == request.Studies).Single();
            var idStudies = studies.IdStudy;

            var enrollment = _context
                .Enrollment
                .Where(e => e.IdStudy == idStudies && e.Semester == semester)
                .Include(e => e.Student)
                .Single();
            if(enrollment == null)
            {
                enrollment = new Enrollment { Semester = semester, StartDate = new DateTime(), IdStudyNavigation = studies };
            }

            if(_context.Student.Where(e => e.IndexNumber == request.IndexNumber).Any())
            {
                response.Message = "Duplicate index number";
                return response;
            }

            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                IndexNumber = request.IndexNumber,
                BirthDate = (DateTime)request.Birthdate,
                IdEnrollmentNavigation = enrollment,
            };

            enrollment.Student.Add(student);

            _context.SaveChanges();

            response.IndexNumber = request.IndexNumber;
            response.Semester = semester;
            response.Message = "Student enrolled";
            return response;
        }

        public PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request)
        {
            var response = new PromoteStudentsResponse();

            if (!_context
                .Enrollment
                .Where(e => e.IdStudyNavigation.Name == request.Studies && e.Semester == request.Semester)
                .Any())
            {
                response.Message = "NO_SUCH_ENROLLMENT";
                return response;
            }

            var oldEnrollment = _context
                .Enrollment
                .Where(e => e.IdStudyNavigation.Name == request.Studies && e.Semester == request.Semester)
                .Include(e => e.Student)
                .Single();

            Enrollment newEnrollment = null;
            
            if(_context
                .Enrollment
                .Where(e => e.IdStudyNavigation.Name == request.Studies && e.Semester == request.Semester + 1)
                .Any())
            {
                newEnrollment = _context
                    .Enrollment
                    .Where(e => e.IdStudyNavigation.Name == request.Studies && e.Semester == request.Semester + 1)
                    .Single();
            }
                

            if(newEnrollment == null)
            {
                newEnrollment = new Enrollment
                {
                    Semester = request.Semester + 1,
                    IdStudyNavigation = _context.Studies.Where(e => e.Name == request.Studies).Single(),
                    StartDate = new DateTime(),
                };
            }

            foreach(Student student in oldEnrollment.Student)
            {
                student.IdEnrollmentNavigation = newEnrollment;
            }

            _context.SaveChanges();

            response.IdEnrollment = newEnrollment.IdEnrollment;
            response.Message = "OK";

            return response;
        }

        /*
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                var response = new EnrollStudentResponse();
                var semester = 1;

                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "SELECT IdStudy FROM Studies WHERE Name=@Name";
                    command.Parameters.AddWithValue("Name", request.Studies);

                    var dataReader = command.ExecuteReader();

                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        transaction.Rollback();
                        response.Message = "The is no such studies in database";
                        return response;
                    }

                    var idStudy = (int)dataReader["IdStudy"];
                    dataReader.Close();

                    command.Parameters.Clear();
                    command.CommandText = "SELECT IdEnrollment FROM Enrollment WHERE IdStudy=@IdStudy AND Semester=@Semester";
                    command.Parameters.AddWithValue("IdStudy", idStudy);
                    command.Parameters.AddWithValue("Semester", semester);

                    int idEnrollment;

                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        command.Parameters.Clear();
                        command.CommandText = "SELECT MAX(IdEnrollment) FROM Enrollment";
                        idEnrollment = (int)command.ExecuteScalar() + 1;
                        command.CommandText = "INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate)" +
                            " VALUES (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                        command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                        command.Parameters.AddWithValue("Semester", semester);
                        command.Parameters.AddWithValue("IdStudy", idStudy);
                        command.Parameters.AddWithValue("StartDate", new DateTime(2020, 10, 1, 8, 0, 0));
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        idEnrollment = (int)dataReader["IdEnrollment"];
                        dataReader.Close();
                    }
                    command.Parameters.Clear();
                    command.CommandText = "SELECT IndexNumber FROM Student";
                    dataReader = command.ExecuteReader();
                    string indexNumber;
                    while (dataReader.Read())
                    {
                        indexNumber = (string)dataReader["IndexNumber"];
                        if (indexNumber == request.IndexNumber)
                        {
                            response.Message = "Duplicate index number";
                            return response;
                        }
                    }
                    dataReader.Close();

                    command.Parameters.Clear();
                    command.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment)" +
                            " VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                    command.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    command.Parameters.AddWithValue("FirstName", request.FirstName);
                    command.Parameters.AddWithValue("LastName", request.LastName);
                    command.Parameters.AddWithValue("BirthDate", request.Birthdate);
                    command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    response.Message = e.Message;
                    return response;
                }

                transaction.Commit();

                response.IndexNumber = request.IndexNumber;
                response.Semester = semester;
                response.Message = "Student enrolled";
                return response;
            }
        }
        

        public PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                var response = new PromoteStudentsResponse();
                Object idEnrollment;

                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.Parameters.Clear();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "PromoteStudents";
                    command.Parameters.AddWithValue("Studies", request.Studies);
                    command.Parameters.AddWithValue("Semester", request.Semester);

                    var returnValue = command.Parameters.Add("@ReturnValue", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;

                    command.ExecuteNonQuery();

                    if ((int)returnValue.Value == 0)
                    {
                        response.Message = "There is no such semester in database";
                        return response;
                    }
                    else
                    {
                        idEnrollment = returnValue.Value;
                    }
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    response.Message = e.Message;
                    return response;
                }

                transaction.Commit();

                response.IdEnrollment = (int)idEnrollment;
                response.Message = "Ok";
                return response;
            }
        }
        */

        public bool CheckIndex(string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {

                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    command.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber=@IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.CommandType = CommandType.Text;

                    var existingIndexNumber = (string)command.ExecuteScalar();

                    if (existingIndexNumber == null)
                    {
                        return false;
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }
                transaction.Commit();
                return true;
            }
        }

        public string GetPassword(string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                string password = null;

                try
                {
                    command.CommandText = "SELECT Password, IndexNumber FROM Student WHERE IndexNumber=@IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.CommandType = CommandType.Text;

                    var dataReader = command.ExecuteReader();

                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        transaction.Rollback();
                        password = "NO_SUCH_USER";
                        return password;
                    }

                    if (!dataReader.IsDBNull(0))
                    {
                        password = (string)dataReader["Password"];
                    }

                    dataReader.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }
                transaction.Commit();
                return password;
            }
        }

        public void CreatePassword(string password, string salt, string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.Parameters.Clear();
                    command.CommandText = "UPDATE Student SET Password = @Password, Salt = @Salt" +
                            " WHERE IndexNumber = @IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.Parameters.AddWithValue("Password", password);
                    command.Parameters.AddWithValue("Salt", salt);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    Console.WriteLine(e);
                }
                transaction.Commit();
            }
        }

        public ICollection<String> GetUserRoles(string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                ICollection<string> _roles = new List<string>();
                try
                {
                    command.CommandText = "SELECT r.RoleName FROM Roles r " +
                        " JOIN StudentRole sr ON r.IdRole = sr.IdRole " +
                        " JOIN Student s ON sr.IndexNumber = s.IndexNumber " +
                        " WHERE s.IndexNumber=@IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.CommandType = CommandType.Text;

                    var dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        _roles.Add(dataReader["RoleName"].ToString());
                    }

                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }
                return _roles;

            }
        }

        public string GetSalt(string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                string salt = null;

                try
                {
                    command.CommandText = "SELECT Salt, IndexNumber FROM Student WHERE IndexNumber=@IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.CommandType = CommandType.Text;

                    var dataReader = command.ExecuteReader();

                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        transaction.Rollback();
                        salt = "NO_SUCH_USER";
                        return salt;
                    }

                    if (!dataReader.IsDBNull(0))
                    {
                        salt = (string)dataReader["Salt"];
                    }

                    dataReader.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }
                transaction.Commit();
                return salt;
            }
        }

        public void UpdateRefreshToken(string indexNumber, Guid refreshToken)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    command.Parameters.Clear();
                    command.CommandText = "UPDATE Student SET RefreshToken = @RefreshToken " +
                            " WHERE IndexNumber = @IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.Parameters.AddWithValue("RefreshToken", refreshToken.ToString());
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    Console.WriteLine(e);
                }
                transaction.Commit();
            }
        }

        public string GetRefreshToken(string indexNumber)
        {
            using (var connection = new SqlConnection(sqlConnectionString))
            using (var command = new SqlCommand())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                var transaction = connection.BeginTransaction();

                command.Connection = connection;
                command.Transaction = transaction;

                string refreshToken = null;

                try
                {
                    command.CommandText = "SELECT RefreshToken, IndexNumber FROM Student WHERE IndexNumber=@IndexNumber";
                    command.Parameters.AddWithValue("IndexNumber", indexNumber);
                    command.CommandType = CommandType.Text;

                    var dataReader = command.ExecuteReader();

                    if (!dataReader.Read())
                    {
                        dataReader.Close();
                        transaction.Rollback();
                        refreshToken = "NO_SUCH_USER";
                        return refreshToken;
                    }

                    if (!dataReader.IsDBNull(0))
                    {
                        refreshToken = (string)dataReader["RefreshToken"];
                    }

                    dataReader.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                }
                transaction.Commit();
                return refreshToken;
            }
        }

    } 
}
