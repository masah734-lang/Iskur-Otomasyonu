using Iskur.Api;
using static Iskur.Api.PlanningRules;
var passed=0;
void Check(string name,bool value){if(!value)throw new Exception("FAIL: "+name);passed++;Console.WriteLine("PASS: "+name);}
Check("Tam gün molayı dışarıda tutar",PlanningRules.Windows("Full").Sum(w=>w.end-w.start)==450);
Check("Sabah dört saat",PlanningRules.Windows("Morning").Sum(w=>w.end-w.start)==240);
Check("Öğleden sonra üç buçuk saat",PlanningRules.Windows("Afternoon").Sum(w=>w.end-w.start)==210);
Check("Mola içindeki ders çakışmaz",!Conflicts(720,810,"Full"));
Check("Bitişik ders çakışmaz",!Conflicts(660,720,"Afternoon"));
Check("Öğleden sonra ders çakışır",Conflicts(840,960,"Afternoon"));
Check("Tam gün sabah dersi çakışır",Conflicts(540,600,"Full"));
Check("Pazar önceki pazartesiye aittir",WeekStart(new(2026,11,1))==new DateOnly(2026,10,26));
Check("Yeni haftanın pazartesisi",WeekStart(new(2026,11,2))==new DateOnly(2026,11,2));
Check("Aylar arasında dördüncü gün engellenir",ExceedsWeeklyLimit([new(2026,4,27),new(2026,4,28),new(2026,4,29),new(2026,5,1)]));
Check("Farklı haftalarda üçer gün serbest",!ExceedsWeeklyLimit([new(2026,11,2),new(2026,11,3),new(2026,11,4),new(2026,11,9),new(2026,11,10),new(2026,11,11)]));
Check("Aynı tarih üst sınırda iki kere sayılmaz",!ExceedsWeeklyLimit([new(2026,11,2),new(2026,11,2),new(2026,11,3),new(2026,11,4)]));
try{PlanningRules.Windows("Invalid");throw new Exception("Geçersiz tür kabul edildi");}catch(ArgumentException){passed++;Console.WriteLine("PASS: Geçersiz çalışma türü reddedilir");}
Console.WriteLine($"{passed} test başarılı.");
