namespace HMS.Application.Dtos;

public class FloorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int Number { get; set; }
}