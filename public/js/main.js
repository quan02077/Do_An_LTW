// js/main.js - Logic tương tác trang chủ, tìm kiếm, lọc danh mục, bookmark trái tim

let activeCategory = 0; // 0 = Tất cả
let activeStatus = "all"; // all, upcoming, ongoing, ended, bookmarked
let searchKeyword = "";

document.addEventListener("DOMContentLoaded", () => {
  renderCategoryPills();
  renderEvents();
  setupEventListeners();
});

function setupEventListeners() {
  // Tìm kiếm sự kiện
  const searchInput = document.getElementById("search-input");
  if (searchInput) {
    searchInput.addEventListener("input", (e) => {
      searchKeyword = e.target.value.trim().toLowerCase();
      renderEvents();
    });
  }

  // Lọc theo trạng thái / Yêu thích
  const statusTabs = document.querySelectorAll(".status-filter-tab");
  statusTabs.forEach((tab) => {
    tab.addEventListener("click", () => {
      statusTabs.forEach((t) => {
        t.classList.remove("bg-indigo-600", "text-white", "font-semibold");
        t.classList.add("text-slate-600", "hover:bg-slate-100");
      });
      tab.classList.remove("text-slate-600", "hover:bg-slate-100");
      tab.classList.add("bg-indigo-600", "text-white", "font-semibold");

      activeStatus = tab.dataset.status;
      renderEvents();
    });
  });

  // Mobile menu toggle
  const mobileMenuBtn = document.getElementById("mobile-menu-btn");
  const mobileMenu = document.getElementById("mobile-menu");
  if (mobileMenuBtn && mobileMenu) {
    mobileMenuBtn.addEventListener("click", () => {
      mobileMenu.classList.toggle("hidden");
    });
  }
}

// Render các chip lọc danh mục ở trang chủ
function renderCategoryPills() {
  const container = document.getElementById("category-pills-container");
  if (!container) return;

  const categories = getCategories().filter((c) => c.active);
  let html = `
    <button onclick="selectCategory(0)" class="category-pill flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-xs font-medium transition-all ${
      activeCategory === 0
        ? "bg-indigo-600 text-white shadow-sm"
        : "bg-white text-slate-700 border border-slate-200 hover:bg-slate-50"
    }">
      <span>Tất cả</span>
    </button>
  `;

  categories.forEach((cat) => {
    const isSelected = activeCategory === cat.id;
    html += `
      <button onclick="selectCategory(${cat.id})" class="category-pill flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-xs font-medium transition-all ${
        isSelected
          ? "bg-indigo-600 text-white shadow-sm"
          : "bg-white text-slate-700 border border-slate-200 hover:bg-slate-50"
      }">
        <span class="${isSelected ? "text-white" : "text-indigo-600"}">${getCategorySymbol(cat.name, "w-3.5 h-3.5")}</span>
        <span>${cat.name}</span>
      </button>
    `;
  });

  container.innerHTML = html;
}

function selectCategory(catId) {
  activeCategory = catId;
  renderCategoryPills();
  renderEvents();
}

