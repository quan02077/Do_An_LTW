// js/data.js - Dữ liệu hạt giống và các hàm tiện ích dùng chung

const DEFAULT_CATEGORIES = [
  { id: 1, name: "Công nghệ", icon: "cpu", description: "Hội thảo, workshop công nghệ và AI", active: true, color: "#4338CA" },
  { id: 2, name: "Âm nhạc", icon: "music", description: "Concert, biểu diễn và lễ hội âm nhạc", active: true, color: "#7C3AED" },
  { id: 3, name: "Thể thao", icon: "trophy", description: "Giải đấu và sự kiện thể thao", active: true, color: "#059669" },
  { id: 4, name: "Giáo dục", icon: "book", description: "Khóa học, seminar và hội thảo học thuật", active: true, color: "#D97706" },
  { id: 5, name: "Nghệ thuật", icon: "palette", description: "Triển lãm và sự kiện nghệ thuật", active: true, color: "#DC2626" },
  { id: 6, name: "Kinh doanh", icon: "briefcase", description: "Hội nghị và networking doanh nghiệp", active: true, color: "#2563EB" },
];

const DEFAULT_EVENTS = [
  {
    id: 1,
    title: "Vietnam AI Summit 2026",
    categoryId: 1,
    image: "https://images.unsplash.com/photo-1677442135703-1787eea5ce01?w=800&h=500&fit=crop&auto=format",
    startDate: "2026-10-15T09:00",
    endDate: "2026-10-15T18:00",
    location: "Trung tâm Hội nghị Quốc gia, Hà Nội",
    description: "Vietnam AI Summit 2026 là sự kiện công nghệ trí tuệ nhân tạo hàng đầu tại Việt Nam, quy tụ hơn 500 chuyên gia, nhà nghiên cứu và lãnh đạo doanh nghiệp trong lĩnh vực AI.\n\nChương trình bao gồm các phiên keynote từ diễn giả quốc tế, workshop thực hành về Machine Learning và Deep Learning, cùng khu vực triển lãm sản phẩm AI từ các startup và tập đoàn lớn.\n\nĐiểm nổi bật:\n- 20+ diễn giả hàng đầu từ Google, Meta, VinAI\n- 6 workshop chuyên sâu\n- Networking lunch và gala dinner\n- Cơ hội kết nối với 500+ chuyên gia",
    capacity: 500,
    registered: 387,
    status: "upcoming",
    featured: true,
  },
  {
    id: 2,
    title: "Monsoon Music Festival",
    categoryId: 2,
    image: "https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?w=800&h=500&fit=crop&auto=format",
    startDate: "2026-10-20T17:00",
    endDate: "2026-10-20T23:00",
    location: "Công viên Thống Nhất, Hà Nội",
    description: "Monsoon Music Festival trở lại với quy mô lớn nhất từ trước đến nay, mang đến những đêm nhạc đầy cảm xúc với sự tham gia của các nghệ sĩ đình đám trong và ngoài nước.\n\nLễ hội âm nhạc mùa mưa là sự kiện thường niên được mong chờ nhất tại Hà Nội, kết hợp hoàn hảo giữa âm nhạc đương đại và không gian xanh của thủ đô.",
    capacity: 2000,
    registered: 1654,
    status: "upcoming",
    featured: true,
  },
  {
    id: 3,
    title: "Hanoi Marathon 2026",
    categoryId: 3,
    image: "https://images.unsplash.com/photo-1571008887538-b36bb32f4571?w=800&h=500&fit=crop&auto=format",
    startDate: "2026-11-01T05:30",
    endDate: "2026-11-01T12:00",
    location: "Hồ Hoàn Kiếm, Hà Nội",
    description: "Hanoi Marathon 2026 — Giải chạy quốc tế qua những con đường lịch sử của Hà Nội. Ba cự ly tham dự: Full Marathon (42km), Half Marathon (21km) và Fun Run (5km). Cơ hội trải nghiệm không khí mùa thu Hà Nội cùng hàng ngàn vận động viên chuyên nghiệp và phong trào.",
    capacity: 3000,
    registered: 2841,
    status: "upcoming",
    featured: false,
  },
  {
    id: 4,
    title: "UX Design Workshop: Figma Advanced",
    categoryId: 4,
    image: "https://images.unsplash.com/photo-1587440871875-191322ee64b0?w=800&h=500&fit=crop&auto=format",
    startDate: "2026-09-28T13:00",
    endDate: "2026-09-28T17:00",
    location: "WeWork Lotte Center, Hà Nội",
    description: "Workshop thực hành về Figma nâng cao dành cho UI/UX Designer. Học cách tạo design system, auto layout, component variants và prototyping chuyên nghiệp với sự hướng dẫn trực tiếp từ Senior Product Designer.",
    capacity: 40,
    registered: 38,
    status: "upcoming",
    featured: false,
  },
  {
    id: 5,
    title: "Triển Lãm Mỹ Thuật Đương Đại",
    categoryId: 5,
    image: "https://images.unsplash.com/photo-1531243269054-5ebf6f34081e?w=800&h=500&fit=crop&auto=format",
    startDate: "2026-09-01T10:00",
    endDate: "2026-09-30T20:00",
    location: "Bảo tàng Mỹ thuật Việt Nam, Hà Nội",
    description: "Triển lãm tác phẩm của 30 nghệ sĩ đương đại Việt Nam và quốc tế. Khám phá sự giao thoa giữa nghệ thuật truyền thống và hiện đại qua hội họa, điêu khắc và nghệ thuật sắp đặt ánh sáng.",
    capacity: 200,
    registered: 89,
    status: "ongoing",
    featured: true,
  },
  {
    id: 6,
    title: "Startup Pitch Night — Mùa 8",
    categoryId: 6,
    image: "https://images.unsplash.com/photo-1559136555-9303baea8ebd?w=800&h=500&fit=crop&auto=format",
    startDate: "2026-08-20T18:00",
    endDate: "2026-08-20T21:00",
    location: "Toong Coworking Space, TP.HCM",
    description: "15 startup tiềm năng trình bày ý tưởng trước hội đồng nhà đầu tư mạo hiểm và các quỹ đầu tư thiên thần. Cơ hội kết nối và tìm kiếm nguồn vốn đầu tư hạt giống (Seed round).",
    capacity: 150,
    registered: 150,
    status: "ended",
    featured: false,
  },
];

