using Bogus;

namespace Layout
{
    public class ConteudoExtratoPdf
    {
        public string ClienteNome { get; set; }
        public string ClienteCnpjFormatado { get; set; }
        public string ClienteBanco { get; set; }
        public string ClienteAgenciaFormatada { get; set; }
        public string ClienteConta { get; set; }
        public DateTime DataHoraEmissao { get; set; }
        public DateTime DataHoraPeriodoDemonstrativo { get; set; }
        public decimal ValorSaldoContaCorrente { get; set; }
        public decimal ValorSaldoBloqueado { get; set; }
        public decimal ValorBloqueado { get; set; }
        public decimal ValorSaldoAplicado { get; set; }
        public decimal ValorSaldoTotalDisponivel { get; set; }
        public List<LancamentosDetalhados> LancamentosDetalhados { get; set; }

        public ConteudoExtratoPdf()
        {
            var _faker = new Faker();

            ClienteNome = _faker.Name.FullName();
            ClienteCnpjFormatado = _faker.Person.Phone;
            ClienteBanco = _faker.Name.FirstName();
            ClienteAgenciaFormatada = _faker.Name.FirstName();
            ClienteConta = _faker.Name.FirstName();
            DataHoraEmissao = _faker.Date.Past();
            DataHoraPeriodoDemonstrativo = _faker.Date.Past();
            ValorSaldoContaCorrente = _faker.Random.Decimal(1, 100);
            ValorSaldoBloqueado = _faker.Random.Decimal(1, 100); ;
            ValorBloqueado = _faker.Random.Decimal(1, 100); ;
            ValorSaldoAplicado = _faker.Random.Decimal(1, 100); ;
            ValorSaldoTotalDisponivel = _faker.Random.Decimal(1, 100); ;
            LancamentosDetalhados = BuildList(_faker);
        }

        public List<LancamentosDetalhados> BuildList(Faker _faker)
        {
            var list = new List<LancamentosDetalhados>()
            {
                new LancamentosDetalhados()
                {
                    Data = _faker.Date.Past(),
                    Historico = _faker.Name.FirstName(),
                    Operacao = _faker.Name.FirstName(),
                    Quantidade = _faker.Random.Decimal(1,10),
                    SaldoDiario = _faker.Random.Decimal(1,100),
                    Valor = _faker.Random.Decimal(1,100),
                },
                new LancamentosDetalhados()
                {
                    Data = _faker.Date.Past(),
                    Historico = _faker.Name.FirstName(),
                    Operacao = _faker.Name.FirstName(),
                    Quantidade = _faker.Random.Decimal(1,10),
                    SaldoDiario = _faker.Random.Decimal(1,100),
                    Valor = _faker.Random.Decimal(1,100),
                }
            };

            return list;
        }

    }

    public class LancamentosDetalhados
    {
        public DateTime? Data { get; set; }
        public decimal? Quantidade { get; set; }
        public string Historico { get; set; }
        public string Operacao { get; set; }
        public decimal? Valor { get; set; }
        public decimal? SaldoDiario { get; set; }
    }
}
