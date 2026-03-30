<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />

    <div class="dashboard-content">
      <h2 class="page-title">Application History</h2>

      <!-- Toolbar -->
      <div class="toolbar">
        <div class="search-wrap">
          <svg width="15" height="15" viewBox="0 0 20 20" fill="none">
            <circle cx="9" cy="9" r="6" stroke="#9ca3af" stroke-width="1.5" />
            <path d="m14 14 3 3" stroke="#9ca3af" stroke-width="1.5" stroke-linecap="round" />
          </svg>
          <input
            type="text"
            v-model="searchQuery"
            placeholder="Search by name or reference..."
            class="search-input"
          />
        </div>

        <select class="filter-select" v-model="statusFilter">
          <option value="">All Status</option>
          <option value="Pending Schedule">Pending Schedule</option>
          <option value="Scheduled">Scheduled</option>
          <option value="Captured">Captured</option>
          <option value="Adjudication">Adjudication</option>
          <option value="Validated">Validated</option>
          <option value="Printed">Printed</option>
          <option value="Active / Card Issued">Active / Card Issued</option>
          <option value="Schedule for Approval">Schedule for Approval</option>
          <option value="Rejected">Rejected</option>
        </select>
      </div>

      <!-- Cards Grid -->
      <div v-if="filteredApps.length" class="cards-grid">
        <div v-for="app in filteredApps" :key="app.ref" class="app-card">
          <span :class="['badge', badgeClass(app.status)]">{{ statusLabel(app.status) }}</span>

          <div class="barcode-wrap">
            <svg
              :viewBox="`0 0 ${barcodeWidth(app.ref)} 52`"
              xmlns="http://www.w3.org/2000/svg"
              preserveAspectRatio="none"
              class="barcode-svg"
            >
              <rect
                v-for="(bar, i) in barcodeBars(app.ref)"
                :key="i"
                :x="bar.x"
                y="0"
                :width="bar.w"
                height="52"
                fill="#1a202c"
              />
            </svg>
            <div class="barcode-num">{{ app.ref }}</div>
          </div>

          <hr class="divider" />

          <div class="card-field">
            <div class="card-field-label">Profile Name</div>
            <div class="card-field-value">{{ app.name }}</div>
          </div>
          <div class="card-field">
            <div class="card-field-label">Scheduled Date</div>
            <div class="card-field-value">{{ app.date }}</div>
          </div>

          <button class="details-btn" @click="openModal(app)">
            <svg width="14" height="14" viewBox="0 0 16 16" fill="none">
              <rect x="1" y="4" width="14" height="1.5" rx="1" fill="currentColor" />
              <rect x="1" y="7.25" width="14" height="1.5" rx="1" fill="currentColor" />
              <rect x="1" y="10.5" width="14" height="1.5" rx="1" fill="currentColor" />
            </svg>
            Details
          </button>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="empty-state">No applications found.</div>
    </div>
  </div>

  <!-- Modal Overlay -->
  <Teleport to="body">
    <div v-if="selectedApp" class="overlay" @click.self="closeModal">
      <div class="modal">
        <button class="modal-close" @click="closeModal">&#x2715;</button>

        <span :class="['badge', badgeClass(selectedApp.status)]">{{
          statusLabel(selectedApp.status)
        }}</span>

        <div class="modal-barcode">
          <svg
            :viewBox="`0 0 ${barcodeWidth(selectedApp.ref)} 56`"
            xmlns="http://www.w3.org/2000/svg"
            preserveAspectRatio="none"
            class="barcode-svg"
          >
            <rect
              v-for="(bar, i) in barcodeBars(selectedApp.ref)"
              :key="i"
              :x="bar.x"
              y="0"
              :width="bar.w"
              height="56"
              fill="#1a202c"
            />
          </svg>
          <div class="barcode-num">{{ selectedApp.ref }}</div>
        </div>

        <div class="modal-field">
          <div class="modal-field-label">Profile Name</div>
          <div class="modal-field-value">{{ selectedApp.name }}</div>
        </div>
        <div class="modal-field">
          <div class="modal-field-label">Site</div>
          <div class="modal-field-value">{{ selectedApp.site }}</div>
        </div>

        <hr class="modal-divider" />

        <div class="modal-field">
          <div class="modal-field-label">Scheduled Date</div>
          <div class="modal-field-value">{{ selectedApp.date }}</div>
        </div>
        <div class="modal-field">
          <div class="modal-field-label">Scheduled Time</div>
          <div class="modal-field-value">{{ selectedApp.time }}</div>
        </div>

        <div class="modal-actions">
          <button
            v-if="selectedApp.status === 6"
            class="btn-download"
            @click="downloadPDF(selectedApp)"
          >
            Download PDF
          </button>
          <template v-if="selectedApp.status === 1">
            <button class="btn-reschedule" @click="openRescheduleModal(selectedApp)">
              Re-schedule
            </button>
            <button class="btn-cancel" @click="cancelAppointment(selectedApp)">
              Cancel Appointment
            </button>
          </template>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- Reschedule Modal -->
  <Teleport to="body">
    <div v-if="showRescheduleModal" class="overlay" @click.self="closeRescheduleModal">
      <div class="modal modal-reschedule">
        <button class="modal-close" @click="closeRescheduleModal">&#x2715;</button>

        <div class="reschedule-header">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" style="flex-shrink: 0">
            <rect x="3" y="4" width="18" height="18" rx="3" stroke="#06195e" stroke-width="1.5" />
            <path d="M3 9h18" stroke="#06195e" stroke-width="1.5" />
            <path d="M8 2v4M16 2v4" stroke="#06195e" stroke-width="1.5" stroke-linecap="round" />
            <path d="M7 13h2v2H7z" fill="#06195e" />
            <path d="M11 13h2v2h-2z" fill="#06195e" />
            <path d="M15 13h2v2h-2z" fill="#06195e" />
          </svg>
          <div>
            <h3 class="reschedule-title">Re-schedule Appointment</h3>
            <p class="reschedule-ref">{{ rescheduleApp?.ref }}</p>
          </div>
        </div>

        <hr class="modal-divider" />

        <!-- Current appointment info -->
        <div class="reschedule-current">
          <div class="reschedule-current-label">Current Appointment</div>
          <div class="reschedule-current-row">
            <span>{{ rescheduleApp?.date }}</span>
            <span class="reschedule-dot">·</span>
            <span>{{ rescheduleApp?.time }}</span>
          </div>
          <div class="reschedule-site">{{ rescheduleApp?.site }}</div>
        </div>

        <hr class="modal-divider" />

        <!-- New date -->
        <div class="rs-field">
          <label class="rs-label">New Date<span class="required-star">*</span></label>
          <input
            type="date"
            v-model="rescheduleDate"
            class="rs-input"
            :class="{ 'rs-input-error': rsValidation && !rescheduleDate }"
            :min="minDate"
          />
        </div>

        <!-- Site selection -->
        <div class="rs-field">
          <label class="rs-label">Site<span class="required-star">*</span></label>
          <select
            v-model="reschedulesite"
            class="rs-input rs-select"
            :class="{ 'rs-input-error': rsValidation && !reschedulesite }"
          >
            <option value="">— Select Site —</option>
            <option v-for="site in dfaSites" :key="site" :value="site">{{ site }}</option>
          </select>
        </div>

        <!-- Time slot selection -->
        <div class="rs-field">
          <label class="rs-label">Time Slot<span class="required-star">*</span></label>
          <div class="time-slots">
            <button
              v-for="slot in timeSlots"
              :key="slot"
              type="button"
              :class="['time-slot-btn', { active: rescheduleTime === slot }]"
              @click="rescheduleTime = slot"
            >
              {{ slot }}
            </button>
          </div>
          <p v-if="rsValidation && !rescheduleTime" class="rs-error-msg">
            Please select a time slot.
          </p>
        </div>

        <div class="modal-actions" style="margin-top: 20px">
          <button class="btn-secondary" @click="closeRescheduleModal">Cancel</button>
          <button class="btn-confirm-reschedule" @click="confirmReschedule">
            Confirm Re-schedule
          </button>
        </div>
      </div>
    </div>
  </Teleport>

  <DialogBox
    :show="showDialog"
    :title="dialogTitle"
    :message="dialogMessage"
    @close="showDialog = false"
  />
  <LoadingDialog :visible="isLoading" />
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import axios from "axios";
import LeftMenu from "@/components/LeftMenu.vue";
import DialogBox from "@/components/DialogBox.vue";
import LoadingDialog from "./LoadingDialog.vue";
import { useAuthStore } from "../stores/auth";
import { BACKEND_DOMAIN } from "@/configs/config";

