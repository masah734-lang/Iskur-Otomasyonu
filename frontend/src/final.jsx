import React, { useEffect, useRef, useState } from 'react';
import QRCode from 'qrcode';
import jsQR from 'jsqr';
import { api, download } from './api';

export const localToday = () => new Intl.DateTimeFormat('sv-SE', {
 timeZone: 'Europe/Istanbul', year: 'numeric', month: '2-digit', day: '2-digit'
}).format(new Date());

const statusNames = {
 Pending: 'Bekliyor', Approved: 'Onaylandı', Rejected: 'Reddedildi', Cancelled: 'İptal edildi',
 Present: 'Geldi', Absent: 'Gelmedi', OnLeave: 'İzinli',
 Reported: 'Bölüm başkanına iletildi', Forwarded: 'Fakülteye iletildi', Closed: 'Kapatıldı',
 Terminated: 'Katılım sonlandırıldı', Morning: 'Sabah', Afternoon: 'Öğleden sonra',
 Created: 'Atandı', Status: 'Durum değişti', Note: 'Not eklendi'
};

function dateTime(value) {
 return value ? new Date(value).toLocaleString('tr-TR', { timeZone: 'Europe/Istanbul' }) : '—';
}

function duration(minutes) {
 return new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2 }).format(minutes / 60) + ' saat';
}

function Field({ label, ...props }) {
 return <label>{label}<input {...props}/></label>;
}

function Empty({ children }) {
 return <p className="muted">{children}</p>;
}

function DataTable({ columns, children, empty }) {
 return <div className="tablewrap"><table><thead><tr>
  {columns.map(label => <th key={label} scope="col">{label}</th>)}
 </tr></thead><tbody>{children}</tbody></table>{!children.length && <Empty>{empty}</Empty>}</div>;
}

export function Leaves({ id, student, management, run, busy }) {
 const [data, setData] = useState(null);
 const path = `students/${id}/leaves`;
 const load = async () => setData(await api(path));
 useEffect(() => { run(load); }, [id]);
 const decide = (row, status) => {
  const note = status === 'Rejected' ? window.prompt('Ret gerekçesi') : null;
  if (status === 'Rejected' && !note?.trim()) return;
  run(async () => { await api(`${path}/${row.id}/decision`, 'POST', { status, note }); await load(); }, 'İzin talebi güncellendi.');
 };

 return <>
  {data && <div className="stats leave-stats">
   {[['Toplam hak', data.entitlement], ['Kullanılan', data.used], ['Onay bekleyen', data.pending], ['Kalan hak', data.remaining]]
    .map(([label, value]) => <article key={label}><span>{label}</span><strong>{value} gün</strong></article>)}
  </div>}
  {data?.warning && <p role="status" className="status-warning">İzin hakkı uyarı eşiğinde. Bekleyen talepler kalan hakkı azaltabilir.</p>}
  {student && <section className="card"><h2>İzin talebi oluştur</h2>
   <p>İzin süresi seçilen aralıktaki onaylı çalışma oturumlarından hesaplanır.</p>
   <form onSubmit={event => {
    event.preventDefault();
    const form = event.currentTarget;
    const input = new FormData(form);
    run(async () => { await api(path, 'POST', input); form.reset(); await load(); }, 'İzin talebi gönderildi.');
   }}>
    <div className="grid2">
     <Field label="Başlangıç" name="start" type="date" required/>
     <Field label="Bitiş" name="end" type="date" required/>
     <label>Tür<select name="type"><option value="Personal">Mazeret</option><option value="Medical">Sağlık raporu</option></select></label>
     <Field label="Belge (PDF, JPG veya PNG, 10 MB)" name="file" type="file" accept="application/pdf,image/jpeg,image/png"/>
    </div>
    <label>Gerekçe<textarea name="reason" rows="3" maxLength={1000} required/></label>
    <button disabled={busy}>Talebi gönder</button>
   </form>
  </section>}
  <section className="card"><h2>İzin talepleri</h2>
   <DataTable columns={['Tarih aralığı', 'Tür ve gerekçe', 'Süre', 'Durum ve geçmiş', 'İşlem']} empty="Henüz izin talebi yok.">
    {(data?.rows || []).map(row => <tr key={row.id}>
     <td>{row.start} – {row.end}</td>
     <td>{row.type === 'Medical' ? 'Sağlık raporu' : 'Mazeret'}<small>{row.reason}</small></td>
     <td>{row.days} gün haktan · {duration(row.minutes)}</td>
     <td><span className="badge">{statusNames[row.status]}</span>
      {row.reviewer && <small>{row.reviewer} · {dateTime(row.reviewedAt)}</small>}
      <details><summary>Geçmiş</summary><ul>{row.history.map((item, index) =>
       <li key={index}>{dateTime(item.recordedAt)} · {statusNames[item.status]} {item.note}</li>)}</ul></details>
     </td>
     <td><div className="actions">
      {row.hasFile && <button type="button" className="ghost" disabled={busy} onClick={() =>
       run(() => download(`${path}/${row.id}/attachment`, 'izin-belgesi'))}>Belgeyi indir</button>}
      {management && row.status === 'Pending' && <>
       <button disabled={busy} onClick={() => decide(row, 'Approved')}>Onayla</button>
       <button className="ghost" disabled={busy} onClick={() => decide(row, 'Rejected')}>Reddet</button>
      </>}
      {student && row.status === 'Pending' && <button className="ghost" disabled={busy} onClick={() => decide(row, 'Cancelled')}>İptal et</button>}
     </div></td>
    </tr>)}
   </DataTable>
  </section>
 </>;
}

