using HMS.Application.Dtos;

namespace HMS.Application.Abstractions.Services;

public interface IPdfService
{
    byte[] GenerateWristbandPdf(WristbandDto data);
}