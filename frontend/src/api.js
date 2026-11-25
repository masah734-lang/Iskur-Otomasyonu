let token = null;

export function setToken(value) { token = value; }

async function request(path, method, body) {
 const form = body instanceof FormData;
 const headers = token ? { Authorization: 'Bearer ' + token } : {};
 if (body !== undefined && !form) headers['Content-Type'] = 'application/json';
 const response = await fetch('/api/' + path, {
  method, headers, body: body === undefined || form ? body : JSON.stringify(body)
 });
 if (!response.ok) {
  const data = await response.json().catch(() => ({}));
  if (response.status === 401) window.dispatchEvent(new Event('session-expired'));
  throw new Error(data.message || (response.status === 401
   ? 'E-posta veya parola hatalı ya da oturumunuz sona erdi.'
   : response.status === 403 ? 'Bu işlem için yetkiniz yok.'
   : Object.values(data.errors || {}).flat().join(' ') || 'İşlem tamamlanamadı.'));
 }
 return response;
}

export async function api(path, method = 'GET', body) {
 const response = await request(path, method, body);
 return response.status === 204 ? null : response.json().catch(() => null);
}

export async function download(path, filename) {
 const response = await request(path, 'GET');
 const disposition = response.headers.get('content-disposition') || '';
 const supplied = disposition.match(/filename="?([^";]+)"?/)?.[1];
 const href = URL.createObjectURL(await response.blob());
 try {
  const link = document.createElement('a');
  link.href = href;
  link.download = supplied || filename;
  link.click();
 } finally {
  setTimeout(() => URL.revokeObjectURL(href), 1000);
 }
}
