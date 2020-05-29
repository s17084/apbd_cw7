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
