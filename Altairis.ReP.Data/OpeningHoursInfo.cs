namespace Altairis.ReP.Data;

public class OpeningHoursInfo
{

    public DateTime Date { get; set; }

    public bool IsException { get; set; }

    public TimeSpan OpeningTime { get; set; }

    public TimeSpan ClosingTime { get; set; }

    public DateTime AbsoluteOpeningTime => Date.Add(OpeningTime);

    public DateTime AbsoluteClosingTime => Date.Add(ClosingTime);

    public bool IsOpen => ClosingTime.Subtract(OpeningTime) > TimeSpan.Zero;

    public override string ToString() => IsOpen ? $"{OpeningTime:hh\\:mm} - {ClosingTime:hh\\:mm}" : string.Empty;

}
