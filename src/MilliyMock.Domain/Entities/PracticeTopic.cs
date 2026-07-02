using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

// Catalog of topics per subject — feeds the frontend topic filter dropdown.
// PracticeQuestion.Topic stores the Slug (loose join keeps JSON seeding simple).
public class PracticeTopic : Auditable
{
    public SubjectType Subject { get; set; }
    public string Slug { get; set; } = null!; // "algebra", "temuriylar-davri", ...
    public string Name { get; set; } = null!; // display label, uz
    public string? Section { get; set; } // optional grouping, e.g. "O'zbekiston tarixi" / "Jahon tarixi"
    public int Order { get; set; }
}