function decodeImage(source) {
 const canvas = document.createElement('canvas');
 const width = source.videoWidth || source.naturalWidth || source.width;
 const height = source.videoHeight || source.naturalHeight || source.height;
 if (!width || !height) return null;
 const scale = Math.min(1, 960 / Math.max(width, height));
 canvas.width = Math.max(1, Math.round(width * scale));
 canvas.height = Math.max(1, Math.round(height * scale));
 const context = canvas.getContext('2d', { willReadFrequently: true });
 context.drawImage(source, 0, 0, canvas.width, canvas.height);
 const pixels = context.getImageData(0, 0, canvas.width, canvas.height);
 return jsQR(pixels.data, pixels.width, pixels.height, { inversionAttempts: 'dontInvert' })?.data;
}

function Scanner({ onCode }) {
 const video = useRef(null);
 const [enabled, setEnabled] = useState(false);
 const [error, setError] = useState('');
 useEffect(() => {
  if (!enabled) return;
  let stream;
  let timer;
  let cancelled = false;
  async function start() {
   try {
    if (!navigator.mediaDevices?.getUserMedia) throw new Error('Kamera için güvenli bağlantı gerekir.');
    stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } } });
    if (cancelled) { stream.getTracks().forEach(track => track.stop()); return; }
    video.current.srcObject = stream;
    await video.current.play();
    if (cancelled) return;
    timer = setInterval(() => {
     if (!video.current) return;
     const code = decodeImage(video.current);
     if (code) { setEnabled(false); onCode(code); }
    }, 250);
   } catch (reason) {
    if (!cancelled) { setError(reason.message); setEnabled(false); }
   }
  }
  start();
  return () => { cancelled = true; clearInterval(timer); stream?.getTracks().forEach(track => track.stop()); };
 }, [enabled]);

 async function fileCode(file) {
  if (!file) return;
  setError('');
  try {
   const bitmap = await createImageBitmap(file);
   try {
    const code = decodeImage(bitmap);
    if (!code) throw new Error('Görselde okunabilir bir karekod bulunamadı.');
    onCode(code);
   } finally { bitmap.close(); }
  } catch (reason) { setError(reason.message); }
 }

 return <>
  <div className="actions">
   <button type="button" className="ghost" onClick={() => { setError(''); setEnabled(!enabled); }}>
    {enabled ? 'Kamerayı kapat' : 'Kamerayı aç'}
   </button>
   <Field label="Karekod görseli" type="file" accept="image/*" onChange={event => {
    fileCode(event.target.files?.[0]); event.target.value = '';
   }}/>
  </div>
  {enabled && <video className="qr-video" ref={video} muted playsInline aria-label="Karekod tarama kamerası"/>}
  {error && <p role="alert" className="error">{error}</p>}
 </>;
}

