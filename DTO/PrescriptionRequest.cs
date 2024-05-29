namespace Zadanie6APBD.DTO;

public class PrescriptionRequest
{
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public PatientDto Patient { get; set; }
    public int IdDoctor { get; set; }
    public List<MedicamentDto> Medicaments { get; set; }
}