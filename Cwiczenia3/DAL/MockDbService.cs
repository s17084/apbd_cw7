using Cwiczenia3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cwiczenia3.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<Student> _students;

        static MockDbService()
        {
            _students = new List<Student>
            {
                //new Student{IdStudent=1, FirstName="Dariusz", LastName="Kulig" },
                //new Student{IdStudent=2, FirstName="Katarzyna", LastName="Rychlik" },
                //new Student{IdStudent=3, FirstName="Andrzej", LastName="Wawrzeńczyk" },
            };
        }

        public IEnumerable<Student> GetStudents()
        {
            return _students;
        }
    }
}