const Auth = useAuthStore();
const isLoading = ref(false);
const showDialog = ref(false);
const dialogTitle = ref("");
const dialogMessage = ref("");

const searchQuery = ref("");
const statusFilter = ref("");
const selectedApp = ref(null);

// ── Reschedule modal state ─────────────────────────────────────
const showRescheduleModal = ref(false);
const rescheduleApp = ref(null);
const rescheduleDate = ref("");
const reschedulesite = ref("");
const rescheduleTime = ref("");
const rsValidation = ref(false);

const minDate = new Date(Date.now() + 86400000).toISOString().split("T")[0];

const dfaSites = [
  "DFA Manila (Aseana)",
  "DFA Cebu",
  "DFA Davao",
  "DFA Quezon City",
  "DFA Pampanga",
  "DFA Iloilo",
  "DFA Cagayan de Oro",
];

const timeSlots = [
  "8:00 AM - 8:30 AM",
  "8:30 AM - 9:00 AM",
  "9:00 AM - 9:30 AM",
  "9:30 AM - 10:00 AM",
  "10:00 AM - 10:30 AM",
  "10:30 AM - 11:00 AM",
  "1:00 PM - 1:30 PM",
  "1:30 PM - 2:00 PM",
  "2:00 PM - 2:30 PM",
  "2:30 PM - 3:00 PM",
];

