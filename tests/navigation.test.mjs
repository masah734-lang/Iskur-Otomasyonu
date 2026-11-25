import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readRoute, routeHash } from '../frontend/src/navigation.mjs';

test('Plan adresi ay ve öğrenci seçimini geri yükler', () => {
 const hash = routeHash('plan', 7, '2026-11');
 assert.equal(hash, '#/plan?ogrenci=7&ay=2026-11');
 assert.deepEqual(readRoute({ hash }, { iskurDepth: 3 }), {
  page: 'plan', selected: 7, month: '2026-11', depth: 3
 });
});

test('Hatalı ay ve kimlik değerleri yok sayılır', () => {
 assert.deepEqual(readRoute({ hash: '#/plan?ogrenci=-4&ay=2026-13' }), {
  page: 'plan', selected: null, month: null, depth: 0
 });
});