// Render danh sách sự kiện theo bộ lọc
function renderEvents() {
  const container = document.getElementById("events-grid-container");
  const countLabel = document.getElementById("events-count-label");
  if (!container) return;

  const allEvents = getEvents();
  const categories = getCategories();
  const bookmarks = getBookmarks();

  let filtered = allEvents.filter((event) => {
    // Lọc theo danh mục
    if (activeCategory !== 0 && event.categoryId !== activeCategory) {
      return false;
    }
    // Lọc theo trạng thái hoặc bookmark yêu thích
    if (activeStatus === "bookmarked") {
      if (!bookmarks.includes(event.id)) return false;
    } else if (activeStatus !== "all" && event.status !== activeStatus) {
      return false;
    }
    // Tìm kiếm theo từ khóa
    if (searchKeyword) {
      const matchTitle = event.title.toLowerCase().includes(searchKeyword);
      const matchLoc = event.location.toLowerCase().includes(searchKeyword);
      const cat = categories.find((c) => c.id === event.categoryId);
      const matchCat = cat && cat.name.toLowerCase().includes(searchKeyword);
      if (!matchTitle && !matchLoc && !matchCat) return false;
    }
    return true;
  });

  if (countLabel) {
    countLabel.textContent = `Hiển thị ${filtered.length} sự kiện`;
  }

  if (filtered.length === 0) {
    container.innerHTML = `
      <div class="col-span-full text-center py-16 px-4 bg-white rounded-2xl border border-slate-200">
        <div class="w-16 h-16 mx-auto mb-4 bg-slate-100 text-slate-400 rounded-full flex items-center justify-center">
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
        </div>
        <h3 class="text-base font-bold text-slate-800 mb-1">Không tìm thấy sự kiện nào</h3>
        <p class="text-sm text-slate-500 mb-4">Hãy thử tìm kiếm với từ khóa khác hoặc xóa bớt bộ lọc.</p>
        <button onclick="resetFilters()" class="px-4 py-2 text-xs font-semibold bg-indigo-50 text-indigo-700 rounded-lg hover:bg-indigo-100 transition-colors">
          Đặt lại bộ lọc
        </button>
      </div>
    `;
    return;
  }

  let html = "";
  filtered.forEach((event) => {
    const cat = categories.find((c) => c.id === event.categoryId) || { name: "Chung" };
    const isSaved = bookmarks.includes(event.id);
    const percent = Math.min(100, Math.round((event.registered / event.capacity) * 100));

    html += `
      <div class="bg-white rounded-2xl border border-slate-200 overflow-hidden shadow-sm hover:shadow-md transition-all duration-200 flex flex-col group">
        <!-- Thumbnail -->
        <div class="relative h-48 w-full overflow-hidden bg-slate-100">
          <img src="${event.image}" alt="${event.title}" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" loading="lazy" />
          
          <!-- Category & Status Badge -->
          <div class="absolute top-3 left-3 flex flex-wrap gap-1.5 items-center">
            <span class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold bg-white/90 backdrop-blur-sm text-slate-800 shadow-sm">
              ${getCategorySymbol(cat.name, "w-3 h-3 text-indigo-600")}
              <span>${cat.name}</span>
            </span>
            ${getStatusBadge(event.status)}
          </div>

          <!-- Nút Bookmark Trái Tim Yêu Thích -->
          <button 
            type="button"
            onclick="handleBookmarkClick(event, ${event.id})"
            class="bookmark-btn-${event.id} absolute top-3 right-3 w-9 h-9 rounded-full bg-white/90 backdrop-blur-sm flex items-center justify-center shadow-sm border border-slate-200/60 hover:scale-110 active:scale-95 transition-all z-10 ${
              isSaved ? "text-rose-500 fill-rose-500 bg-white" : "text-slate-400 hover:text-rose-500"
            }"
            title="${isSaved ? "Bỏ lưu sự kiện" : "Lưu vào yêu thích"}"
          >
            ${getHeartSymbol(isSaved, `w-5 h-5 transition-colors ${isSaved ? "text-rose-500" : ""}`)}
          </button>
        </div>

        <!-- Body -->
        <div class="p-5 flex-1 flex flex-col justify-between">
          <div>
            <div class="flex items-center gap-2 text-xs font-semibold text-indigo-600 mb-1.5">
              ${getCalendarSymbol("w-3.5 h-3.5")}
              <span>${fmtDateTime(event.startDate)}</span>
            </div>

            <h3 class="text-base font-bold text-slate-900 group-hover:text-indigo-600 transition-colors line-clamp-2 mb-2">
              <a href="event-detail.html?id=${event.id}">${event.title}</a>
            </h3>

            <div class="flex items-center gap-1.5 text-xs text-slate-500 line-clamp-1 mb-4">
              ${getMapPinSymbol("w-3.5 h-3.5 text-slate-400 flex-shrink-0")}
              <span>${event.location}</span>
            </div>
          </div>

          <!-- Capacity & Action -->
          <div>
            <div class="space-y-1.5 mb-4">
              <div class="flex justify-between text-xs font-medium text-slate-600">
                <span>Số chỗ đã đăng ký</span>
                <span class="font-semibold text-slate-800">${event.registered}/${event.capacity} (${percent}%)</span>
              </div>
              <div class="w-full h-1.5 bg-slate-100 rounded-full overflow-hidden">
                <div class="h-full rounded-full transition-all duration-500 ${
                  percent >= 100 ? "bg-rose-500" : percent >= 80 ? "bg-amber-500" : "bg-indigo-600"
                }" style="width: ${percent}%"></div>
              </div>
            </div>

            <div class="flex items-center gap-2 pt-2 border-t border-slate-100">
              <a href="event-detail.html?id=${event.id}" class="flex-1 text-center px-4 py-2 rounded-xl text-xs font-semibold bg-slate-100 text-slate-700 hover:bg-indigo-50 hover:text-indigo-600 transition-colors">
                Xem chi tiết
              </a>
              <a href="event-detail.html?id=${event.id}&register=1" class="flex-1 text-center px-4 py-2 rounded-xl text-xs font-semibold bg-indigo-600 text-white hover:bg-indigo-700 shadow-sm transition-colors">
                Đăng ký vé
              </a>
            </div>
          </div>
        </div>
      </div>
    `;
  });

  container.innerHTML = html;
}

// Xử lý bấm Bookmark Trái tim
function handleBookmarkClick(e, eventId) {
  e.preventDefault();
  e.stopPropagation();

  const isAdded = toggleBookmark(eventId);
  const events = getEvents();
  const ev = events.find((item) => item.id === eventId);
  const title = ev ? ev.title : "Sự kiện";

  if (isAdded) {
    showToast(`Đã lưu "${title}" vào mục Yêu thích!`, "success");
  } else {
    showToast(`Đã bỏ lưu "${title}" khỏi mục Yêu thích!`, "info");
  }

  // Cập nhật lại giao diện nút trái tim
  document.querySelectorAll(`.bookmark-btn-${eventId}`).forEach((btn) => {
    btn.classList.add("animate-heart-pop");
    btn.innerHTML = getHeartSymbol(isAdded, `w-5 h-5 transition-colors ${isAdded ? "text-rose-500" : ""}`);
    if (isAdded) {
      btn.classList.remove("text-slate-400");
      btn.classList.add("text-rose-500", "fill-rose-500");
    } else {
      btn.classList.remove("text-rose-500", "fill-rose-500");
      btn.classList.add("text-slate-400");
    }
    setTimeout(() => btn.classList.remove("animate-heart-pop"), 350);
  });

  // Nếu đang ở tab Yêu thích thì render lại danh sách
  if (activeStatus === "bookmarked") {
    renderEvents();
  }
}

function resetFilters() {
  activeCategory = 0;
  activeStatus = "all";
  searchKeyword = "";
  const searchInput = document.getElementById("search-input");
  if (searchInput) searchInput.value = "";
  renderCategoryPills();

  const statusTabs = document.querySelectorAll(".status-filter-tab");
  statusTabs.forEach((tab) => {
    if (tab.dataset.status === "all") {
      tab.classList.add("bg-indigo-600", "text-white", "font-semibold");
    } else {
      tab.classList.remove("bg-indigo-600", "text-white", "font-semibold");
      tab.classList.add("text-slate-600", "hover:bg-slate-100");
    }
  });

  renderEvents();
}

