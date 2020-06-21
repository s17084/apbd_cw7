using Cwiczenia10.DTOs.Requests;
using Cwiczenia10.Models.CreatedByScaffold;
using Cwiczenia5.DTOs.Requests;
using Cwiczenia5.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia5.Services
{
    public interface IStudentDbService
    {
        string DeleteStudent(string indexNumber);
        string UpdateStudent(UpdateStudentRequest req, string indexNumber);
        IEnumerable<Student> GetStudents();
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        PromoteStudentsResponse PromoteStudents(PromoteStudentsRequest request);
        bool CheckIndex(string indexNumber);
        string GetPassword(string indexNumber);
        string GetSalt(string indexNumber);
        void CreatePassword(string password, string salt, string indexNumber);
        ICollection<String> GetUserRoles(string indexNumber);
        void UpdateRefreshToken(string indexNumber, Guid refreshToken);
        string GetRefreshToken(string indexNumber);
    }
}
