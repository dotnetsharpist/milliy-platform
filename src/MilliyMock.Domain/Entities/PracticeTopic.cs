using MilliyMock.Domain.Commons;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Domain.Entities;

// Catalog of topics per subject — feeds the frontend topic filter dropdown.
// PracticeQuestion.Topic stores the Slug (loose join keeps JSON seeding simple).
public class PracticeTopic : Auditable
{
    public SubjectType Subject { get; set; }
    public string Slug { get; set; } = null!; // "algebra", "tarix-uzb", ...
    public string Name { get; set; } = null!; // display label, uz
    public int Order { get; set; }
}
