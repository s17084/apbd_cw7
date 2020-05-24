using Cwiczenia5.DTOs.Requests;
using Cwiczenia5.DTOs.Responses;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Cwiczenia5.Services
{
    public class SqlServerStudentDbService : IStudentDbService
    {
        //"Data Source=db-mssql;Initial Catalog=s17084;Integrated Security=True";
        // Operowano na bazie lokalnej w związku z problemami technicznymi db-mssql
        private static readonly string sqlConnectionString = "Data Source=DESKTOP-7GSCK0U\\SQLEXPRESS;Initial Catalog=s17084;Integrated Security=True";

        public SqlServerStudentDbService() { }

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
                    // 1. Sprawdzamy czy istnieją studia podane w request
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

                    // 2. Sprawdzamy czy istnieje odpowiedni wpis w Enrollment, jeśli nie to go dodajemy
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

                    // 3. Dodajemy odpowiedni wpis do tabeli Student sprawdzając jednocześnie czy podano unikatowy nr indeksu
                    // jest to co prawda PK więc nie trzeba, ale skoro jest w poleceniu, to sprawdzamy
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
                    // 1. Sprawdzamy czy istnieje odpowiedni wpis w tabeli Enrollment
                    //    UWAGA!!! Funkcjonalność przeniesiona do procedury składowanej

                    //command.CommandText = "" +
                    //    "SELECT IdEnrollment FROM Enrollment e" +
                    //    " JOIN Studies s ON e.IdStudy = s.IdStudy" +
                    //    " WHERE s.Name=@Name AND e.Semester=@Semester";
                    //command.Parameters.AddWithValue("Name", request.Studies);
                    //command.Parameters.AddWithValue("Semester", request.Semester);

                    //idEnrollment = command.ExecuteScalar();
                    //if(idEnrollment == null)
                    //{
                    //    response.Message = "There is no such studies in database";
                    //    return response;
                    //}

                    // 2. Wywołanie procedury składowanej
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

                return true;
            }
        }
    }
}
