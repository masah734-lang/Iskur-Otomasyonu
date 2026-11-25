namespace Iskur.Api;
public static class PlanningRules {
 public static DateOnly WeekStart(DateOnly d)=>d.AddDays(-(((int)d.DayOfWeek+6)%7));
 public static (int start,int end)[] Windows(string kind)=>kind switch {
  "Morning"=>[(480,720)],"Afternoon"=>[(810,1020)],"Full"=>[(480,720),(810,1020)],
  _=>throw new ArgumentException("Çalışma türü geçersiz.")};
 public static bool Conflicts(int courseStart,int courseEnd,string kind)=>Windows(kind).Any(w=>courseStart<w.end&&courseEnd>w.start);
 public static bool ExceedsWeeklyLimit(IEnumerable<DateOnly> dates)=>dates.Distinct().GroupBy(WeekStart).Any(g=>g.Count()>3);
}
