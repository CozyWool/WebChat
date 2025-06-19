function toLocalTime(utcDateString) {
    const [datePart, timePart] = utcDateString.trim().split(/\s+/);
    const [dd, mm, yyyy] = datePart.split('.').map(Number);
    const [hh, min, ss] = timePart.split(':').map(Number);

    const utcDate = new Date(Date.UTC(yyyy, mm - 1, dd, hh, min, ss));

    return new Date(utcDate.getTime());
}

function formatLastSeen(utcDateString) {
    const localDate = toLocalTime(utcDateString);

    const now = new Date();
    const diffMs = now - localDate;
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return "только что";
    if (diffMins < 60) return `${diffMins} ${plural(diffMins, 'минуту', 'минуты', 'минут')} назад`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} ${plural(diffHours, 'час', 'часа', 'часов')} назад`;

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays <= 1) return `${diffDays} ${plural(diffDays, 'день', 'дня', 'дней')} назад`;

    return formatDateTime(localDate);
}

function plural(n, one, few, many) {
    const nAbs = Math.abs(n);
    if (nAbs % 10 === 1 && nAbs % 100 !== 11) return one;
    if (nAbs % 10 >= 2 && nAbs % 10 <= 4 && (nAbs % 100 < 10 || nAbs % 100 >= 20)) return few;
    return many;
}

function formatDateTime(date) {
    const pad = (n) => n.toString().padStart(2, '0');
    const dd = pad(date.getDate());
    const mm = pad(date.getMonth() + 1);
    const yyyy = date.getFullYear();

    const hh = pad(date.getHours());
    const min = pad(date.getMinutes());

    const currentDate = new Date(Date.now());
    const isCurrentYear = currentDate.getFullYear() === yyyy;
    const isToday = pad(currentDate.getDate()) === dd
        && pad(currentDate.getMonth() + 1) === mm
        && isCurrentYear;

    if (isToday) {
        return `${hh}:${min}`;
    }
    if (isCurrentYear) {
        return `${dd}.${mm} ${hh}:${min}`;
    }
    return `${dd}.${mm}.${yyyy} ${hh}:${min}`;
}

document.querySelectorAll('.date-humanizer').forEach(el => {
    const utc = el.dataset.utc;
    if (utc) {
        el.textContent = formatLastSeen(utc);
    }
});
document.querySelectorAll('.date-utc-input').forEach(el => {
    const utc = el.dataset.utc;
    if (utc) {
        el.value = formatDateTime(toLocalTime(utc));
    }
});
document.querySelectorAll('.date-utc-span').forEach(el => {
    const utc = el.textContent.trim();
    if (utc) {
        console.log(utc);
        el.textContent = formatDateTime(toLocalTime(utc));
    }
});