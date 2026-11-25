export function calendarTone({ weekend, holiday, selected, clash }) {
 if (weekend || (holiday && !holiday.afternoonOnly)) return 'closed';
 if (selected && clash) return 'conflict';
 return selected ? 'chosen' : 'unset';
}

export function formatCourseTime(minutes) {
 return String(Math.floor(minutes / 60)).padStart(2, '0') + ':' + String(minutes % 60).padStart(2, '0');
}

export function courseTimeRange(course) {
 return formatCourseTime(course.startMinute) + '–' + formatCourseTime(course.endMinute);
}
