using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace MediCare.Data
{

    // The converter marks every date read from the database as UTC, Nothing changes in the database
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter(): base(
                value => value,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
        {
        }
    }
}
