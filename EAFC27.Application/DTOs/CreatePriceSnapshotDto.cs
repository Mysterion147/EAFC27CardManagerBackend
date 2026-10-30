using System.ComponentModel.DataAnnotations;

namespace EAFC27.Application.DTOs;

public class CreatePriceSnapshotDto
{
    [Range(200, 15000000, ErrorMessage = "The price must be between 200 and 15,000,000.")]
    public int Price { get; set; }
}