const DEFAULT_USERS = [
  { id: 1, name: "Nguyễn Minh Anh", email: "minhanh@email.com", phone: "0912 345 678", role: "user", active: true, avatar: "NMA", password: "user123" },
  { id: 2, name: "Trần Văn Hùng", email: "vanhung@email.com", phone: "0987 654 321", role: "user", active: true, avatar: "TVH", password: "user123" },
  { id: 3, name: "Lê Thị Thu Hà", email: "thuha@email.com", phone: "0901 234 567", role: "admin", active: true, avatar: "LTH", password: "admin123" },
  { id: 4, name: "Phạm Quốc Bảo", email: "quocbao@email.com", phone: "0978 123 456", role: "user", active: false, avatar: "PQB", password: "user123" },
  { id: 5, name: "Vũ Ngọc Linh", email: "ngoclinh@email.com", phone: "0965 432 109", role: "user", active: true, avatar: "VNL", password: "user123" },
];

const DEFAULT_REGISTRATIONS = [
  { id: 1, userId: 1, eventId: 1, registeredAt: "2026-09-01T10:23", notes: "Muốn tham gia workshop ML", status: "confirmed" },
  { id: 2, userId: 1, eventId: 2, registeredAt: "2026-09-05T14:11", notes: "Vé VIP hàng ghế đầu", status: "confirmed" },
  { id: 3, userId: 1, eventId: 6, registeredAt: "2026-08-10T09:00", notes: "Đã hoàn thành sự kiện", status: "confirmed" },
  { id: 4, userId: 2, eventId: 1, registeredAt: "2026-09-02T11:30", notes: "Cần hóa đơn VAT", status: "confirmed" },
  { id: 5, userId: 5, eventId: 3, registeredAt: "2026-09-10T08:45", notes: "Đăng ký cự ly 21km", status: "confirmed" },
  { id: 6, userId: 2, eventId: 5, registeredAt: "2026-09-08T15:20", notes: "", status: "cancelled" },
];

// ─── LocalStorage Helpers ───────────────────────────────────────────────────
function getCategories() {
  const data = localStorage.getItem("eventvn_categories");
  if (!data) {
    localStorage.setItem("eventvn_categories", JSON.stringify(DEFAULT_CATEGORIES));
    return DEFAULT_CATEGORIES;
  }
  return JSON.parse(data);
}

function saveCategories(categories) {
  localStorage.setItem("eventvn_categories", JSON.stringify(categories));
}

function getEvents() {
  const data = localStorage.getItem("eventvn_events");
  if (!data) {
    localStorage.setItem("eventvn_events", JSON.stringify(DEFAULT_EVENTS));
    return DEFAULT_EVENTS;
  }
  return JSON.parse(data);
}

