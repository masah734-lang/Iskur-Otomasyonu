import assert from 'node:assert/strict';
import { test } from 'node:test';
import { calendarTone, courseTimeRange } from '../frontend/src/calendar.mjs';

const base = { weekend: false, holiday: null, selected: false, clash: false };
for (const [name, override, expected] of [
 ['Boş gün gri', {}, 'unset'],
 ['Ders günü seçimden önce gri', { hasCourse: true }, 'unset'],
 ['Seçili uygun gün yeşil', { selected: true }, 'chosen'],
 ['Çakışan seçimin uyarı kenarı var', { selected: true, clash: true }, 'conflict'],
 ['Hafta sonu kırmızı', { weekend: true }, 'closed'],
 ['Tam tatil seçimin önüne geçer', { holiday: { afternoonOnly: false }, selected: true }, 'closed'],
 ['Yarım tatilde seçim yoksa gri', { holiday: { afternoonOnly: true } }, 'unset'],
 ['Yarım tatilde seçilen sabah yeşil', { holiday: { afternoonOnly: true }, selected: true }, 'chosen'],
 ['Dersin diğer yarısındaki uygun seçim yeşil', { hasCourse: true, selected: true }, 'chosen']
]) {
 test(name, () => assert.equal(calendarTone({ ...base, ...override }), expected));
}

test('Ders saatleri sıfır dolgulu bir aralık olarak görüntülenir', () => {
 assert.equal(courseTimeRange({ startMinute: 545, endMinute: 620 }), '09:05–10:20');
});
