using JournalApp.Data;
using JournalApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace JournalApp.Services;

public class JournalService
{
    private readonly JournalDbContext _context;
    private readonly string _dbPath;
    private readonly string _exportFilePath;

    public JournalService(JournalDbContext context)
    {
        _context = context;
        _dbPath = "journalapp.db";
        _exportFilePath = "database_query.txt";
    }

    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _context.Entries.OrderByDescending(e => e.EntryDate).ToListAsync();
    }

    public async Task<JournalEntry?> GetEntryAsync(int id)
    {
        return await _context.Entries.FindAsync(id);
    }

    public async Task<JournalEntry?> GetTodayEntryAsync()
    {
        return await _context.Entries.FirstOrDefaultAsync(e => e.EntryDate == DateTime.Today);
    }

    public async Task<bool> HasEntryForDateAsync(DateTime date)
    {
        return await _context.Entries.AnyAsync(e => e.EntryDate == date.Date);
    }

    public async Task AddEntryAsync(JournalEntry entry)
    {
        entry.EntryDate = DateTime.Today;
        entry.CreatedDate = DateTime.Now;
        entry.WordCount = CountWords(entry.Content);
        
        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();
        await ExportDatabaseQueryAsync();
    }

    public async Task UpdateEntryAsync(JournalEntry entry)
    {
        var existing = await _context.Entries.FindAsync(entry.Id);
        if (existing != null)
        {
            existing.Title = entry.Title;
            existing.Content = entry.Content;
            existing.PrimaryMood = entry.PrimaryMood;
            existing.SecondaryMood1 = entry.SecondaryMood1;
            existing.SecondaryMood2 = entry.SecondaryMood2;
            existing.Category = entry.Category;
            existing.Tags = entry.Tags;
            existing.ModifiedDate = DateTime.Now;
            existing.WordCount = CountWords(entry.Content);
            
            _context.Entries.Update(existing);
            await _context.SaveChangesAsync();
            await ExportDatabaseQueryAsync();
        }
    }

    public async Task DeleteEntryAsync(int id)
    {
        var entry = await _context.Entries.FindAsync(id);
        if (entry != null)
        {
            _context.Entries.Remove(entry);
            await _context.SaveChangesAsync();
            await ExportDatabaseQueryAsync();
        }
    }

    public async Task<List<JournalEntry>> SearchEntriesAsync(string query)
    {
        query = query.ToLower();
        return await _context.Entries
            .Where(e => e.Title.ToLower().Contains(query) || e.Content.ToLower().Contains(query))
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> FilterByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Entries
            .Where(e => e.EntryDate >= startDate.Date && e.EntryDate <= endDate.Date)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> FilterByMoodAsync(string mood)
    {
        return await _context.Entries
            .Where(e => e.PrimaryMood == mood)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    public async Task<List<JournalEntry>> FilterByTagAsync(string tag)
    {
        return await _context.Entries
            .Where(e => e.Tags.Contains(tag))
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private async Task ExportDatabaseQueryAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sqlite3",
                    Arguments = $"\"{_dbPath}\" \".schema Entries\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        using (var reader = process.StandardOutput)
                        {
                            var schema = reader.ReadToEnd();
                            File.WriteAllText(_exportFilePath, schema);
                        }
                        process.WaitForExit();
                    }
                }

                // Append data
                psi.Arguments = $"\"{_dbPath}\" \"SELECT * FROM Entries;\"";
                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        using (var reader = process.StandardOutput)
                        {
                            var data = reader.ReadToEnd();
                            File.AppendAllText(_exportFilePath, data);
                        }
                        process.WaitForExit();
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exporting database query: {ex.Message}");
        }
    }
}

