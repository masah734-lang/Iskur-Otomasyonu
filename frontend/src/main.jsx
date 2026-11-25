import React, {useState,useEffect} from 'react';
import {createRoot} from 'react-dom/client';
import {api,setToken} from './api';
import './style.css';
import { Administration, Attendance, Leaves, NewParticipation, QrDisplay, Reports, Settings, Tasks, localToday } from './final';
import { calendarTone, courseTimeRange, formatCourseTime } from './calendar.mjs';
import { readRoute, routeHash } from './navigation.mjs';
const roleNames={Student:'Öğrenci',Advisor:'Danışman',DepartmentHead:'Bölüm Başkanı',FacultyManagement:'Fakülte Yönetimi'};
const states={Draft:'Taslak',Submitted:'İncelemede',Pending:'Bekliyor',Accepted:'Kabul edildi',Rejected:'Reddedildi'};
const kinds={Full:'Tam gün · 08.00–17.00',Morning:'Sabah · 08.00–12.00',Afternoon:'Öğleden sonra · 13.30–17.00'};
const weekdays=['Pazar','Pazartesi','Salı','Çarşamba','Perşembe','Cuma','Cumartesi'];
function Field({label,...props}){return <label>{label}<input {...props}/></label>}
const studentMenu = [
 ['profile', 'Profilim'], ['courses', 'Ders programım'], ['plan', 'Aylık çalışma planım'],
 ['attendance', 'Yoklamam'], ['leaves', 'İzinlerim'], ['tasks', 'Görevlerim'], ['reports', 'Çalışma dökümüm']
];
const staffMenu = [
 ['students', 'Öğrenciler'], ['profile', 'Öğrenci profili'], ['plan', 'Çalışma planları'],
 ['attendance', 'Yoklama ve devamsızlık'], ['qr', 'Yoklama karekodu'], ['leaves', 'İzin talepleri'],
 ['tasks', 'Görevler'], ['reports', 'Raporlar']
];
const managementMenu = [
 ['new', 'Öğrenci ekle'], ['holidays', 'Tatil takvimi'], ['settings', 'Program ayarları'], ['administration', 'Kurum yönetimi']
];
function menuFor(roles) {
 if (roles.includes('Student')) return studentMenu;
 return roles.includes('FacultyManagement') ? [...staffMenu, ...managementMenu] : staffMenu;
}
function currentRoute() {
 return readRoute(window.location, window.history.state);
}
function defaultStudent(rows) {
 const today=localToday();
 return (rows.find(row=>row.active&&row.start<=today&&row.end>=today) || rows[0])?.id ?? null;
}
function App(){
 const [route,setRoute]=useState(currentRoute);
 const [user,setUser]=useState(null),[message,setMessage]=useState(''),[error,setError]=useState(''),[busy,setBusy]=useState(false),[look,setLook]=useState({departments:[],periods:[],advisors:[],workUnits:[],qrUnits:[]}),[students,setStudents]=useState([]),[scopeStudents,setScopeStudents]=useState([]);
 const [filters,setFilters]=useState({search:'',departmentId:'',workUnitId:'',active:'',advisorId:'',year:'',month:''});
 const student=user?.roles.includes('Student'),management=user?.roles.includes('FacultyManagement'),head=user?.roles.includes('DepartmentHead'),advisor=user?.roles.includes('Advisor'),review=management||advisor;
 const menu=user?menuFor(user.roles):[];
 const page=menu.some(([id])=>id===route.page)?route.page:menu[0]?.[0];
 const selected=scopeStudents.some(row=>row.id===route.selected)?route.selected:student?defaultStudent(scopeStudents):null;
 const needsStudent=['profile','courses','plan','attendance','leaves','tasks'].includes(page);
 const planMonth=route.month||localToday().slice(0,7);

 async function run(fn,success){setError('');setMessage('');setBusy(true);try{await fn();if(success)setMessage(success);}catch(e){setError(e.message);}finally{setBusy(false);}}
 function writeRoute(nextPage, nextStudent, month, reset=false) {
  const hash=routeHash(nextPage,nextStudent,month);
  const history=window.history;
  if(!reset&&hash===window.location.hash)return;
  const url=window.location.pathname+window.location.search+hash;
  if(reset)history.replaceState({iskurDepth:0},'',url);
  else history.pushState({iskurDepth:currentRoute().depth+1},'',url);
  setRoute(currentRoute());
 }
 function navigate(nextPage, nextStudent=selected, month=route.month) {
  setError('');
  setMessage('');
  writeRoute(nextPage,nextStudent,month);
 }
 function clearSession() {
  setUser(null);
  setStudents([]);
  setScopeStudents([]);
  setToken(null);
 }
 useEffect(()=>{
  const historyChanged=()=>{setRoute(currentRoute());setError('');setMessage('');};
  window.addEventListener('session-expired',clearSession);
  window.addEventListener('popstate',historyChanged);
  window.addEventListener('hashchange',historyChanged);
  return ()=>{
   window.removeEventListener('session-expired',clearSession);
   window.removeEventListener('popstate',historyChanged);
   window.removeEventListener('hashchange',historyChanged);
  };
 },[]);
 async function loadStudents(f=filters){
  const q=new URLSearchParams(Object.entries(f).filter(([,v])=>v!==''));
  const rows=await api('students?'+q);
  setStudents(rows);
  if(!q.size)setScopeStudents(rows);
  return rows;
 }
 async function login(e){
  e.preventDefault();
  const form=new FormData(e.target);
  await run(async()=>{
   const auth=await api('auth/login','POST',{email:form.get('email'),password:form.get('password')});
   setToken(auth.accessToken);
   const [me,lookups,rows]=await Promise.all([api('me'),api('lookups'),loadStudents({})]);
   setLook(lookups);
   const target=currentRoute();
   const screens=menuFor(me.roles);
   const nextPage=screens.some(([id])=>id===target.page)?target.page:screens[0][0];
   const nextStudent=rows.some(row=>row.id===target.selected)?target.selected:me.roles.includes('Student')?defaultStudent(rows):null;
   writeRoute(nextPage,nextStudent,target.month,true);
   setUser(me);
  });
 }
 if(!user)return <main className="login"><section className="intro"><div className="brand">İŞKUR <span>ÖĞRENCİ TAKİBİ</span></div><h1>Çalışma hayatına<br/>düzenli bir başlangıç.</h1><p>Öğrenci bilgileri, aylık çalışma planları ve danışman değerlendirmeleri tek yerde.</p><div className="introfoot">TEKNOLOJİ FAKÜLTESİ</div></section><section className="loginform"><div className="eyebrow">ÖĞRENCİ ÇALIŞMA TAKİP SİSTEMİ</div><h2>Hoş geldiniz</h2><p>Size tanımlanan hesapla giriş yapın.</p><form onSubmit={login}><Field label="E-posta" name="email" type="email" placeholder="ogrenci1@demo.local" required/><Field label="Parola" name="password" type="password" autoComplete="current-password" required/>{error&&<div role="alert" className="error">{error}</div>}<button disabled={busy}>{busy?'Giriş yapılıyor…':'Giriş yap →'}</button></form><small>Örnek hesaplar ve kurulum bilgileri README dosyasındadır.</small></section></main>;
 return <div className="layout"><aside><div className="brand">İŞKUR <span>ÖĞRENCİ TAKİBİ</span></div><div className="portal">{roleNames[user.roles[0]]} PORTALI</div><nav>{menu.map(([id,label])=><button className={page===id?'active':''} key={id} aria-current={page===id?'page':undefined} onClick={()=>navigate(id)}>{label}</button>)}</nav><div className="asidefoot">Teknoloji Fakültesi<br/>2026–2027 Akademik Yılı</div></aside><div className="workspace"><header><div>{route.depth>0&&<button type="button" className="ghost back-button" onClick={()=>window.history.back()}>← Geri dön</button>}<span>İŞKUR / ÖĞRENCİ ÇALIŞMA TAKİBİ</span></div><div>{user.fullName}<button className="ghost" onClick={()=>{clearSession();setError('');setMessage('');writeRoute(null,null,null,true)}}>Çıkış</button></div></header><main><div className="eyebrow">{roleNames[user.roles[0]]}</div><h1>{menu.find(x=>x[0]===page)?.[1]}</h1>{error&&<div role="alert" className="error">{error}</div>}{message&&<div role="status" className="success">{message}</div>}
 {((!student&&(needsStudent||page==='reports'))||(student&&scopeStudents.length>1))&&<label className="selector">Seçili öğrenci<select value={selected||''} onChange={e=>navigate(page,Number(e.target.value)||null)}><option value="">Öğrenci seçin</option>{scopeStudents.map(s=><option value={s.id} key={s.id}>{s.fullName} · {s.studentNumber} · {s.start?.slice(0,4)} ({s.active?'aktif':'pasif'})</option>)}</select></label>}
 {page==='students'&&<><div className="stats"><article><span>Görüntülenen öğrenci</span><strong>{students.length}</strong></article><article><span>Erişim kapsamı</span><strong className="small">{management?'Fakültem':head?'Çalışma bölümüm':'Danışmanı olduğum öğrenciler'}</strong></article><article><span>Dönem</span><strong className="small">2026–2027</strong></article></div><section className="card"><h2>Öğrenci listesi</h2><form className="filters" onSubmit={e=>{e.preventDefault();run(()=>loadStudents())}}><Field label="Ad veya öğrenci numarası" value={filters.search} onChange={e=>setFilters({...filters,search:e.target.value})}/><label>Eğitim bölümü<select value={filters.departmentId} onChange={e=>setFilters({...filters,departmentId:e.target.value})}><option value="">Tümü</option>{look.departments.map(x=><option key={x.id} value={x.id}>{x.name}</option>)}</select></label><label>Çalışma birimi<select value={filters.workUnitId} onChange={e=>setFilters({...filters,workUnitId:e.target.value})}><option value="">Tümü</option>{look.workUnits.map(x=><option key={x.id} value={x.id}>{x.name}</option>)}</select></label><label>Program durumu<select value={filters.active} onChange={e=>setFilters({...filters,active:e.target.value})}><option value="">Tümü</option><option value="true">Aktif</option><option value="false">Pasif</option></select></label><label>Danışman<select value={filters.advisorId} onChange={e=>setFilters({...filters,advisorId:e.target.value})}><option value="">Tümü</option>{look.advisors.map(x=><option key={x.id} value={x.id}>{x.fullName}</option>)}</select></label><Field label="Plan yılı" type="number" min="2020" max="2100" value={filters.year} onChange={e=>setFilters({...filters,year:e.target.value})}/><Field label="Plan ayı" type="number" min="1" max="12" value={filters.month} onChange={e=>setFilters({...filters,month:e.target.value})}/><button disabled={busy||(!filters.year&&!!filters.month)}>Filtrele</button><button type="button" className="ghost" onClick={()=>{setFilters({search:'',departmentId:'',workUnitId:'',active:'',advisorId:'',year:'',month:''});run(()=>loadStudents({}))}}>Temizle</button></form><div className="tablewrap"><table><thead><tr><th>Öğrenci</th><th>Eğitim bölümü</th><th>Çalışma birimi</th><th>Danışman</th><th>Durum</th><th>İşlem</th></tr></thead><tbody>{students.map(s=><tr key={s.id}><td><b>{s.fullName}</b><small>{s.studentNumber}</small></td><td>{s.department}</td><td>{s.assignment?.unit||'Atanmadı'}</td><td>{s.assignment?.advisor||'Atanmadı'}</td><td><span className="badge">{s.active?'Aktif':'Pasif'}</span></td><td><button className="ghost" onClick={()=>navigate('profile',s.id)}>İncele →</button></td></tr>)}</tbody></table>{students.length===0&&<p>Filtrelerinize uyan öğrenci bulunamadı.</p>}</div></section></>}
 {page==='profile'&&selected&&<Profile key={selected} id={selected} run={run} look={look} management={management} student={student} head={head} busy={busy}/>}
 {page==='profile'&&selected&&management&&<NewParticipation key={'participation-'+selected} id={selected} look={look} run={run} busy={busy} onCreated={async id=>{await loadStudents({});navigate('profile',id)}}/>}
 {page==='courses'&&selected&&<Courses key={selected} id={selected} run={run} busy={busy}/>}
 {page==='plan'&&selected&&<Plan key={selected+'-'+planMonth} id={selected} month={planMonth} onMonthChange={month=>navigate('plan',selected,month)} run={run} student={student} review={review} busy={busy}/>}
 {page==='new'&&management&&<NewStudent look={look} run={run} busy={busy} onCreated={async id=>{await loadStudents({});navigate('profile',id)}}/>}
 {page==='holidays'&&management&&<Holidays run={run} busy={busy}/>}
 {page==='attendance'&&selected&&<Attendance key={selected} id={selected} student={student} management={management} advisor={advisor} head={head} run={run} busy={busy}/>}
 {page==='leaves'&&selected&&<Leaves key={selected} id={selected} student={student} management={management} run={run} busy={busy}/>}
 {page==='tasks'&&selected&&<Tasks key={selected} id={selected} advisor={advisor} run={run} busy={busy}/>}
 {page==='qr'&&!student&&<QrDisplay units={look.qrUnits??look.workUnits}/>}
 {page==='reports'&&<Reports id={selected} student={student} management={management} canExport={management||head} look={look} run={run} busy={busy}/>}
 {page==='settings'&&management&&<Settings run={run} busy={busy}/>}
 {page==='administration'&&management&&<Administration run={run} busy={busy} refreshLookups={async()=>setLook(await api('lookups'))}/>}
 {!selected&&needsStudent&&<p>Devam etmek için listeden bir öğrenci seçin.</p>}
 </main><footer>İŞKUR Öğrenci Çalışma Takip Sistemi</footer></div></div>;
}
function Profile({id,run,look,management,student,head,busy}){
 const [p,setP]=useState(null);const load=async()=>setP(await api('students/'+id));useEffect(()=>{run(load)},[id]);if(!p)return <p>Profil yükleniyor…</p>;
 const edit=(name,value)=>setP({...p,[name]:value});
 return <><section className="card"><h2>{p.fullName}</h2><p>{p.studentNumber} · {p.department}</p><form onSubmit={e=>{e.preventDefault();run(async()=>{await api('students/'+id,'PUT',p);await load()},'Profil kaydedildi.')}}><div className="grid2">{[['fullName','Ad soyad'],['email','E-posta'],['phone','Telefon'],['nationalId','T.C. kimlik numarası'],['iban','IBAN']].filter(([k])=>management||student||!['nationalId','iban'].includes(k)).map(([k,label])=><Field key={k} label={label} value={p[k]||''} required type={k==='email'?'email':'text'} maxLength={k==='nationalId'?11:k==='iban'?26:100} disabled={!(management||student)||student&&['email','nationalId'].includes(k)} onChange={e=>edit(k,k==='iban'?e.target.value.toUpperCase().replaceAll(' ',''):e.target.value)}/>)}</div><p>Kayıt yılı: {p.registrationYear} · Program: {p.start} – {p.end}</p>{(management||student)&&<button disabled={busy}>Profili kaydet</button>}</form></section><section className="card"><h2>Çalışma birimi ve danışman</h2><p>{p.assignment?.unit||'Atanmadı'} · {p.assignment?.advisor||'Danışman atanmadı'}</p>{(management||head)&&<form onSubmit={e=>{e.preventDefault();const f=new FormData(e.target);run(async()=>{await api(`students/${id}/assignment`,'PUT',{workUnitId:Number(f.get('unit')),advisorId:f.get('advisor')||null});await load()},'Atama kaydedildi; önceki atama geçmişte korundu.')}}><div className="grid2"><label>Çalışma birimi<select name="unit" defaultValue={p.assignment?.workUnitId} disabled={!management}>{look.workUnits.map(x=><option value={x.id} key={x.id}>{x.name}</option>)}</select>{!management&&<input type="hidden" name="unit" value={p.assignment?.workUnitId||''}/>}</label><label>Danışman<select name="advisor" defaultValue={p.assignment?.advisorId||''}><option value="">Atanmadı</option>{look.advisors.map(x=><option key={x.id} value={x.id}>{x.fullName}</option>)}</select></label></div><button disabled={busy}>Atamayı güncelle</button></form>}</section></>;
}
function Courses({id,run,busy}){
 const [rows,setRows]=useState([]);useEffect(()=>{run(async()=>setRows(await api(`students/${id}/courses`)))},[id]);
 function change(i,k,v){setRows(rows.map((r,n)=>n===i?{...r,[k]:v}:r))}
 const min=s=>Number(s.slice(0,2))*60+Number(s.slice(3));
 return <section className="card"><h2>Haftalık ders programı</h2><p>Dersleri elle girin. Çalışma planındaki çakışmalar saat aralıklarına göre hesaplanır. Gönderilmiş plan varsa değişiklikten önce geri çekin.</p><form onSubmit={e=>{e.preventDefault();run(()=>api(`students/${id}/courses`,'PUT',rows),'Ders programı kaydedildi.')}}>{rows.map((r,i)=><div className="course" key={i}><Field label="Ders adı" value={r.name} required onChange={e=>change(i,'name',e.target.value)}/><label>Gün<select value={r.dayOfWeek} onChange={e=>change(i,'dayOfWeek',Number(e.target.value))}>{[1,2,3,4,5].map(d=><option key={d} value={d}>{weekdays[d]}</option>)}</select></label><Field label="Başlangıç" type="time" value={formatCourseTime(r.startMinute)} required onChange={e=>change(i,'startMinute',min(e.target.value))}/><Field label="Bitiş" type="time" value={formatCourseTime(r.endMinute)} required onChange={e=>change(i,'endMinute',min(e.target.value))}/><button className="ghost" type="button" onClick={()=>setRows(rows.filter((_,n)=>n!==i))}>Sil</button></div>)}<div className="actions"><button type="button" className="ghost" onClick={()=>setRows([...rows,{id:0,name:'',dayOfWeek:1,startMinute:540,endMinute:600}])}>+ Ders ekle</button><button disabled={busy}>Dersleri kaydet</button></div></form></section>;
}
function Plan({id,month,onMonthChange,run,student,review,busy}){
 const [plan,setPlan]=useState({status:'Draft',days:[]}),[holidays,setHolidays]=useState([]),[edit,setEdit]=useState(null),[courses,setCourses]=useState([]),[ready,setReady]=useState(false);
 const [y,m]=month.split('-').map(Number);const path=`students/${id}/plans/${y}/${m}`;
 async function load(){setReady(false);const [p,h,c]=await Promise.all([api(path),api('holidays?year='+y),api(`students/${id}/courses`)]);setPlan(p);setHolidays(h);setCourses(c);setEdit(null);setReady(true)}
 useEffect(()=>{run(load)},[id,month]);const today=localToday();
 const withinWindow=plan.window?today>=plan.window.opens&&today<=plan.window.closes:month>=today.slice(0,7);
 const editable=student&&plan.status==='Draft'&&ready&&withinWindow;
 function conflict(date,kind){const dow=new Date(date+'T12:00:00').getDay();const windows=kind==='Morning'?[[480,720]]:kind==='Afternoon'?[[810,1020]]:[[480,720],[810,1020]];return courses.some(c=>c.dayOfWeek===dow&&windows.some(([s,e])=>c.startMinute<e&&c.endMinute>s));}
 async function save(submit){await run(async()=>{const p=await api(path,'PUT',{days:plan.days.map(d=>({date:d.date,kind:d.kind,note:d.note})),submit,version:plan.version,examDates:plan.examDates||[]});setPlan(p);setEdit(null)},submit?'Plan incelemeye gönderildi.':'Taslak kaydedildi.');}
 function reviewDay(day, decision) {
  const reason=decision==='Rejected'?window.prompt('Ret gerekçesini yazın.',day.reason||''):null;
  if(decision==='Rejected'&&reason===null)return;

  run(async()=>{
   await api(`plan-days/${day.id}/decision`,'PUT',{decision,reason,version:plan.version});
   await load();
  },decision==='Accepted'?'Çalışma günü kabul edildi.':'Çalışma günü reddedildi.');
 }
 const days=Array.from({length:new Date(y,m,0).getDate()},(_,i)=>`${month}-${String(i+1).padStart(2,'0')}`),offset=(new Date(y,m-1,1).getDay()+6)%7;
 return <><section className="card"><div className="sectionhead"><div><h2>Aylık çalışma takvimi</h2><p>Her tarih ayrı değerlendirilir. Öğle arası: 12.00–13.30.</p></div><Field label="Ay / yıl" type="month" value={month} min="2020-01" max="2100-12" onChange={e=>{if(e.target.value)onMonthChange(e.target.value)}}/></div><div className="planmeta"><span className="badge">{states[plan.status]}</span><span>{plan.days.length} gün seçildi</span>{plan.updatedAt&&<span>Son değişiklik: {new Date(plan.updatedAt).toLocaleString('tr-TR')}</span>}<span>{(plan.days.reduce((a,d)=>a+(d.kind==='Full'?450:d.kind==='Morning'?240:210),0)/60).toLocaleString('tr-TR')} saat planlandı</span></div><p className="muted">Çalışmaya uygun haftalarda 1–3 gün seçin. Yarım gün bir tarih sayılır. Hafta iki aya yayılıyorsa sonraki aya düşen çalışma günlerini o ayın planında seçebilirsiniz.</p>{plan.window&&<p>Düzenleme aralığı {plan.window.opens} – {plan.window.closes}</p>}{student&&ready&&!withinWindow&&<p className="status-warning">Bu ayın düzenleme aralığı kapalı.</p>}<div className="calendar-legend" aria-label="Takvim renk açıklamaları"><span className="legend-green">Yeşil: Seçilmiş, çakışmasız gün</span><span className="legend-red">Kırmızı: Tatil / hafta sonu</span><span className="legend-yellow">Sarı etiket: Ders saati</span><span className="legend-gray">Gri: Çalışma seçilmemiş gün</span></div>
 <div className="calendar-scroll"><div className="calendar">{['Pzt','Sal','Çar','Per','Cum','Cmt','Paz'].map(x=><b className="dayhead" key={x}>{x}</b>)}
 {Array.from({length:offset},(_,i)=><div key={'empty'+i}/>)}
 {days.map(date=>{
  const dateObj=new Date(date+'T12:00:00');
  const dow=dateObj.getDay();
  const holiday=holidays.find(h=>h.date===date);
  const weekend=dow===0||dow===6;
  const closed=weekend||Boolean(holiday&&!holiday.afternoonOnly);
  const day=plan.days.find(d=>d.date===date);
  const clash=Boolean(day&&conflict(date,day.kind));
  const dayCourses=closed?[]:courses.filter(c=>c.dayOfWeek===dow).sort((a,b)=>a.startMinute-b.startMinute);
  const exam=(plan.examDates||[]).includes(date);
  const tone=calendarTone({weekend,holiday,selected:Boolean(day),clash});
  const summary=day?kinds[day.kind].split(' · ')[0]:exam?'Sınav günü':null;
  const status=clash?'Ders çakışması':day?states[day.decision||'Pending']:null;
  const description=[date,holiday?.name||(weekend?'Hafta sonu':null),summary,status,
   ...dayCourses.map(course=>`${course.name} ${courseTimeRange(course)}`)].filter(Boolean).join(', ');
  return <button type="button" className={`day ${tone} ${edit===date?'editing':''}`} disabled={!editable||closed}
   key={date} aria-label={description} onClick={()=>setEdit(date)}>
   <strong>{dateObj.getDate()}</strong>
   {dayCourses.length>0&&<span className="day-courses">
    {dayCourses.map(course=><span className="day-course" key={course.id} title={course.name}>{courseTimeRange(course)}</span>)}
   </span>}
   {holiday&&<span>{holiday.name}</span>}
   {weekend&&!holiday&&<span>Hafta sonu</span>}
   {summary&&<span>{summary}</span>}
   {status&&<small>{status}</small>}
   {!closed&&!holiday&&!dayCourses.length&&!summary&&<span>Henüz seçilmedi</span>}
  </button>;
 })}
 </div></div>{editable&&<div className="actions"><button disabled={busy} className="ghost" onClick={()=>save(false)}>Taslağı kaydet</button><button disabled={busy} onClick={()=>save(true)}>İncelemeye gönder →</button></div>}{student&&withinWindow&&plan.status==='Submitted'&&<button className="ghost" disabled={busy} onClick={()=>{if(window.confirm('Plan taslağa dönecek ve bütün gün kararları sıfırlanacak. Devam edilsin mi?'))run(async()=>setPlan(await api(path+'/withdraw','POST',plan.version)),'Plan geri çekildi.')}}>Planı geri çek ve düzenle</button>}</section>
 {edit&&editable&&<section className="card"><h2>{edit} günü</h2><label className="check"><input type="checkbox" checked={(plan.examDates||[]).includes(edit)}
 onChange={e=>setPlan({...plan,days:e.target.checked?plan.days.filter(d=>d.date!==edit):plan.days,examDates:e.target.checked?[...(plan.examDates||[]),edit]:(plan.examDates||[]).filter(d=>d!==edit)})}/>Sınav nedeniyle plan dışında bırak</label><label>Çalışma türü<select disabled={(plan.examDates||[]).includes(edit)} value={plan.days.find(d=>d.date===edit)?.kind||''} onChange={e=>{const rest=plan.days.filter(d=>d.date!==edit);setPlan({...plan,days:e.target.value?[...rest,{date:edit,kind:e.target.value,note:'',decision:'Pending'}]:rest})}}><option value="">Çalışma yok</option>{Object.entries(kinds).map(([k,v])=><option key={k} value={k} disabled={holidays.some(h=>h.date===edit&&h.afternoonOnly)&&k!=='Morning'}>{v}</option>)}</select></label>{plan.days.some(d=>d.date===edit)&&<Field label={conflict(edit,plan.days.find(d=>d.date===edit)?.kind)?"Ders çakışması açıklaması (gönderimde zorunlu)":"İsteğe bağlı açıklama"} maxLength={500} value={plan.days.find(d=>d.date===edit)?.note||''} onChange={e=>setPlan({...plan,days:plan.days.map(d=>d.date===edit?{...d,note:e.target.value}:d)})}/>}<button className="ghost" onClick={()=>setEdit(null)}>Tamam</button></section>}
 <section className="card"><h2>Günlerin değerlendirme durumu</h2><p>Çakışma olmaması otomatik kabul değildir. Kabul edilen günler çalışma takviminizi oluşturur.</p><div className="tablewrap"><table><thead><tr><th>Tarih</th><th>Çalışma</th><th>Ders kontrolü</th><th>Karar</th><th>Gerekçe / İşlem</th></tr></thead><tbody>{plan.days.slice().sort((a,b)=>a.date.localeCompare(b.date)).map(d=><tr key={d.date}><td>{d.date}</td><td>{kinds[d.kind]}</td><td>{conflict(d.date,d.kind)?'Çakışma var':'Çakışma yok'}</td><td><span className={'badge '+d.decision}>{states[d.decision||'Pending']}</span></td><td>{d.reason}{review&&plan.status==='Submitted'&&<div className="actions">{[['Accepted','Kabul et'],['Rejected','Reddet']].map(([decision,label])=><button disabled={busy} className="ghost" key={decision} onClick={()=>reviewDay(d,decision)}>{label}</button>)}</div>}</td></tr>)}</tbody></table>{!plan.days.length&&<p>Bu ay için henüz çalışma günü seçilmemiş.</p>}</div></section></>;
}
function NewStudent({look,run,onCreated,busy}){return <section className="card"><h2>Programa kabul edilen öğrenciyi kaydet</h2><p>İlk çalışma birimi “Atama bekliyor” olur. Kayıttan sonra birim ve danışman atayın.</p><form onSubmit={e=>{e.preventDefault();const f=Object.fromEntries(new FormData(e.target));run(async()=>{const r=await api('students','POST',{profile:{fullName:f.fullName,email:f.email,phone:f.phone,nationalId:f.nationalId,iban:f.iban.toUpperCase().replaceAll(' ','')},studentNumber:f.studentNumber,departmentId:Number(f.departmentId),registrationYear:Number(f.registrationYear),programPeriodId:Number(f.programPeriodId),password:f.password});await onCreated(r.id)},'Öğrenci oluşturuldu.')}}><div className="grid2">{[['fullName','Ad soyad','text'],['email','E-posta','email'],['phone','Telefon','tel'],['studentNumber','Öğrenci numarası','text'],['nationalId','T.C. kimlik numarası','text'],['iban','IBAN','text'],['registrationYear','Kayıt yılı','number'],['password','İlk parola','password']].map(([name,label,type])=><Field key={name} name={name} label={label} type={type} required defaultValue={name==='registrationYear'?2026:undefined} minLength={name==='password'?10:undefined}/>)}<label>Eğitim bölümü<select name="departmentId" required>{look.departments.map(x=><option key={x.id} value={x.id}>{x.name}</option>)}</select></label><label>Program dönemi<select name="programPeriodId" required>{look.periods.map(x=><option key={x.id} value={x.id}>{x.name}</option>)}</select></label></div><p>İlk parola en az 10 karakter, büyük/küçük harf, sayı ve özel karakter içermeli.</p><button disabled={busy}>Öğrenciyi oluştur</button></form></section>}
function Holidays({run,busy}){const [year,setYear]=useState(2026),[rows,setRows]=useState([]);async function load(){setRows(await api('holidays?year='+year))}useEffect(()=>{run(load)},[year]);return <section className="card"><h2>Resmî tatil tanımları</h2><p>Başlangıç verilerinde sabit ulusal tarihler vardır. Dinî bayramlar ve kurumun ek tatillerini plan gönderiminden önce buraya ekleyin. Yarım tatillerde yalnız sabah seçilebilir.</p><Field label="Yıl" type="number" min={2020} max={2100} value={year} onChange={e=>setYear(Number(e.target.value))}/><form className="filters" onSubmit={e=>{e.preventDefault();const f=new FormData(e.target);run(async()=>{await api('holidays','PUT',{date:f.get('date'),name:f.get('name'),afternoonOnly:f.get('half')==='on'});await load()},'Tatil kaydedildi.')}}><Field label="Tarih" name="date" type="date" required/><Field label="Tatil adı" name="name" required/><label className="check"><input type="checkbox" name="half"/>Yalnız öğleden sonra tatil</label><button disabled={busy}>Ekle / güncelle</button></form><ul>{rows.map(x=><li key={x.date}>{x.date} · {x.name} {x.afternoonOnly?'(öğleden sonra)':''}</li>)}</ul></section>}
createRoot(document.getElementById('root')).render(<App/>);
