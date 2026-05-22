using MilliyMock.Domain.Commons;

namespace MilliyMock.Domain.Entities;

//
public class TempUser : Auditable
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FatherName { get; set; } = null!;
}