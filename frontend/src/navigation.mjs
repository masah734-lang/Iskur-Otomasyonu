export function readRoute(location, historyState) {
 const [page, query] = location.hash.slice(2).split('?');
 const params = new URLSearchParams(query);
 const id = Number(params.get('ogrenci'));
 const month = params.get('ay');
 return {
  page: location.hash.startsWith('#/') ? page : null,
  selected: Number.isSafeInteger(id) && id > 0 ? id : null,
  month: /^(?:20[2-9]\d|2100)-(?:0[1-9]|1[0-2])$/.test(month || '') ? month : null,
  depth: Number.isInteger(historyState?.iskurDepth) ? historyState.iskurDepth : 0
 };
}

export function routeHash(page, selected, month) {
 if (!page) return '';
 const params = new URLSearchParams();
 if (selected) params.set('ogrenci', selected);
 if (page === 'plan' && month) params.set('ay', month);
 return '#/' + page + (params.size ? '?' + params : '');
}
