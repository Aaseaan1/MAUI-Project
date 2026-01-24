using iTextSharp.text;
using iTextSharp.text.pdf;
using JournalApp.Models;

namespace JournalApp.Services
{
        public class PdfExportService
        {
                // Helper to show emoji and label or fallback
                private string GetMoodEmojiAndLabel(string mood)
                {
                    if (string.IsNullOrWhiteSpace(mood)) return "📝 [No mood]";
                    return $"{GetMoodEmoji(mood)} {mood}";
                }
        public byte[] ExportEntryToPdf(JournalEntry entry)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();
            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
            var title = new Paragraph(entry.Title, titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph(" ")); // Spacer
            // Date and Mood
            var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            // Use Arial Unicode MS for emoji compatibility, fallback to Helvetica
            Font emojiFont;
            try {
                var arialUnicodePath = "/Library/Fonts/Arial Unicode.ttf";
                if (System.IO.File.Exists(arialUnicodePath)) {
                    var arialBaseFont = BaseFont.CreateFont(arialUnicodePath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    emojiFont = new Font(arialBaseFont, 14);
                } else {
                    emojiFont = FontFactory.GetFont(FontFactory.HELVETICA, 14);
                }
            } catch {
                emojiFont = FontFactory.GetFont(FontFactory.HELVETICA, 14);
            }
            var primaryMood = entry.PrimaryMood ?? string.Empty;
            var secondaryMood1 = entry.SecondaryMood1 ?? string.Empty;
            var secondaryMood2 = entry.SecondaryMood2 ?? string.Empty;
            // Grouped details, using emojiFont for mood fields
            document.Add(new Paragraph($"Date: {entry.EntryDate:MMMM dd, yyyy}", infoFont));
            document.Add(new Paragraph($"Primary Mood: {GetMoodEmojiAndLabel(primaryMood)}", emojiFont));
            document.Add(new Paragraph($"Secondary Mood 1: {GetMoodEmojiAndLabel(secondaryMood1)}", emojiFont));
            document.Add(new Paragraph($"Secondary Mood 2: {GetMoodEmojiAndLabel(secondaryMood2)}", emojiFont));
            document.Add(new Paragraph($"Category: {entry.Category ?? ""}", infoFont));
            document.Add(new Paragraph($"Tags: {entry.Tags ?? ""}", infoFont));
            document.Add(new Paragraph($"Word Count: {entry.WordCount}", infoFont));
            document.Add(new Paragraph("Comments:", infoFont));
            var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            var content = new Paragraph(entry.Content, contentFont);
            content.Alignment = Element.ALIGN_JUSTIFIED;
            document.Add(content);
            document.Close();
            writer.Close();
            return memoryStream.ToArray();
        }

        public byte[] ExportMultipleEntriesToPdf(List<JournalEntry> entries)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            var writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();
            // Main Title
            var mainTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24);
            var mainTitle = new Paragraph("Journal Entries", mainTitleFont);
            mainTitle.Alignment = Element.ALIGN_CENTER;
            document.Add(mainTitle);
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph($"Total Entries: {entries.Count}"));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph("_____________________________________________________________________"));
            foreach (var entry in entries.OrderByDescending(e => e.EntryDate))
            {
                document.NewPage();
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                // Use Arial Unicode MS for emoji compatibility, fallback to Helvetica
                Font emojiFont;
                try {
                    var arialUnicodePath = "/Library/Fonts/Arial Unicode.ttf";
                    if (System.IO.File.Exists(arialUnicodePath)) {
                        var arialBaseFont = BaseFont.CreateFont(arialUnicodePath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        emojiFont = new Font(arialBaseFont, 14);
                    } else {
                        emojiFont = FontFactory.GetFont(FontFactory.HELVETICA, 14);
                    }
                } catch {
                    emojiFont = FontFactory.GetFont(FontFactory.HELVETICA, 14);
                }
                // Always show all fields, even if empty
                document.Add(new Paragraph("==============================", infoFont));
                document.Add(new Paragraph($"Title: {entry.Title}", titleFont));
                var primaryMood = entry.PrimaryMood ?? string.Empty;
                var secondaryMood1 = entry.SecondaryMood1 ?? string.Empty;
                var secondaryMood2 = entry.SecondaryMood2 ?? string.Empty;
                // Grouped details, using emojiFont for mood fields
                document.Add(new Paragraph($"Date: {entry.EntryDate:MMMM dd, yyyy}", infoFont));
                document.Add(new Paragraph($"Primary Mood: {GetMoodEmojiAndLabel(primaryMood)}", emojiFont));
                document.Add(new Paragraph($"Secondary Mood 1: {GetMoodEmojiAndLabel(secondaryMood1)}", emojiFont));
                document.Add(new Paragraph($"Secondary Mood 2: {GetMoodEmojiAndLabel(secondaryMood2)}", emojiFont));
                document.Add(new Paragraph($"Category: {entry.Category ?? ""}", infoFont));
                document.Add(new Paragraph($"Tags: {entry.Tags ?? ""}", infoFont));
                document.Add(new Paragraph($"Word Count: {entry.WordCount}", infoFont));
                document.Add(new Paragraph("Comments:", infoFont));
                var contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var content = new Paragraph(entry.Content, contentFont);
                content.Alignment = Element.ALIGN_JUSTIFIED;
                document.Add(content);
                document.Add(new Paragraph("==============================", infoFont));
            }
            document.Close();
            writer.Close();
            return memoryStream.ToArray();
        }
        private string GetMoodEmojiOrDefault(string mood)
        {
            if (string.IsNullOrWhiteSpace(mood)) return "📝";
            return GetMoodEmoji(mood);
        }

        private string GetMoodEmoji(string mood)
        {
            return mood switch
            {
                "Happy" => "😃",
                "Relaxed" => "😌",
                "Confident" => "😎",
                "Sad" => "😢",
                "Angry" => "😠",
                "Anxious" => "😰",
                "Neutral" => "😐",
                _ => "📝"
            };
        }
    }
}