export function QrDisplay({ units }) {
 const [unit, setUnit] = useState(() => units.length === 1 ? String(units[0].id) : '');
 const [code, setCode] = useState(null);
 const [error, setError] = useState('');
 const [active, setActive] = useState(false);

 useEffect(() => {
  if (!unit || !active) return;
  let cancelled = false;
  let timer;
  async function refresh() {
   try {
    const result = await api('attendance/qr?unitId=' + unit);
    const image = await QRCode.toDataURL(result.token, { width: 300, margin: 3 });
    if (cancelled) return;
    setCode({ ...result, image });
    setError('');
    timer = setTimeout(refresh, Math.max(250, Date.parse(result.expiresAt) - Date.now() + 100));
   } catch (reason) {
    if (cancelled) return;
    setCode(null);
    setError(reason.message);
    timer = setTimeout(refresh, 20000);
   }
  }
  refresh();
  return () => { cancelled = true; clearTimeout(timer); };
 }, [unit, active]);

 return <section className="card"><h2>Dinamik yoklama karekodu</h2>
  <p>Birimini seçip <strong>Karekodu göster</strong> düğmesine bas. Öğrencinin <strong>Yoklamam</strong> ekranındaki kamerayla okutabilmesi için bu sayfayı açık tut.</p>
  <p>Kod, Türkiye saatiyle <strong>08.00–08.20</strong> ve <strong>13.30–13.50</strong> arasında görüntülenir. Her 30 saniyede bir yenilenir.</p>
  {units.length===0&&<p className="status-warning">Bu hesapla karekod açabileceğin bir çalışma birimi bulunamadı.</p>}
  <div className="actions">
   <label>Çalışma birimi<select value={unit} onChange={event => { setCode(null); setError(''); setUnit(event.target.value); }}>
    <option value="">Birim seçin</option>{units.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}
   </select></label>
   <button disabled={!unit} onClick={() => { setActive(!active); setCode(null); setError(''); }}>
    {active ? 'Karekodu kapat' : 'Karekodu göster'}
   </button>
  </div>
  {error && <p role="status" className="status-warning">{error}</p>}
  {code && <div className="qr-result">
   <img width="300" height="300" src={code.image} alt={statusNames[code.slot] + ' yoklama karekodu'}/>
   <p>{statusNames[code.slot]} · {dateTime(code.expiresAt)} saatine kadar geçerli</p>
  </div>}
 </section>;
}

