using System.ComponentModel.DataAnnotations;

namespace MediCare.Web.Models;

// One row of the weekly-hours card (Index page).
public class ScheduleDayRowViewModel
{
    public string DayName { get; set; } = string.Empty;
    public bool IsWorking { get; set; }
    public string HoursText { get; set; } = "Off-Duty";
    public int SlotLengthMinutes { get; set; }
    public bool IsToday { get; set; }
}

public class WeeklyScheduleViewModel
{
    public List<ScheduleDayRowViewModel> Days { get; set; } = new();
    public int WorkingDayCount => Days.Count(d => d.IsWorking);
}

// The edit form (Edit page): one time range + one slot length + day toggles,
// matching the "Consultation & Availability Settings" part of the Doctor Profile design.
public class EditScheduleViewModel
{
    [Required]
    [Display(Name = "From")]
    public TimeOnly StartTime { get; set; } = new TimeOnly(9, 0);

    [Required]
    [Display(Name = "To")]
    public TimeOnly EndTime { get; set; } = new TimeOnly(17, 0);

    [Required]
    [Display(Name = "Consultation Slot Duration")]
    [Range(5, 240)]
    public int SlotLengthMinutes { get; set; } = 30;

    public List<DayToggleViewModel> Days { get; set; } = new();
}

public class DayToggleViewModel
{
    public DayOfWeek DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public bool IsWorking { get; set; }
}