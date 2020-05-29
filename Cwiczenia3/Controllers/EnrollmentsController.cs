using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cwiczenia5.DTOs.Requests;
using Cwiczenia5.DTOs.Responses;
using Cwiczenia5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cwiczenia5.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    [Authorize(Roles="employee")]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var response = _service.EnrollStudent(request);

            if (response.IndexNumber == null)
            {
                return BadRequest(response.Message);
            }

            return CreatedAtAction(nameof(EnrollStudent), response);
        }

        [HttpPost("promotions")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            var response = _service.PromoteStudents(request);

            if (response.IdEnrollment == 0)
            {
                return NotFound(response.Message);
            }

            return CreatedAtAction(nameof(PromoteStudents), response);
        }
    }
}