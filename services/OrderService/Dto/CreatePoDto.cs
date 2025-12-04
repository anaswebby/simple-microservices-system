namespace OrderService.Dto
{
    public record CreatePoItemDto(string ProductSku, int Quantity);
    public record CreatePoDto(string? PoNumber, List<CreatePoItemDto> Items);
}