export function Attendance({ id, student, management, advisor, head, run, busy }) {
 const [month, setMonth] = useState(localToday().slice(0, 7));
 const [data, setData] = useState(null);
 const [reports, setReports] = useState([]);
 const [token, setToken] = useState('');
 const [scanned, setScanned] = useState(false);
 const [year, number] = month.split('-');
 const path = `students/${id}/attendance`;
 const reportPath = `students/${id}/absence-reports`;

 async function load() {
  const [attendance, list] = await Promise.all([
   api(`${path}?year=${year}&month=${Number(number)}`), student ? [] : api(reportPath)
  ]);
  setData(attendance);
  setReports(list);
 }
 useEffect(() => { run(load); }, [id, month]);

 function mark(row, status) {
  const reason = window.prompt(status === 'Report' ? 'Devamsızlık bildiriminin gerekçesi' : 'Yoklama düzeltmesinin gerekçesi');
  if (!reason?.trim()) return;
  run(async () => {
   if (status === 'Report') await api(reportPath, 'POST', { planDayId: row.planDayId, reason });
   else await api(`${path}/${row.planDayId}`, 'PUT', { slot: row.slot, status, reason });
   await load();
  }, status === 'Report' ? 'Devamsızlık bildirimi iletildi.' : 'Yoklama güncellendi.');
 }

 function review(row, status) {
  if (status === 'Terminated' && !window.confirm('Öğrencinin program katılımı bugün itibarıyla pasif olacak. Devam edilsin mi?')) return;
  const reason = window.prompt('Karar gerekçesi');
  if (!reason?.trim()) return;
  run(async () => { await api(`${reportPath}/${row.id}/decision`, 'POST', { status, reason }); await load(); }, 'Karar kaydedildi.');
 }

 return <>
  {student && <section className="card"><h2>Bugünkü yoklamaya katıl</h2>
   <ol className="qr-guide">
    <li>Danışmanın veya yetkili personel kendi hesabında <strong>Yoklama karekodu</strong> menüsünden çalışma birimin için kodu açar.</li>
    <li>Yetkilinin ekranındaki kodu kameraya göster veya <strong>Karekod görseli</strong> alanından görseli seç.</li>
    <li>Kod okunduğunda <strong>Yoklama ver</strong> düğmesine bas. Kodun süresi dolarsa güncel kodu yeniden okut.</li>
   </ol>
   <Scanner onCode={code=>{setToken(code);setScanned(true);}}/>
   {scanned&&<p role="status" className="success">Karekod okundu. Yoklamayı kaydetmek için Yoklama ver düğmesine bas.</p>}
   <form className="filters" onSubmit={event => {
    event.preventDefault();
    run(async () => {
     await api(path + '/check-in', 'POST', { token: token.trim() });
     setToken('');
     setScanned(false);
     const current = localToday().slice(0, 7);
     if (current === month) await load(); else setMonth(current);
    }, 'Yoklama kaydedildi.');
   }}>
    <Field label="Karekod içeriği" value={token} onChange={event => {setToken(event.target.value);setScanned(false);}} maxLength={2048} required/>
    <button disabled={busy || !token.trim()}>Yoklama ver</button>
   </form>
  </section>}
  <section className="card"><div className="sectionhead"><div><h2>Oturum kayıtları</h2>
   {data && <p>Planlanan {duration(data.plannedMinutes)} · Gerçekleşen {duration(data.actualMinutes)}</p>}</div>
   <Field label="Ay / yıl" type="month" value={month} onChange={event => { if (event.target.value) setMonth(event.target.value); }}/>
  </div>
   <DataTable columns={['Tarih', 'Oturum', 'Durum', 'İşlem']} empty="Bu ay onaylanmış çalışma oturumu yok.">
    {(data?.rows || []).map(row => <tr key={row.planDayId + row.slot}>
     <td>{row.date}</td><td>{statusNames[row.slot]} · {duration(row.minutes)}</td>
     <td><span className="badge">{statusNames[row.status]}</span>{row.recordedAt && <small>{dateTime(row.recordedAt)}</small>}</td>
     <td><div className="actions">
      {management && !['OnLeave', 'Pending'].includes(row.status) && <button className="ghost" disabled={busy}
       onClick={() => mark(row, row.status === 'Present' ? 'Absent' : 'Present')}>
       {row.status === 'Present' ? 'Gelmedi işaretle' : 'Geldi işaretle'}
      </button>}
      {advisor && row.status === 'Absent' && !reports.some(item => item.date === row.date) &&
       <button className="ghost" disabled={busy} onClick={() => mark(row, 'Report')}>Devamsızlığı bildir</button>}
     </div></td>
    </tr>)}
   </DataTable>
  </section>
  {!student && <section className="card"><h2>Devamsızlık bildirimleri</h2>
   <DataTable columns={['Tarih', 'Gerekçe', 'Durum ve geçmiş', 'İşlem']} empty="Devamsızlık bildirimi yok.">
    {reports.map(row => <tr key={row.id}>
     <td>{row.date}</td><td>{row.reason}</td><td>{statusNames[row.status]}<small>{row.resolution}</small>
      <details><summary>Geçmiş</summary><ul>{row.history.map((item, index) =>
       <li key={index}>{dateTime(item.recordedAt)} · {statusNames[item.status]} · {item.reason}</li>)}</ul></details></td>
     <td><div className="actions">
      {head && row.status === 'Reported' && <button disabled={busy} onClick={() => review(row, 'Forwarded')}>Fakülteye ilet</button>}
      {management && row.status === 'Forwarded' && <button disabled={busy} onClick={() => review(row, 'Terminated')}>Katılımı sonlandır</button>}
      {(head && row.status === 'Reported' || management && row.status === 'Forwarded') &&
       <button className="ghost" disabled={busy} onClick={() => review(row, 'Closed')}>Bildirimi kapat</button>}
     </div></td>
    </tr>)}
   </DataTable>
  </section>}
 </>;
}

