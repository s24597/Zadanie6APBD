using Zadanie6APBD.Data;
using Zadanie6APBD.DTO;
using Zadanie6APBD.Models;

namespace Zadanie6APBD.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zadanie6APBD.Data;
using Zadanie6APBD.Models;
using Zadanie6APBD.DTO;

    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public PrescriptionsController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] PrescriptionRequest request)
        {
            if (request.Medicaments.Count > 10)
                return BadRequest("Recepta może obejmować maksymalnie 10 leków.");

            if (request.DueDate < request.Date)
                return BadRequest("DueDate musi być większe lub równe Date.");

            var patient = await _context.Patients.FindAsync(request.Patient.IdPatient);
            if (patient == null)
            {
                patient = new Patient
                {
                    FirstName = request.Patient.FirstName,
                    LastName = request.Patient.LastName,
                    Birthdate = request.Patient.Birthdate
                };
                _context.Patients.Add(patient);
            }

            foreach (var med in request.Medicaments)
            {
                if (!await _context.Medicaments.AnyAsync(m => m.IdMedicament == med.IdMedicament))
                    return BadRequest($"Lek o ID {med.IdMedicament} nie istnieje.");
            }

            var prescription = new Prescription
            {
                Date = request.Date,
                DueDate = request.DueDate,
                IdPatient = patient.IdPatient,
                IdDoctor = request.IdDoctor,
                PrescriptionMedicaments = request.Medicaments.Select(m => new PrescriptionMedicament
                {
                    IdMedicament = m.IdMedicament,
                    Dose = m.Dose,
                    Details = m.Details
                }).ToList()
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return Ok(prescription);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientDetails(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.PrescriptionMedicaments)
                        .ThenInclude(pm => pm.Medicament)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                .FirstOrDefaultAsync(p => p.IdPatient == id);

            if (patient == null)
                return NotFound();

            return Ok(patient);
        }
    }