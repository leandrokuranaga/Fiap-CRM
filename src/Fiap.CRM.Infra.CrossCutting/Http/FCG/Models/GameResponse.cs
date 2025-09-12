namespace Fiap.CRM.Infra.CrossCutting.Http.FCG.Models
{
    public record GameResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public int? PromotionId { get; set; }
        public decimal PriceWithDiscount { get; set; }
    }

    public record ApiEnvelope<T>(T Data, bool Success, string? Message);
}