export function Tasks({ id, advisor, run, busy }) {
 const [rows, setRows] = useState([]);
 const [completed, setCompleted] = useState('');
 const path = `students/${id}/tasks`;
 const load = async () => setRows(await api(path + (completed ? '?completed=' + completed : '')));
 useEffect(() => { run(load); }, [id, completed]);

 return <>
  {advisor && <section className="card"><h2>Görev ata</h2>
   <form onSubmit={event => {
    event.preventDefault();
    const form = event.currentTarget;
    const input = Object.fromEntries(new FormData(form));
    run(async () => { await api(path, 'POST', input); form.reset(); await load(); }, 'Görev atandı.');
   }}>
    <label>Görev açıklaması<textarea name="description" rows="3" maxLength={1000} required/></label>
    <Field label="Hedef tarih" name="dueDate" type="date" min={localToday()} required/>
    <button disabled={busy}>Görevi ata</button>
   </form>
  </section>}
  <section className="card"><div className="sectionhead"><h2>Görevler ve değerlendirmeler</h2>
   <label>Durum<select value={completed} onChange={event => setCompleted(event.target.value)}>
    <option value="">Tümü</option><option value="true">Tamamlandı</option><option value="false">Tamamlanmadı</option>
   </select></label>
  </div>
  <DataTable columns={['Görev', 'Hedef tarih', 'Durum', 'Değerlendirmeler']} empty="Filtreye uyan görev bulunamadı.">
   {rows.map(row => <tr key={row.id}>
    <td>{row.description}<small>{row.advisor}</small></td><td>{row.dueDate}</td>
    <td><span className="badge">{row.completed ? 'Tamamlandı' : 'Tamamlanmadı'}</span>
     <details><summary>Geçmiş</summary><ul>{row.history.map((item, index) =>
      <li key={index}>{dateTime(item.recordedAt)} · {statusNames[item.action]}
       {item.action === 'Status' && (item.completed ? ' · Tamamlandı' : ' · Tamamlanmadı')}</li>)}</ul></details></td>
    <td>{row.notes.length > 0 && <ul>{row.notes.map((note, index) =>
     <li key={index}>{note.note} <small>{note.author} · {dateTime(note.at)}</small></li>)}</ul>}
     {advisor && <form className="task-evaluation" onSubmit={event => {
      event.preventDefault();
      const form = event.currentTarget;
      const values = new FormData(form);
      run(async () => {
       await api(`${path}/${row.id}/evaluation`, 'POST', {
        completed: values.get('completed') === 'true', note: values.get('note')
       });
       form.reset();
       await load();
      }, 'Değerlendirme kaydedildi.');
     }}>
      <label>Durum<select key={String(row.completed)} name="completed" defaultValue={String(row.completed)}>
       <option value="false">Tamamlanmadı</option><option value="true">Tamamlandı</option>
      </select></label>
      <Field label="Not ekle" name="note" maxLength={1000}/>
      <button className="ghost" disabled={busy}>Kaydet</button>
     </form>}
    </td>
   </tr>)}
  </DataTable></section>
 </>;
}

function ReportResult({ result, empty, children }) {
 if (result.error) return <p className="error" role="alert">{result.error}</p>;
 if (!result.rows) return <p role="status">Rapor yükleniyor…</p>;
 if (result.rows.length === 0) return <p role="status">{empty}</p>;
 return <>
  <p role="status">{result.label ? result.label + ' · ' : ''}{result.rows.length} kayıt bulundu.</p>
  {children(result.rows)}
 </>;
}