function saveEvents(events) {
  localStorage.setItem("eventvn_events", JSON.stringify(events));
}

function getUsers() {
  const data = localStorage.getItem("eventvn_users");
  if (!data) {
    localStorage.setItem("eventvn_users", JSON.stringify(DEFAULT_USERS));
    return DEFAULT_USERS;
  }
  return JSON.parse(data);
}

function saveUsers(users) {
  localStorage.setItem("eventvn_users", JSON.stringify(users));
}

function getRegistrations() {
  const data = localStorage.getItem("eventvn_registrations");
  if (!data) {
    localStorage.setItem("eventvn_registrations", JSON.stringify(DEFAULT_REGISTRATIONS));
    return DEFAULT_REGISTRATIONS;
  }
  return JSON.parse(data);
}

function saveRegistrations(regs) {
  localStorage.setItem("eventvn_registrations", JSON.stringify(regs));
}

// ─── Bookmarks (Trái tim yêu thích) ──────────────────────────────────────────
function getBookmarks() {
  const data = localStorage.getItem("eventvn_bookmarks");
  if (!data) return [1, 2]; // Mặc định bookmark 2 sự kiện tiêu biểu
  try {
    return JSON.parse(data);
  } catch (e) {
    return [];
  }
}

function isBookmarked(eventId) {
  const bookmarks = getBookmarks();
  return bookmarks.includes(Number(eventId));
}

function toggleBookmark(eventId) {
  const id = Number(eventId);
  let bookmarks = getBookmarks();
  let isAdded = false;
  if (bookmarks.includes(id)) {
    bookmarks = bookmarks.filter((b) => b !== id);
  } else {
    bookmarks.push(id);
    isAdded = true;
  }
  localStorage.setItem("eventvn_bookmarks", JSON.stringify(bookmarks));
  updateBookmarkBadges();
  return isAdded;
}

function updateBookmarkBadges() {
  const count = getBookmarks().length;
  document.querySelectorAll(".bookmark-count-badge").forEach((el) => {
    el.textContent = count;
    if (count === 0) {
      el.classList.add("hidden");
    } else {
      el.classList.remove("hidden");
    }
  });
}

// ─── Formatters ─────────────────────────────────────────────────────────────
function fmtDate(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  return d.toLocaleDateString("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric" });
}

function fmtDateTime(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  return d.toLocaleString("vi-VN", { day: "2-digit", month: "2-digit", year: "numeric", hour: "2-digit", minute: "2-digit" });
}

function fmtTime(iso) {
  if (!iso) return "";
  return new Date(iso).toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" });
}

// ─── Vector SVG Symbols Generators ──────────────────────────────────────────
function getHeartSymbol(filled = false, className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="${filled ? "currentColor" : "none"}" stroke="currentColor" stroke-width="${filled ? "0" : "2"}" stroke-linecap="round" stroke-linejoin="round">
      <path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z" />
    </svg>
  `;
}

function getCalendarSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <rect width="18" height="18" x="3" y="4" rx="2" ry="2" />
      <line x1="16" x2="16" y1="2" y2="6" />
      <line x1="8" x2="8" y1="2" y2="6" />
      <line x1="3" x2="21" y1="10" y2="10" />
    </svg>
  `;
}

function getMapPinSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M20 10c0 4.993-5.539 10.193-7.399 11.799a1 1 0 0 1-1.202 0C9.539 20.193 4 14.993 4 10a8 8 0 0 1 16 0" />
      <circle cx="12" cy="10" r="3" />
    </svg>
  `;
}

function getClockSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <circle cx="12" cy="12" r="10" />
      <polyline points="12 6 12 12 16 14" />
    </svg>
  `;
}

function getSearchSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.3-4.3" />
    </svg>
  `;
}

function getUserSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  `;
}

function getUsersSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="4" />
      <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
      <path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  `;
}

function getMailSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <rect width="20" height="16" x="2" y="4" rx="2" />
      <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
    </svg>
  `;
}

function getPhoneSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z" />
    </svg>
  `;
}

function getCheckSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
      <polyline points="20 6 9 17 4 12" />
    </svg>
  `;
}