// ── Status map ─────────────────────────────────────────────────
const STATUS_MAP = {
  0: { label: "Pending Schedule", badge: "badge-pending" },
  1: { label: "Scheduled", badge: "badge-progress" },
  2: { label: "Captured", badge: "badge-progress" },
  3: { label: "Adjudication", badge: "badge-warning" },
  4: { label: "Validated", badge: "badge-info" },
  5: { label: "Printed", badge: "badge-info" },
  6: { label: "Active / Card Issued", badge: "badge-done" },
  7: { label: "Schedule for Approval", badge: "badge-pending" },
  99: { label: "Rejected", badge: "badge-cancelled" },
  100: { label: "Display Only", badge: "badge-cancelled" },
};

const statusLabel = (code) => STATUS_MAP[code]?.label ?? `Status ${code}`;
const statusBadge = (code) => STATUS_MAP[code]?.badge ?? "badge-cancelled";

// ── Format schedule ────────────────────────────────────────────
const formatScheduleDate = (iso) => {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("en-PH", {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
};

const formatScheduleTime = (iso) => {
  if (!iso) return "—";
  return new Date(iso).toLocaleTimeString("en-PH", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: true,
  });
};

// ── Applications data ──────────────────────────────────────────
const applications = ref([]);

const fetchApplications = async () => {
  try {
    isLoading.value = true;
    const res = await axios.get(`${BACKEND_DOMAIN}/api/Application/My-Applications`, {
      headers: { Authorization: `Bearer ${Auth.token}` },
    });

    applications.value = res.data.map((a) => ({
      id: a.applicationId,
      ref: a.barcode ?? a.barcodePath ?? `APP-${a.applicationId}`,
      name: a.profileName,
      site: a.site, // not in list response, shown in detail modal if needed
      date: formatScheduleDate(a.schedule),
      time: formatScheduleTime(a.schedule),
      status: a.status, // keep as numeric code
    }));
  } catch (err) {
    console.error(err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to load applications.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

onMounted(fetchApplications);

// ── Filtering ──────────────────────────────────────────────────
const filteredApps = computed(() => {
  const q = searchQuery.value.toLowerCase();
  const s = statusFilter.value;
  return applications.value.filter(
    (a) =>
      (!q || a.name.toLowerCase().includes(q) || a.ref.toLowerCase().includes(q)) &&
      (!s || statusLabel(a.status) === s),
  );
});

// ── Badge class (now uses numeric code) ───────────────────────
const badgeClass = (code) => statusBadge(code);

// ── Barcode helpers ────────────────────────────────────────────
const barcodeBars = (ref) => {
  const bars = [];
  let x = 0;
  for (let i = 0; i < ref.length * 2 + 30; i++) {
    const w = i % 3 === 0 ? 3 : i % 5 === 0 ? 2 : 1;
    if (i % 2 === 0) bars.push({ x, w });
    x += w + 1;
  }
  return bars;
};

const barcodeWidth = (ref) => {
  let x = 0;
  for (let i = 0; i < ref.length * 2 + 30; i++) {
    x += (i % 3 === 0 ? 3 : i % 5 === 0 ? 2 : 1) + 1;
  }
  return x;
};

// ── Modal ──────────────────────────────────────────────────────
const openModal = (app) => {
  selectedApp.value = app;
};
const closeModal = () => {
  selectedApp.value = null;
};

// ── Actions ───────────────────────────────────────────────────
const downloadPDF = async (app) => {
  try {
    isLoading.value = true;
    dialogTitle.value = "Download";
    dialogMessage.value = `PDF for ${app.ref} will be downloaded.`;
    showDialog.value = true;
  } catch (err) {
    console.error(err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to download PDF.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
    closeModal();
  }
};

const openRescheduleModal = (app) => {
  rescheduleApp.value = app;
  rescheduleDate.value = "";
  reschedulesite.value = app.site;
  rescheduleTime.value = "";
  rsValidation.value = false;
  showRescheduleModal.value = true;
  closeModal();
};

const closeRescheduleModal = () => {
  showRescheduleModal.value = false;
  rescheduleApp.value = null;
};

const confirmReschedule = async () => {
  rsValidation.value = true;
  if (!rescheduleDate.value || !reschedulesite.value || !rescheduleTime.value) return;

  try {
    isLoading.value = true;
    const idx = applications.value.findIndex((a) => a.id === rescheduleApp.value.id);
    if (idx !== -1) {
      const d = new Date(rescheduleDate.value);
      applications.value[idx].date = d.toLocaleDateString("en-PH", {
        day: "numeric",
        month: "long",
        year: "numeric",
      });
      applications.value[idx].time = rescheduleTime.value;
      applications.value[idx].site = reschedulesite.value;
    }
    closeRescheduleModal();
    dialogTitle.value = "Re-scheduled";
    dialogMessage.value = "Your appointment has been successfully re-scheduled.";
    showDialog.value = true;
  } catch (err) {
    console.error(err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to re-schedule appointment.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
  }
};

const cancelAppointment = async (app) => {
  try {
    isLoading.value = true;
    const idx = applications.value.findIndex((a) => a.id === app.id);
    if (idx !== -1) applications.value[idx].status = 99;
    dialogTitle.value = "Cancelled";
    dialogMessage.value = `Appointment ${app.ref} has been cancelled.`;
    showDialog.value = true;
  } catch (err) {
    console.error(err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to cancel appointment.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
    closeModal();
  }
};
</script>

<style scoped>
.app-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  grid-template-rows: auto 1fr;
  height: 100vh;
  background-color: #f4f7f9;
  overflow: hidden;
}

.leftMenu {
  grid-column: 1;
  grid-row: 1 / span 2;
  z-index: 100;
}

.header {
  grid-column: 2;
  grid-row: 1;
  z-index: 90;
}

.dashboard-content {
  grid-column: 2;
  grid-row: 2;
  padding: 30px;
  box-sizing: border-box;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.page-title {
  font-size: 1.8rem;
  color: #06195e;
  font-weight: 800;
  margin-bottom: 24px;
}

/* ── Toolbar ── */
.toolbar {
  display: flex;
  gap: 10px;
  margin-bottom: 24px;
  flex-wrap: wrap;
  align-items: center;
}

.search-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #fff;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  padding: 0 12px;
  flex: 1;
  max-width: 320px;
}

.search-input {
  border: none;
  background: transparent;
  font-size: 14px;
  color: #1a202c;
  padding: 10px 0;
  outline: none;
  width: 100%;
  margin-bottom: 0;
}

.filter-select {
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: #fff;
  color: #1a202c;
  font-size: 14px;
  padding: 10px 36px 10px 12px;
  outline: none;
  cursor: pointer;
  appearance: none;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-repeat: no-repeat;
  background-position: right 8px center;
  background-size: 16px;
}

/* ── Cards ── */
.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.app-card {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 2px 8px rgba(6, 25, 94, 0.05);
  transition: box-shadow 0.2s;
}

.app-card:hover {
  box-shadow: 0 4px 16px rgba(6, 25, 94, 0.09);
}

/* ── Badges ── */
.badge {
  display: inline-block;
  font-size: 12px;
  font-weight: 700;
  padding: 4px 14px;
  border-radius: 20px;
  margin-bottom: 14px;
  width: fit-content;
}

.badge-pending {
  background: #dbeafe;
  color: #1e40af;
}

.badge-progress {
  background: #fef3c7;
  color: #92400e;
}

.badge-done {
  background: #d1fae5;
  color: #065f46;
}

.badge-cancelled {
  background: #f3f4f6;
  color: #374151;
}

/* ── Barcode ── */
.barcode-wrap {
  text-align: center;
  margin-bottom: 14px;
}

.barcode-svg {
  width: 100%;
  height: 52px;
}

.barcode-num {
  font-size: 12px;
  color: #718096;
  margin-top: 4px;
  letter-spacing: 0.5px;
}

.divider,
.modal-divider {
  border: none;
  border-top: 1px solid #e2e8f0;
  margin: 10px 0;
}

/* ── Card fields ── */
.card-field {
  margin-bottom: 10px;
}

.card-field-label {
  font-size: 12px;
  color: #718096;
  margin-bottom: 2px;
}

.card-field-value {
  font-size: 14px;
  font-weight: 700;
  color: #1a202c;
}

.details-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 16px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: transparent;
  color: #374151;
  font-size: 13px;
  padding: 8px 14px;
  cursor: pointer;
  width: fit-content;
  transition: background 0.15s;
}

.details-btn:hover {
  background: #f9fafb;
}

/* ── Empty state ── */
.empty-state {
  text-align: center;
  color: #718096;
  font-size: 14px;
  padding: 80px 0;
}

/* ── Overlay & Modal ── */
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
}

.modal {
  background: #fff;
  border-radius: 14px;
  width: 340px;
  padding: 24px;
  position: relative;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
}

.modal-close {
  position: absolute;
  top: 14px;
  right: 14px;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 18px;
  color: #9ca3af;
  line-height: 1;
}

.modal-close:hover {
  color: #374151;
}

.modal-barcode {
  text-align: center;
  margin: 14px 0 16px;
}

.modal-field {
  margin-bottom: 12px;
}

.modal-field-label {
  font-size: 12px;
  color: #718096;
  margin-bottom: 2px;
}

.modal-field-value {
  font-size: 14px;
  font-weight: 700;
  color: #1a202c;
}

/* ── Modal action buttons ── */
.modal-actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
  flex-wrap: wrap;
}

.btn-download {
  flex: 1;
  padding: 11px;
  border-radius: 8px;
  border: none;
  background: #16a34a;
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-download:hover {
  background: #15803d;
}

.btn-reschedule {
  flex: 1;
  padding: 11px;
  border-radius: 8px;
  border: none;
  background: #2563eb;
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-reschedule:hover {
  background: #1d4ed8;
}

.btn-cancel {
  flex: 1;
  padding: 11px;
  border-radius: 8px;
  border: none;
  background: #dc2626;
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-cancel:hover {
  background: #b91c1c;
}

/* ── Responsive ── */
@media (max-width: 1200px) {
  .app-layout {
    grid-template-columns: 20vw 1fr;
  }
}

@media (max-width: 992px) {
  .app-layout {
    grid-template-columns: 80px 1fr;
  }
}

/* ── Reschedule Modal ── */
.modal-reschedule {
  width: 420px;
  max-height: 90vh;
  overflow-y: auto;
}

.reschedule-header {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 14px;
}

.reschedule-title {
  font-size: 1rem;
  font-weight: 700;
  color: #06195e;
  margin: 0 0 2px;
}

.reschedule-ref {
  font-size: 12px;
  color: #718096;
  margin: 0;
  letter-spacing: 0.4px;
}

.reschedule-current {
  background: #f0f7ff;
  border: 1px solid #bfdbfe;
  border-radius: 8px;
  padding: 10px 14px;
  margin-bottom: 4px;
}

.reschedule-current-label {
  font-size: 11px;
  font-weight: 700;
  color: #1d4ed8;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
}

.reschedule-current-row {
  font-size: 13px;
  font-weight: 700;
  color: #1a202c;
  display: flex;
  align-items: center;
  gap: 6px;
}

.reschedule-dot {
  color: #718096;
}

.reschedule-site {
  font-size: 12px;
  color: #4a5568;
  margin-top: 2px;
}

/* ── Reschedule form fields ── */
.rs-field {
  margin-bottom: 16px;
}

.rs-label {
  display: block;
  font-size: 12px;
  font-weight: 700;
  color: #4a5568;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  margin-bottom: 6px;
}

.required-star {
  color: #e53935;
  font-weight: 800;
  margin-left: 2px;
}

.rs-input {
  width: 100%;
  padding: 9px 12px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  font-size: 0.9rem;
  color: #1a202c;
  background: #fff;
  box-sizing: border-box;
  transition:
    border-color 0.18s,
    box-shadow 0.18s;
  outline: none;
}

.rs-input:focus {
  border-color: #06195e;
  box-shadow: 0 0 0 3px rgba(6, 25, 94, 0.1);
}

.rs-select {
  appearance: none;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-repeat: no-repeat;
  background-position: right 10px center;
  background-size: 16px;
  padding-right: 36px;
  cursor: pointer;
}

.rs-input-error {
  border-color: #e53e3e !important;
  background: #fff5f5 !important;
}

.rs-error-msg {
  font-size: 12px;
  color: #e53e3e;
  margin-top: 4px;
}

/* ── Time slots grid ── */
.time-slots {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.time-slot-btn {
  padding: 9px 8px;
  border: 1.5px solid #d1d9e6;
  border-radius: 8px;
  background: #f8fafc;
  color: #718096;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
  text-align: center;
}

.time-slot-btn:hover {
  border-color: #06195e;
  color: #06195e;
  background: #eff6ff;
}

.time-slot-btn.active {
  background: #06195e;
  color: #fff;
  border-color: #06195e;
}

/* ── Confirm reschedule buttons ── */
.btn-secondary {
  flex: 1;
  padding: 11px;
  border-radius: 8px;
  border: 1.5px solid #d1d9e6;
  background: transparent;
  color: #374151;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-secondary:hover {
  background: #f9fafb;
}

.btn-confirm-reschedule {
  flex: 2;
  padding: 11px;
  border-radius: 8px;
  border: none;
  background: #06195e;
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-confirm-reschedule:hover {
  background: #0a2580;
}

.badge-warning {
  background: #fde68a;
  color: #92400e;
}

.badge-info {
  background: #e0f2fe;
  color: #0369a1;
}

@media (max-width: 768px) {
  .modal-reschedule {
    width: 90vw;
  }

  .time-slots {
    grid-template-columns: 1fr 1fr;
  }
}
</style>