export function Reports({ id, student, management, canExport, look, run, busy }) {
 const [work, setWork] = useState({});
 const [warnings, setWarnings] = useState({});
 const [tasks, setTasks] = useState({});
 const [year, setYear] = useState(localToday().slice(0, 4));
 const [month, setMonth] = useState(localToday().slice(5, 7));
 const form = useRef(null);
 const latestSearch = useRef(0);
 const monthName = number => new Date(2026, number - 1, 1).toLocaleDateString('tr-TR', { month: 'long' });

 function search() {
  const searchId = ++latestSearch.current;
  const input = Object.fromEntries(new FormData(form.current));
  const query = new URLSearchParams(Object.entries(input).filter(([, value]) => value));
  if (id) query.set('participationId', id);
  const tasksQuery = new URLSearchParams(query);
  tasksQuery.delete('attendanceStatus');
  query.delete('completed');
  const label = input.month ? monthName(Number(input.month)) + ' ' + input.year : input.year;

  async function load(endpoint, update, details = {}) {
   update({});
   try {
    const rows = await api(endpoint);
    if (!Array.isArray(rows)) throw new Error('Sunucudan beklenen rapor verisi gelmedi.');
    if (searchId === latestSearch.current) update({ rows, ...details });
   } catch (error) {
    if (searchId === latestSearch.current) update({ error: error.message });
   }
  }

  load('reports/work?' + query, setWork, { query: query.toString(), label });
  if (!student) load('reports/tasks?' + tasksQuery, setTasks, { label });
  if (management) load('reports/warnings', setWarnings);
 }

 useEffect(() => {
  search();
  return () => { latestSearch.current++; };
 }, [id]);

 const loadingWork = !work.rows && !work.error;

 return <>
  <section className="card"><h2>Çalışma dökümü</h2>
   <form ref={form} className="filters" onSubmit={event => { event.preventDefault(); search(); }}>
    <Field label="Yıl" name="year" type="number" min="2020" max="2100" value={year} onChange={e => setYear(e.target.value)} required/>
    <label>Ay<select name="month" value={Number(month) || ''} onChange={e => setMonth(e.target.value)}>
     <option value="">Yılın tamamı</option>{Array.from({ length: 12 }, (_, index) => <option key={index + 1} value={index + 1}>
      {monthName(index + 1)}</option>)}
    </select></label>
    {!student && <>
     <Filter label="Eğitim bölümü" name="departmentId" options={look.departments}/>
     <Filter label="Çalışma birimi" name="workUnitId" options={look.workUnits}/>
     <Filter label="Danışman" name="advisorId" options={look.advisors.map(a => ({ id: a.id, name: a.fullName }))}/>
     <Filter label="Program durumu" name="active" options={[{ id: 'true', name: 'Aktif' }, { id: 'false', name: 'Pasif' }]}/>
     <Filter label="Görev durumu" name="completed" options={[{ id: 'true', name: 'Tamamlandı' }, { id: 'false', name: 'Tamamlanmadı' }]}/>
    </>}
    <Filter label="Yoklama durumu" name="attendanceStatus" options={['Pending', 'Present', 'Absent', 'OnLeave'].map(id => ({ id, name: statusNames[id] }))}/>
    <button type="submit" disabled={busy || loadingWork}>{loadingWork ? 'Rapor getiriliyor…' : 'Raporu getir'}</button>
    {canExport && <button type="button" className="ghost" disabled={busy || !work.query} onClick={() =>
     run(() => download('reports/work?' + work.query + '&csv=true', 'Iskur-Calisma.csv'))}>CSV indir</button>}
   </form>
   <ReportResult result={work} empty={`${work.label} döneminde bu filtrelere uyan onaylı çalışma günü bulunamadı.`}>
    {rows => <DataTable columns={['Öğrenci', 'Ay', 'Görev yeri', 'Planlanan', 'Gerçekleşen', 'İzinli / devamsız gün']}>
    {rows.map((row, index) => <tr key={index}>
     <td><strong>{row.fullName}</strong><small>{row.studentNumber} · {row.department}</small>
      {management && <small>{row.nationalId} · {row.iban}</small>}</td>
     <td>{row.month}/{row.year}</td><td>{row.unit}<small>{row.advisor}</small></td>
     <td>{row.plannedDates.length} gün · {duration(row.plannedMinutes)}<small>{row.plannedDates.map(d => d.slice(8)).join(', ')}</small></td>
     <td>{row.actualDates.length} gün · {duration(row.actualMinutes)}<small>{row.actualDates.map(d => d.slice(8)).join(', ')}</small></td>
     <td>{row.leaveDays} / {row.absentDays}</td>
    </tr>)}
   </DataTable>}
   </ReportResult>
  </section>
  {!student && <section className="card"><h2>Görev dökümü</h2>
   <ReportResult result={tasks} empty="Bu filtrelerde görev bulunamadı.">
   {rows => <DataTable columns={['Öğrenci', 'Danışman', 'Görev', 'Hedef tarih', 'Durum', 'Not sayısı']}>
    {rows.map(row => <tr key={row.id}><td>{row.fullName}</td><td>{row.advisor}</td>
     <td>{row.description}</td><td>{row.dueDate}</td><td>{row.completed ? 'Tamamlandı' : 'Tamamlanmadı'}</td>
     <td>{row.assessmentCount}</td></tr>)}
   </DataTable>}
   </ReportResult>
  </section>}
  {management && <section className="card"><h2>İzin ve devamsızlık uyarıları</h2>
   <ReportResult result={warnings} empty="Eşik uyarısı bulunan öğrenci yok.">
   {rows => <DataTable columns={['Öğrenci', 'Kalan izin', 'Bekleyen izin', 'Devamsızlık', 'Uyarı']}>
    {rows.map(row => <tr key={row.id}>
     <td>{row.fullName}<small>{row.studentNumber}</small></td><td>{row.remaining} gün</td>
     <td>{row.pending} gün</td><td>{row.absent} gün</td>
     <td>{[row.leaveWarning && 'İzin eşiği', row.absenceWarning && 'Devamsızlık eşiği'].filter(Boolean).join(' · ')}</td>
    </tr>)}
   </DataTable>}
   </ReportResult>
  </section>}
 </>;
}

