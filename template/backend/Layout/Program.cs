using iTextSharp.text;
using iTextSharp.text.pdf;
using Layout;
using System.Globalization;

byte[] GerarPdfConsolidadoContaCorrenteTextSharp(ConteudoExtratoPdf c, string imagePath)
{
    using var ms = new MemoryStream();
    var doc = new Document(PageSize.A4, 24, 24, 36, 36); // margem inferior um pouco maior p/ rodapé
    var writer = PdfWriter.GetInstance(doc, ms);
    doc.Open();

    // ===== FONTES =====
    var preto = BaseColor.BLACK;
    var cinza900 = new BaseColor(34, 34, 34);
    var cinza700 = new BaseColor(83, 89, 96);
    var cinzaLinha = new BaseColor(0xCE, 0xD0, 0xD2);

    var fTitulo = FontFactory.GetFont("OpenSans-Bold", 14, Font.BOLD, preto);
    var fSub = FontFactory.GetFont("OpenSans-Regular", 8, Font.NORMAL, preto);
    var fData = FontFactory.GetFont("OpenSans-Regular", 10, Font.NORMAL, preto);
    var fLabel = FontFactory.GetFont("OpenSans-Bold", 10, Font.BOLD, cinza900);
    var fValor = FontFactory.GetFont("OpenSans-Regular", 10, Font.NORMAL, cinza900);
    var fSaldoNum = FontFactory.GetFont("OpenSans-Bold", 12, Font.BOLD, cinza900);
    var fObs = FontFactory.GetFont("OpenSans-Regular", 8, Font.NORMAL, cinza700);
    var fTh = FontFactory.GetFont("OpenSans-Bold", 8, Font.BOLD, new BaseColor(8, 7, 7));
    var fTd = FontFactory.GetFont("OpenSans-Regular", 8, Font.NORMAL, preto);

    // ====== CABEÇALHO ======
    var header = new PdfPTable(new float[] { 350f, 197f }) { TotalWidth = 547f, LockedWidth = true };
    header.DefaultCell.Border = Rectangle.NO_BORDER;

    // Coluna esquerda
    var colEsq = new PdfPTable(1) { WidthPercentage = 100 };
    colEsq.DefaultCell.Border = Rectangle.NO_BORDER;
    colEsq.AddCell(new PdfPCell(new Phrase(c.ClienteNome, fData)) { Border = Rectangle.NO_BORDER, PaddingBottom = 2f });
    colEsq.AddCell(new PdfPCell(new Phrase("CNPJ " + c.ClienteCnpjFormatado, fData)) { Border = Rectangle.NO_BORDER });

    // Coluna direita
    var colDir = new PdfPTable(2) { WidthPercentage = 100 };
    colDir.DefaultCell.Border = Rectangle.NO_BORDER;
    colDir.SetWidths(new float[] { 60f, 137f });
    PdfPCell KV(string k, string v)
    {
        var p = new Paragraph();
        p.Add(new Chunk(k + " ", fLabel));
        p.Add(new Chunk(v ?? "-", fValor));
        var cell = new PdfPCell(p) { Border = Rectangle.NO_BORDER, PaddingBottom = 2f };
        return cell;
    }
    colDir.AddCell(KV("Banco", c.ClienteBanco));
    colDir.AddCell(KV("", ""));
    colDir.AddCell(KV("Agência", c.ClienteAgenciaFormatada));
    colDir.AddCell(KV("Conta", c.ClienteConta));

    // Linha de logo + colEsq
    var headerLinha = new PdfPTable(new float[] { 60f, 287f }) { WidthPercentage = 100 };
    headerLinha.DefaultCell.Border = Rectangle.NO_BORDER;
    PdfPCell logoCell;
    try
    {
        var logo = Image.GetInstance(imagePath);
        logo.ScaleToFit(48f, 48f);
        logo.Alignment = Element.ALIGN_LEFT;
        logoCell = new PdfPCell(logo) { Border = Rectangle.NO_BORDER, FixedHeight = 48f, Padding = 0 };
    }
    catch
    {
        logoCell = new PdfPCell { Border = Rectangle.NO_BORDER, FixedHeight = 48f };
    }
    headerLinha.AddCell(logoCell);
    headerLinha.AddCell(new PdfPCell(colEsq) { Border = Rectangle.NO_BORDER, PaddingLeft = 6f });

    header.AddCell(headerLinha);
    header.AddCell(new PdfPCell(colDir) { Border = Rectangle.NO_BORDER });
    doc.Add(header);

    // Emissão
    doc.Add(new Paragraph($"Emitido em {c.DataHoraEmissao}", fSub) { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 2f });

    // ====== TÍTULO ======
    doc.Add(new Paragraph("Extrato consolidado", fTitulo) { SpacingBefore = 10f, SpacingAfter = 2f });
    doc.Add(new Paragraph($"De {c.DataHoraPeriodoDemonstrativo}", fSub) { Alignment = Element.ALIGN_LEFT, SpacingAfter = 8f });

    // ====== SALDOS ======
    var saldosGrid = new PdfPTable(new float[] { 240f, 307f }) { WidthPercentage = 100 };
    saldosGrid.DefaultCell.Border = Rectangle.NO_BORDER;

    saldosGrid.AddCell(new PdfPCell(new Phrase("Saldo da Conta Corrente", fLabel))
    { Border = Rectangle.NO_BORDER, PaddingBottom = 4f });

    var valores = new PdfPTable(new float[] { 1f, 1f }) { WidthPercentage = 100 };
    valores.DefaultCell.Border = Rectangle.NO_BORDER;

    void LinhaValor(string label, string valor, bool bold = false, bool maior = false)
    {
        var fL = FontFactory.GetFont("OpenSans-Regular", 9, Font.NORMAL, cinza900);
        var fV = bold ? (maior ? fSaldoNum : FontFactory.GetFont("OpenSans-Bold", 10, Font.BOLD, cinza900))
                      : FontFactory.GetFont("OpenSans-Regular", 10, Font.NORMAL, cinza900);
        valores.AddCell(new PdfPCell(new Phrase(label, fL)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6f });
        valores.AddCell(new PdfPCell(new Phrase(valor, fV)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6f, HorizontalAlignment = Element.ALIGN_RIGHT });
    }

    string V(decimal? v) => v.HasValue ? v.Value.ToString("C", new CultureInfo("pt-BR")) : "R$ 0,00";
    LinhaValor("Saldo da Conta Corrente", V(c.ValorSaldoContaCorrente), bold: true, maior: true);
    LinhaValor("Saldo Bloqueado", V(c.ValorSaldoBloqueado));
    LinhaValor("CPMF", "R$ 0,00");
    LinhaValor("Valor Bloqueado", V(c.ValorBloqueado));
    LinhaValor("Saldo Aplicado", V(c.ValorSaldoAplicado));

    valores.AddCell(new PdfPCell(new Phrase("Saldo Total Disponível*", fLabel))
    { Border = Rectangle.NO_BORDER, PaddingTop = 6f });
    valores.AddCell(new PdfPCell(new Phrase(V(c.ValorSaldoTotalDisponivel), fSaldoNum))
    { Border = Rectangle.NO_BORDER, PaddingTop = 6f, HorizontalAlignment = Element.ALIGN_RIGHT });

    saldosGrid.AddCell(new PdfPCell(valores) { Border = Rectangle.NO_BORDER });
    doc.Add(saldosGrid);
    doc.Add(new Paragraph("*Valor inclui limites disponíveis", fSub)
    { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 4f, SpacingAfter = 6f });

    // ====== TABELA DE EXTRATO ======
    var tabela = new PdfPTable(6) { WidthPercentage = 100, SpacingBefore = 6f, SpacingAfter = 12f };
    tabela.SetWidths(new float[] { 12, 10, 42, 12, 12, 12 });
    var bgHeader = new BaseColor(230, 231, 232);
    void AddHeader(string t, int a = Element.ALIGN_LEFT) =>
        tabela.AddCell(new PdfPCell(new Phrase(t, fTh))
        { BackgroundColor = bgHeader, Border = Rectangle.NO_BORDER, PaddingTop = 6f, PaddingBottom = 6f, HorizontalAlignment = a });
    AddHeader("Data");
    AddHeader("Quantidade");
    AddHeader("Histórico");
    AddHeader("Operação", Element.ALIGN_CENTER);
    AddHeader("Valor (R$)", Element.ALIGN_RIGHT);
    AddHeader("Saldo diário (R$)", Element.ALIGN_RIGHT);

    bool zebra = false;
    if (c.LancamentosDetalhados == null || !c.LancamentosDetalhados.Any())
    {
        var cell = new PdfPCell(new Phrase("Não existem lançamentos para o período selecionado", fTd))
        {
            Colspan = 6,
            HorizontalAlignment = Element.ALIGN_CENTER,
            BackgroundColor = bgHeader,
            PaddingTop = 8f,
            PaddingBottom = 8f,
            Border = Rectangle.NO_BORDER
        };
        tabela.AddCell(cell);
    }
    else
    {
        foreach (var item in c.LancamentosDetalhados)
        {
            var bg = zebra ? new BaseColor(242, 243, 243) : BaseColor.WHITE;
            zebra = !zebra;
            string data = item.Data?.ToString("dd/MM/yyyy") ?? "-";
            tabela.AddCell(Cel(data, bg));
            tabela.AddCell(Cel(item.Quantidade?.ToString() ?? "-", bg));
            tabela.AddCell(Cel(item.Historico ?? "-", bg));
            tabela.AddCell(Cel(item.Operacao?.ToString() ?? "-", bg, Element.ALIGN_CENTER));
            tabela.AddCell(Cel(item.Valor?.ToString("N2", new CultureInfo("pt-BR")) ?? "-", bg, Element.ALIGN_RIGHT));
            tabela.AddCell(Cel(item.SaldoDiario?.ToString("N2", new CultureInfo("pt-BR")) ?? "-", bg, Element.ALIGN_RIGHT));
        }
    }
    doc.Add(tabela);

    // Observação
    doc.Add(new Paragraph(
        "Os saldos e lançamentos apresentados acima são baseados nas informações disponíveis no momento da emissão e podem sofrer alterações com novos lançamentos.",
        fObs)
    { SpacingBefore = 4f });

    // ====== RODAPÉ ======
    var cb = writer.DirectContent;
    var page = doc.PageSize;
    cb.SetLineWidth(0.5f);
    cb.SetColorStroke(new BaseColor(222, 226, 230));
    cb.MoveTo(doc.Left, doc.Bottom + 28);
    cb.LineTo(doc.Right, doc.Bottom + 28);
    cb.Stroke();

    var fRodape = FontFactory.GetFont(BaseFont.HELVETICA, 9, Font.NORMAL, new BaseColor(83, 89, 96));
    var left = new Phrase("Consulta de transações - Internet banking", fRodape);
    ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, left, doc.Left, doc.Bottom + 14, 0);

    var right = new Phrase($"Página {writer.PageNumber} de {writer.PageNumber}", fRodape);
    ColumnText.ShowTextAligned(cb, Element.ALIGN_RIGHT, right, doc.Right, doc.Bottom + 14, 0);

    doc.Close();
    return ms.ToArray();

    // === HELPER LOCAL DE CÉLULA ===
    PdfPCell Cel(string txt, BaseColor bg, int align = Element.ALIGN_LEFT)
    {
        return new PdfPCell(new Phrase(txt, fTd))
        {
            BackgroundColor = bg,
            Border = Rectangle.NO_BORDER,
            HorizontalAlignment = align,
            VerticalAlignment = Element.ALIGN_MIDDLE,
            PaddingTop = 5f,
            PaddingBottom = 5f,
            PaddingLeft = 6f,
            PaddingRight = 6f
        };
    }
}

byte[] pdfBytes = GerarPdfConsolidadoContaCorrenteTextSharp(new ConteudoExtratoPdf(), "C:\\Users\\Rodrigo\\Desktop\\download.jpeg");

// Caminho da Área de Trabalho do usuário atual
string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

// Nome do arquivo
string filePath = Path.Combine(desktopPath, "ExtratoConsolidado.pdf");

// Grava o arquivo
File.WriteAllBytes(filePath, pdfBytes);

Console.WriteLine($"PDF salvo em: {filePath}");