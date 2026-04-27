// ── Sidebar toggle (mobile) ───────────────────────────────────────────────
const toggle = document.getElementById('sidebarToggle');
const sidebar = document.getElementById('sidebar');
if (toggle && sidebar) {
    toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
    document.addEventListener('click', e => {
        if (!sidebar.contains(e.target) && !toggle.contains(e.target))
            sidebar.classList.remove('open');
    });
}

// ── Availability checker on Booking form ──────────────────────────────────
function setupAvailabilityCheck() {
    const venueEl = document.getElementById('Booking_VenueID');
    const dateEl = document.getElementById('Booking_EventDate');
    const startEl = document.getElementById('Booking_StartTime');
    const endEl = document.getElementById('Booking_EndTime');
    const statusEl = document.getElementById('availability-status');
    const excludeId = document.getElementById('excludeBookingId')?.value;

    if (!venueEl || !dateEl || !startEl || !endEl || !statusEl) return;

    async function check() {
        const venueId = venueEl.value;
        const eventDate = dateEl.value;
        const startTime = startEl.value;
        const endTime = endEl.value;

        if (!venueId || !eventDate || !startTime || !endTime) {
            statusEl.textContent = '';
            return;
        }

        statusEl.textContent = 'Checking...';
        statusEl.className = '';

        try {
            const params = new URLSearchParams({ venueId, eventDate, startTime, endTime });
            if (excludeId) params.append('excludeId', excludeId);

            const res = await fetch(`/Bookings/CheckAvailability?${params}`);
            const data = await res.json();

            statusEl.textContent = data.message;
            statusEl.className = data.available ? 'ok' : 'conflict';
        } catch {
            statusEl.textContent = '';
        }
    }

    [venueEl, dateEl, startEl, endEl].forEach(el => el.addEventListener('change', check));
}

document.addEventListener('DOMContentLoaded', setupAvailabilityCheck);

// ── Auto-dismiss flash alerts ─────────────────────────────────────────────
document.querySelectorAll('.alert').forEach(el => {
    setTimeout(() => {
        el.style.transition = 'opacity 0.5s';
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 500);
    }, 4000);
});