function Filter({ label, name, options }) {
 return <label>{label}<select name={name}><option value="">Tümü</option>
  {options.map(item => <option key={item.id} value={item.id}>{item.name}</option>)}
 </select></label>;
}

export function Settings({ run, busy }) {
 const [policy, setPolicy] = useState(null);
 const [windows, setWindows] = useState([]);
 async function load() {
  const [rules, months] = await Promise.all([api('settings/policy'), api('settings/plan-windows')]);
  setPolicy(rules);
  setWindows(months);
 }
 useEffect(() => { run(load); }, []);

 return <>
  {policy && <section className="card"><h2>İzin ve devamsızlık kuralları</h2>
   <form onSubmit={event => {
    event.preventDefault();
    run(async () => { await api('settings/policy', 'PUT', policy); await load(); }, 'Kurallar kaydedildi.');
   }}>
    <div className="grid2">{[
     ['leaveDays', 'Katılım başına izin hakkı', 0],
     ['leaveWarningDays', 'Kalan izin uyarı eşiği', 0],
     ['absenceWarningDays', 'Devamsızlık uyarı eşiği', 1]
    ].map(([key, label, min]) => <Field key={key} label={label} type="number" required min={min} max={365}
     value={policy[key]} onChange={event => setPolicy({ ...policy, [key]: Number(event.target.value) })}/>)}
    </div>
    <label className="check"><input type="checkbox" checked={policy.medicalLeaveExempt}
     onChange={event => setPolicy({ ...policy, medicalLeaveExempt: event.target.checked })}/>
     Sağlık raporlu izni haktan düşme</label>
    <button disabled={busy}>Kuralları kaydet</button>
   </form>
  </section>}
  <section className="card"><h2>Aylık plan düzenleme aralığı</h2>
   <form className="filters" onSubmit={event => {
    event.preventDefault();
    const values = new FormData(event.currentTarget);
    const [year, month] = values.get('month').split('-').map(Number);
    run(async () => {
     await api('settings/plan-windows', 'PUT', { year, month, opens: values.get('opens'), closes: values.get('closes') });
     await load();
    }, 'Plan aralığı kaydedildi.');
   }}>
    <Field label="Plan ayı" name="month" type="month" defaultValue={localToday().slice(0, 7)} required/>
    <Field label="Açılış" name="opens" type="date" required/>
    <Field label="Kapanış" name="closes" type="date" required/>
    <button disabled={busy}>Aralığı kaydet</button>
   </form>
   <DataTable columns={['Plan dönemi', 'Açılış', 'Kapanış']} empty="Özel düzenleme aralığı tanımlanmadı.">
    {windows.map(row => <tr key={row.year + '/' + row.month}><td>{row.month}/{row.year}</td>
     <td>{row.opens}</td><td>{row.closes}</td></tr>)}
   </DataTable>
  </section>
 </>;
}

