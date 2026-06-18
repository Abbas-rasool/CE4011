using System;
using System.Globalization;
using System.Linq;
using FrameAnalysis.UI.Core.Documents;
using FrameAnalysis.UI.Core.Services;
using FrameAnalysisProgram.ANALYSIS_CORE.Validation;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static MemberDesigner.Designers.Enums;

namespace FrameAnalysis.UI.Core.Reporting;

/// <summary>
/// Builds a printable timber member-design report (PDF) from a finished <see cref="DesignOutcome"/>.
/// Per check it shows the check title and value breakdown produced by the design classes
/// (<c>GetTitle()</c> / <c>GetSummary()</c>, carried on <see cref="CheckResult"/>), the
/// utilization ratio (D/C) and pass/fail status, plus the symbolic formula and standard clause
/// reference from <see cref="TimberDesignReferences"/>. WPF-free: the caller supplies the path.
/// </summary>
public static class DesignReportGenerator
{
    static DesignReportGenerator()
    {
        // QuestPDF Community licence (free for individuals / small orgs). Set once, before use.
        QuestPDF.Settings.License = LicenseType.Community;
        // Formulas use Greek / maths glyphs — don't fail if a font lacks one.
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    /// <summary>Generates the report and writes it to <paramref name="path"/>.</summary>
    public static void Save(string path, ProjectDocument doc, DesignOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(outcome);

        eTimberCode code = doc.Design.Code;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(t => t.FontFamily("Segoe UI").FontSize(9));

                page.Header().Element(h => ComposeHeader(h, doc, code));
                page.Content().PaddingVertical(8).Element(c => ComposeContent(c, outcome, code));
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf(path);
    }

    private static void ComposeHeader(IContainer container, ProjectDocument doc, eTimberCode code)
    {
        container.Column(col =>
        {
            col.Item().Text("Timber Member Design Report").FontSize(16).Bold();
            col.Item().PaddingTop(2).Text(t =>
            {
                t.Span("Project: ").SemiBold();
                t.Span(string.IsNullOrWhiteSpace(doc.ProjectName) ? "Untitled" : doc.ProjectName);
                t.Span($"      Code: {CodeName(code)}      Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeContent(IContainer container, DesignOutcome outcome, eTimberCode code)
    {
        container.Column(col =>
        {
            col.Spacing(14);

            // --- Summary ---
            col.Item().Column(s =>
            {
                s.Item().Text("Summary").FontSize(12).Bold();
                s.Item().Text($"Members designed: {outcome.Results.Count}");

                var governing = outcome.Results.OrderByDescending(r => r.GoverningUtilization).FirstOrDefault();
                if (governing is not null)
                    s.Item().Text($"Governing member: {governing.ElementId}  (D/C = {governing.GoverningUtilization:0.00}, {governing.GoverningStatus})");

                bool anyFail = outcome.Results.Any(r => r.GoverningStatus == eDesignStatus.Fail);
                s.Item().Text(t =>
                {
                    t.Span("Overall: ").SemiBold();
                    t.Span(anyFail ? "FAIL — one or more members exceed capacity"
                                   : "PASS — all members within capacity")
                        .FontColor(anyFail ? Colors.Red.Medium : Colors.Green.Medium).Bold();
                });
            });

            if (outcome.Results.Count == 0)
                col.Item().Text("No member results were produced. Check that materials have design values and that the analysis ran successfully.")
                    .Italic();

            // --- Per-member sections ---
            foreach (var member in outcome.Results.OrderBy(r => r.ElementId))
                col.Item().Element(c => ComposeMember(c, member, code));

            // --- Messages ---
            if (outcome.Messages.Count > 0)
                col.Item().Column(m =>
                {
                    m.Item().Text("Messages").FontSize(12).Bold();
                    foreach (var msg in outcome.Messages)
                        m.Item().Text($"[{msg.Severity}] {msg.Message}")
                            .FontColor(msg.Severity == ValidationSeverity.Error ? Colors.Red.Medium
                                       : msg.Severity == ValidationSeverity.Warning ? Colors.Orange.Darken1
                                       : Colors.Grey.Darken1);
                });
        });
    }

    private static void ComposeMember(IContainer container, MemberDesignResult member, eTimberCode code)
    {
        container.Column(col =>
        {
            col.Spacing(5);

            string gov = member.GoverningCheck is { } gc ? gc.ToString() : "—";
            col.Item().Text(t =>
            {
                t.Span($"Member {member.ElementId}").FontSize(11).Bold();
                t.Span($"   —  governing: {gov}  (D/C = {member.GoverningUtilization:0.00}, {member.GoverningStatus})");
            });

            foreach (var check in member.Checks.OrderBy(c => c.CheckType))
                col.Item().Element(c => ComposeCheck(c, check, code));
        });
    }

    private static void ComposeCheck(IContainer container, CheckResult check, eTimberCode code)
    {
        var entry = TimberDesignReferences.Get(code, check.CheckType);
        string title = string.IsNullOrWhiteSpace(check.Title) ? check.CheckType.ToString() : check.Title;

        var statusColor = check.Status switch
        {
            eDesignStatus.Pass => Colors.Green.Medium,
            eDesignStatus.Fail => Colors.Red.Medium,
            _ => Colors.Orange.Darken1
        };

        container.Border(0.75f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(col =>
        {
            // Title + D/C + status
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(title).SemiBold().FontSize(10);
                row.ConstantItem(170).AlignRight().Text(t =>
                {
                    t.Span($"D/C = {check.Utilization.ToString("0.00", CultureInfo.InvariantCulture)}    ");
                    t.Span(check.Status.ToString()).FontColor(statusColor).Bold();
                });
            });

            // Standard clause reference. The formula + values come from the check's GetSummary below.
            if (!string.IsNullOrWhiteSpace(entry.Reference))
                col.Item().PaddingTop(2).Text(t =>
                {
                    t.Span("Reference:  ").SemiBold();
                    t.Span(entry.Reference).FontColor(Colors.Grey.Darken1);
                });

            // Value breakdown from the check's GetSummary()
            if (!string.IsNullOrWhiteSpace(check.Summary))
                col.Item().PaddingTop(3).Text(check.Summary).FontSize(8.5f).FontColor(Colors.Grey.Darken3);

            if (!string.IsNullOrWhiteSpace(check.GoverningCombination))
                col.Item().PaddingTop(2).Text($"Governing combination: {check.GoverningCombination}")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static string CodeName(eTimberCode code) => code switch
    {
        eTimberCode.US => "US — NDS 2018",
        eTimberCode.EC5 => "Eurocode 5 (EN 1995-1-1)",
        eTimberCode.TR => "Turkish — TS 647",
        _ => code.ToString()
    };
}