function getStarSymbol(className = "w-4 h-4") {
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="currentColor" stroke="none">
      <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
    </svg>
  `;
}

function getCategorySymbol(name, className = "w-4 h-4") {
  const n = (name || "").toLowerCase();
  if (n.includes("công nghệ")) {
    return `
      <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <rect width="16" height="16" x="4" y="4" rx="2" />
        <rect width="6" height="6" x="9" y="9" rx="1" />
        <path d="M15 2v2" /><path d="M15 20v2" /><path d="M2 15h2" /><path d="M2 9h2" /><path d="M20 15h2" /><path d="M20 9h2" /><path d="M9 2v2" /><path d="M9 20v2" />
      </svg>
    `;
  }
  if (n.includes("âm nhạc")) {
    return `
      <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M9 18V5l12-2v13" /><circle cx="6" cy="18" r="3" /><circle cx="18" cy="16" r="3" />
      </svg>
    `;
  }
  if (n.includes("thể thao")) {
    return `
      <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="10" />
        <path d="M2.1 13.4A10 10 0 0 0 6 19.9" />
        <path d="M21.9 10.6A10 10 0 0 0 18 4.1" />
        <path d="M4.1 6A10 10 0 0 0 10.6 21.9" />
        <path d="M19.9 18a10 10 0 0 0-6.5-15.9" />
      </svg>
    `;
  }
  if (n.includes("giáo dục")) {
    return `
      <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M4 19.5v-15A2.5 2.5 0 0 1 6.5 2H20v20H6.5a2.5 2.5 0 0 1-2.5-2.5Z" />
        <path d="M6 6h10" /><path d="M6 10h10" />
      </svg>
    `;
  }
  if (n.includes("nghệ thuật")) {
    return `
      <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="13.5" cy="6.5" r=".5" fill="currentColor" />
        <circle cx="17.5" cy="10.5" r=".5" fill="currentColor" />
        <circle cx="8.5" cy="7.5" r=".5" fill="currentColor" />
        <circle cx="6.5" cy="12.5" r=".5" fill="currentColor" />
        <path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10c.926 0 1.648-.746 1.648-1.688 0-.437-.18-.835-.437-1.125-.29-.289-.438-.652-.438-1.125a1.64 1.64 0 0 1 1.668-1.668h1.996c3.051 0 5.563-2.512 5.563-5.563C22 6.5 17.5 2 12 2Z" />
      </svg>
    `;
  }
  return `
    <svg viewBox="0 0 24 24" class="${className}" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M16 20V4a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16" />
      <rect width="20" height="14" x="2" y="6" rx="2" />
    </svg>
  `;
}

function getStatusBadge(status) {
  const map = {
    upcoming: { label: "Sắp diễn ra", bg: "bg-indigo-50 text-indigo-700 border-indigo-200" },
    ongoing: { label: "Đang diễn ra", bg: "bg-emerald-50 text-emerald-700 border-emerald-200" },
    ended: { label: "Đã kết thúc", bg: "bg-slate-100 text-slate-600 border-slate-200" },
    cancelled: { label: "Đã hủy", bg: "bg-rose-50 text-rose-700 border-rose-200" },
    confirmed: { label: "Đã xác nhận", bg: "bg-emerald-50 text-emerald-700 border-emerald-200" },
    pending: { label: "Chờ duyệt", bg: "bg-amber-50 text-amber-700 border-amber-200" },
  };
  const s = map[status] || { label: status, bg: "bg-slate-100 text-slate-600 border-slate-200" };
  return `<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${s.bg}">${s.label}</span>`;
}

// ─── Toast Notification Helper ──────────────────────────────────────────────
function showToast(message, type = "success") {
  let container = document.getElementById("toast-container");
  if (!container) {
    container = document.createElement("div");
    container.id = "toast-container";
    container.className = "fixed bottom-5 right-5 z-50 flex flex-col gap-2 max-w-sm pointer-events-none";
    document.body.appendChild(container);
  }

  const toast = document.createElement("div");
  toast.className = `pointer-events-auto flex items-center gap-3 px-4 py-3 rounded-xl shadow-lg border text-sm font-medium animate-slide-up transition-all duration-300 ${
    type === "success"
      ? "bg-white text-slate-900 border-slate-200"
      : type === "error"
      ? "bg-rose-50 text-rose-800 border-rose-200"
      : "bg-indigo-50 text-indigo-800 border-indigo-200"
  }`;

  toast.innerHTML = `
    <div class="flex-shrink-0 text-indigo-600">${type === "success" ? getCheckSymbol("w-4 h-4") : ""}</div>
    <div class="flex-1">${message}</div>
    <button type="button" class="text-slate-400 hover:text-slate-600 ml-2" onclick="this.parentElement.remove()">
      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
    </button>
  `;

  container.appendChild(toast);
  setTimeout(() => {
    toast.classList.add("opacity-0", "translate-y-2");
    setTimeout(() => toast.remove(), 300);
  }, 3200);
}