export function Administration({ run, busy, refreshLookups }) {
 const [data, setData] = useState(null);
 const load = async () => setData(await api('administration'));
 useEffect(() => { run(load); }, []);

 function save(event, type) {
  event.preventDefault();
  const form = event.currentTarget;
  const input = Object.fromEntries(new FormData(form));
  if ('departmentId' in input) input.departmentId = Number(input.departmentId) || null;
  run(async () => {
   await api('administration/' + type, 'POST', input);
   form.reset();
   await Promise.all([load(), refreshLookups()]);
  }, 'Kayıt eklendi.');
 }

 if (!data) return null;
 return <>
  <section className="card"><h2>Personel hesapları</h2>
   <form className="filters" onSubmit={event => save(event, 'staff')}>
    <Field label="Ad soyad" name="fullName" maxLength={100} required/>
    <Field label="E-posta" name="email" type="email" required/>
    <Field label="İlk parola" name="password" type="password" minLength={10} required/>
    <label>Rol<select name="role">
     <option value="Advisor">Danışman</option><option value="DepartmentHead">Bölüm başkanı</option>
     <option value="FacultyManagement">Fakülte yönetimi</option>
    </select></label>
    <OrganizationDepartment departments={data.departments}/>
    <button disabled={busy}>Personel ekle</button>
   </form>
   <DataTable columns={['Ad soyad', 'E-posta', 'Rol', 'Bölüm']} empty="Personel hesabı yok.">
    {data.staff.map(row => <tr key={row.id}>
     <td>{row.fullName}</td><td>{row.email}</td>
     <td>{{ Advisor: 'Danışman', DepartmentHead: 'Bölüm başkanı', FacultyManagement: 'Fakülte yönetimi' }[row.role]}</td>
     <td>{data.departments.find(d => d.id === row.departmentId)?.name || '—'}</td>
    </tr>)}
   </DataTable>
  </section>
  <section className="card"><h2>Bölümler ve çalışma birimleri</h2>
   <div className="grid2">
    <div><h3>Bölümler</h3>
     <form onSubmit={event => save(event, 'departments')}>
      <Field label="Bölüm adı" name="name" maxLength={100} required/><button disabled={busy}>Bölüm ekle</button>
     </form>
     <ul>{data.departments.map(row => <li key={row.id}>{row.name}</li>)}</ul>
    </div>
    <div><h3>Birimler</h3>
     <form onSubmit={event => save(event, 'units')}>
      <Field label="Birim adı" name="name" maxLength={100} required/>
      <OrganizationDepartment departments={data.departments}/>
      <button disabled={busy}>Birim ekle</button>
     </form>
     <ul>{data.units.map(row => <li key={row.id}>{row.name}</li>)}</ul>
    </div>
   </div>
  </section>
  <section className="card"><h2>Program dönemleri</h2>
   <form className="filters" onSubmit={event => save(event, 'periods')}>
    <Field label="Dönem adı" name="name" maxLength={100} required/>
    <Field label="Başlangıç" name="start" type="date" required/>
    <Field label="Bitiş" name="end" type="date" required/>
    <button disabled={busy}>Dönem ekle</button>
   </form>
   <DataTable columns={['Dönem', 'Başlangıç', 'Bitiş']} empty="Program dönemi yok.">
    {data.periods.map(row => <tr key={row.id}><td>{row.name}</td><td>{row.start}</td><td>{row.end}</td></tr>)}
   </DataTable>
  </section>
 </>;
}

function OrganizationDepartment({ departments }) {
 return <label>Bölüm<select name="departmentId"><option value="">Fakülte geneli</option>
  {departments.map(row => <option key={row.id} value={row.id}>{row.name}</option>)}
 </select></label>;
}

export function NewParticipation({ id, look, run, busy, onCreated }) {
 return <section className="card"><h2>Yeni program katılımı</h2>
  <form className="filters" onSubmit={event => {
   event.preventDefault();
   const values = new FormData(event.currentTarget);
   run(async () => {
    const result = await api(`students/${id}/participations`, 'POST', {
     programPeriodId: Number(values.get('period')), workUnitId: Number(values.get('unit'))
    });
    await onCreated(result.id);
   }, 'Yeni katılım oluşturuldu.');
  }}>
   <label>Dönem<select name="period" required><option value="">Dönem seçin</option>
    {look.periods.map(row => <option key={row.id} value={row.id}>{row.name}</option>)}
   </select></label>
   <label>Çalışma birimi<select name="unit" required><option value="">Birim seçin</option>
    {look.workUnits.map(row => <option key={row.id} value={row.id}>{row.name}</option>)}
   </select></label>
   <button disabled={busy}>Katılım ekle</button>
  </form>
 </section>;
}
