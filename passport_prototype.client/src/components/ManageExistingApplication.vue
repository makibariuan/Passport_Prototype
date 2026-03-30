<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />

    <div class="dashboard-content">
      <h2 class="page-title">Manage Existing Applications</h2>

      <!-- Important Notice -->
      <div v-if="showNotice" class="important-box">
        <div class="important-top">
          <b class="important-header">IMPORTANT!</b>
          <button class="notice-close" @click="showNotice = false">&#x2715;</button>
        </div>
        <ol class="important-list">
          <li>
            Please print your application form after clicking the
            <strong>DOWNLOAD FORM</strong> button below. Your PC needs to have a suitable PDF reader
            installed such as Adobe Reader or Foxit Reader.
          </li>
          <li>Please only use A4-size paper to print your application form.</li>
          <li>Bring the printed application form on the date of your appointment.</li>
          <li>
            Do not sign the application form yet. Your signature should be affixed on site, in the
            presence of the passport officer.
          </li>
          <li>
            Please arrive at your chosen application site at least thirty minutes in advance of your
            scheduled time.
          </li>
        </ol>
      </div>

      <!-- Toolbar -->
      <div class="toolbar">
        <div class="search-wrap">
          <svg width="15" height="15" viewBox="0 0 20 20" fill="none">
            <circle cx="9" cy="9" r="6" stroke="#9ca3af" stroke-width="1.5" />
            <path d="m14 14 3 3" stroke="#9ca3af" stroke-width="1.5" stroke-linecap="round" />
          </svg>
          <input type="text" v-model="searchQuery" placeholder="Search..." class="search-input" />
        </div>

        <select class="filter-select" v-model="statusFilter">
          <option value="">Status</option>
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

        <button class="help-btn">
          <svg width="14" height="14" viewBox="0 0 20 20" fill="none">
            <circle cx="10" cy="10" r="8" stroke="#374151" stroke-width="1.5" />
            <path d="M10 14v-1" stroke="#374151" stroke-width="1.5" stroke-linecap="round" />
            <path
              d="M10 11c0-1.5 2.5-1.5 2.5-3.5a2.5 2.5 0 0 0-5 0"
              stroke="#374151"
              stroke-width="1.5"
              stroke-linecap="round"
            />
          </svg>
          Help
        </button>
      </div>

      <!-- Cards -->
      <div v-if="filteredApps.length" class="cards-grid">
        <div v-for="app in filteredApps" :key="app.id" class="app-card">
          <!-- Status badge -->
          <span :class="['badge', badgeClass(app.status)]">{{ statusLabel(app.status) }}</span>

          <!-- Profile info -->
          <div class="card-field">
            <div class="card-field-label">Profile Name</div>
            <div class="card-field-value">{{ app.name }}</div>
          </div>

          <div class="card-field">
            <div class="card-field-label">Scheduled Date</div>
            <div class="card-field-value">{{ app.scheduledDate }}</div>
          </div>

          <!-- Card footer -->
          <div class="card-footer">
            <button class="btn-manage" @click="openManageModal(app)">Manage</button>
            <button class="btn-icon-print" title="Download Form" @click="downloadForm(app)">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
                <path
                  d="M6 9V3h12v6"
                  stroke="#4a5568"
                  stroke-width="1.5"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                />
                <rect
                  x="3"
                  y="9"
                  width="18"
                  height="8"
                  rx="2"
                  stroke="#4a5568"
                  stroke-width="1.5"
                />
                <path
                  d="M7 21h10v-6H7v6z"
                  stroke="#4a5568"
                  stroke-width="1.5"
                  stroke-linejoin="round"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="empty-state">
        <svg
          width="48"
          height="48"
          viewBox="0 0 24 24"
          fill="none"
          style="margin-bottom: 12px; opacity: 0.3"
        >
          <rect x="3" y="3" width="18" height="18" rx="3" stroke="#06195e" stroke-width="1.5" />
          <path
            d="M8 12h8M8 8h5M8 16h3"
            stroke="#06195e"
            stroke-width="1.5"
            stroke-linecap="round"
          />
        </svg>
        <p>No applications found.</p>
      </div>
    </div>
  </div>

  <!-- ═══════════════════════════════════════════ -->
  <!-- MANAGE MODAL                               -->
  <!-- ═══════════════════════════════════════════ -->
  <Teleport to="body">
    <div v-if="selectedApp" class="overlay" @click.self="closeManageModal">
      <div class="modal">
        <button class="modal-close" @click="closeManageModal">&#x2715;</button>

        <!-- Header -->
        <div class="manage-modal-header">
          <span :class="['badge', badgeClass(selectedApp.status)]" style="margin-bottom: 0">
            {{ statusLabel(selectedApp.status) }}
          </span>
          <h3 class="manage-modal-title">{{ selectedApp.name }}</h3>
          <p class="manage-modal-ref">Ref: {{ selectedApp.ref }}</p>
        </div>

        <hr class="modal-divider" />

        <!-- Details -->
        <div class="modal-field">
          <div class="modal-field-label">Scheduled Date & Time</div>
          <div class="modal-field-value">{{ selectedApp.scheduledDate }}</div>
        </div>
        <div class="modal-field">
          <div class="modal-field-label">Application Site</div>
          <div class="modal-field-value">{{ selectedApp.site }}</div>
        </div>
        <div class="modal-field">
          <div class="modal-field-label">Application Type</div>
          <div class="modal-field-value">{{ selectedApp.type }}</div>
        </div>

        <hr class="modal-divider" />

        <!-- Required Documents checklist -->
        <div class="modal-docs-section">
          <div class="modal-field-label" style="margin-bottom: 10px">Required Documents</div>
          <div
            v-for="doc in selectedApp.documents"
            :key="doc.name"
            :class="['doc-item', doc.submitted ? 'doc-submitted' : 'doc-missing']"
          >
            <span class="doc-icon">
              <svg v-if="doc.submitted" width="14" height="14" viewBox="0 0 16 16" fill="none">
                <circle cx="8" cy="8" r="7" fill="#16a34a" />
                <path
                  d="m5 8 2 2 4-4"
                  stroke="#fff"
                  stroke-width="1.5"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                />
              </svg>
              <svg v-else width="14" height="14" viewBox="0 0 16 16" fill="none">
                <circle cx="8" cy="8" r="7" fill="#f59e0b" />
                <path d="M8 5v4M8 11v.5" stroke="#fff" stroke-width="1.5" stroke-linecap="round" />
              </svg>
            </span>
            <span class="doc-name">{{ doc.name }}</span>
            <span class="doc-status-tag">{{ doc.submitted ? "Submitted" : "Pending" }}</span>
          </div>
        </div>

        <hr class="modal-divider" />

        <!-- Actions -->
        <div class="modal-actions">
          <button class="btn-download-form" @click="downloadForm(selectedApp)">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none">
              <path
                d="M12 3v13M7 11l5 5 5-5"
                stroke="#fff"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
              <path d="M4 20h16" stroke="#fff" stroke-width="1.5" stroke-linecap="round" />
            </svg>
            Download Form
          </button>

          <template v-if="selectedApp.status === 1 || selectedApp.status === 0">
            <button class="btn-reschedule" @click="goToReschedule">Re-schedule</button>
            <button class="btn-cancel-appt" @click="confirmCancel(selectedApp)">Cancel</button>
          </template>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- Cancel Confirmation Modal -->
  <Teleport to="body">
    <div v-if="showCancelConfirm" class="overlay" @click.self="showCancelConfirm = false">
      <div class="modal modal-sm">
        <button class="modal-close" @click="showCancelConfirm = false">&#x2715;</button>
        <div class="confirm-icon">
          <svg width="36" height="36" viewBox="0 0 24 24" fill="none">
            <circle cx="12" cy="12" r="10" stroke="#dc2626" stroke-width="1.5" />
            <path d="M12 7v6M12 15v1" stroke="#dc2626" stroke-width="1.5" stroke-linecap="round" />
          </svg>
        </div>
        <h3 class="confirm-title">Cancel Appointment?</h3>
        <p class="confirm-msg">
          Are you sure you want to cancel the appointment for
          <strong>{{ cancelTarget?.name }}</strong> on
          <strong>{{ cancelTarget?.scheduledDate }}</strong
          >? This action cannot be undone.
        </p>
        <div class="modal-actions" style="margin-top: 20px">
          <button class="btn-secondary" @click="showCancelConfirm = false">Go Back</button>
          <button class="btn-cancel-appt" @click="executeCancel">Yes, Cancel</button>
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
const showNotice = ref(true);

const searchQuery = ref("");
const statusFilter = ref("");
const selectedApp = ref(null);
const showCancelConfirm = ref(false);
const cancelTarget = ref(null);

// ── Status map ─────────────────────────────────────────────────
const STATUS_MAP = {
  0: { label: "Pending Schedule", badge: "badge-pending" },
  1: { label: "Scheduled", badge: "badge-progress" },
  2: { label: "Captured", badge: "badge-progress" },
  3: { label: "Adjudication", badge: "badge-init" },
  4: { label: "Validated", badge: "badge-info" },
  5: { label: "Printed", badge: "badge-info" },
  6: { label: "Active / Card Issued", badge: "badge-done" },
  7: { label: "Schedule for Approval", badge: "badge-pending" },
  99: { label: "Rejected", badge: "badge-cancelled" },
  100: { label: "Display Only", badge: "badge-cancelled" },
};

const statusLabel = (code) => STATUS_MAP[code]?.label ?? `Status ${code}`;
const statusBadge = (code) => STATUS_MAP[code]?.badge ?? "badge-cancelled";

// ── Format helpers ─────────────────────────────────────────────
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
      scheduledDate: `${formatScheduleDate(a.schedule)}, ${formatScheduleTime(a.schedule)}`,
      site: "—",
      type: "—",
      status: a.status,
      documents: [],
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

// ── Filtering ─────────────────────────────────────────────────
const filteredApps = computed(() => {
  const q = searchQuery.value.toLowerCase();
  const s = statusFilter.value;
  return applications.value.filter(
    (a) =>
      (!q || a.name.toLowerCase().includes(q) || a.ref.toLowerCase().includes(q)) &&
      (!s || statusLabel(a.status) === s),
  );
});

// ── Badge class (numeric code) ─────────────────────────────────
const badgeClass = (code) => statusBadge(code);

// ── Modal ─────────────────────────────────────────────────────
const openManageModal = (app) => {
  selectedApp.value = app;
};
const closeManageModal = () => {
  selectedApp.value = null;
};

// ── Actions ───────────────────────────────────────────────────
const downloadForm = async (app) => {
  try {
    isLoading.value = true;
    dialogTitle.value = "Download";
    dialogMessage.value = `Application form for ${app.ref} will be downloaded.`;
    showDialog.value = true;
  } catch (err) {
    console.error(err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to download application form.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
    closeManageModal();
  }
};

const goToReschedule = () => {
  closeManageModal();
  dialogTitle.value = "Re-schedule";
  dialogMessage.value = "Redirecting to re-schedule page...";
  showDialog.value = true;
};

const confirmCancel = (app) => {
  cancelTarget.value = app;
  showCancelConfirm.value = true;
};

const executeCancel = async () => {
  try {
    isLoading.value = true;
    const idx = applications.value.findIndex((a) => a.id === cancelTarget.value.id);
    if (idx !== -1) applications.value[idx].status = 99;
    showCancelConfirm.value = false;
    closeManageModal();
    dialogTitle.value = "Cancelled";
    dialogMessage.value = `Appointment for ${cancelTarget.value.name} has been cancelled.`;
    showDialog.value = true;
  } catch (err) {
    console.error(err);
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to cancel appointment.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
    cancelTarget.value = null;
  }
};
</script>

<style scoped>
/* ── Layout ── */
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
  margin-bottom: 20px;
}

/* ── Important notice ── */
.important-box {
  background: #fff8e1;
  border: 1px solid #facc15;
  border-left: 5px solid #f59e0b;
  border-radius: 10px;
  padding: 14px 16px;
  margin-bottom: 20px;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.important-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.important-header {
  font-weight: 800;
  color: #92400e;
  font-size: 0.9rem;
}

.notice-close {
  background: none;
  border: none;
  color: #92400e;
  font-size: 16px;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  opacity: 0.7;
}

.notice-close:hover {
  opacity: 1;
}

.important-list {
  font-size: 0.84rem;
  color: #78350f;
  line-height: 1.7;
  padding-left: 18px;
  margin: 0;
}

.important-list li {
  margin-bottom: 2px;
}

/* ── Toolbar ── */
.toolbar {
  display: flex;
  gap: 10px;
  margin-bottom: 24px;
  align-items: center;
  flex-wrap: wrap;
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
  max-width: 280px;
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

.help-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  background: #fff;
  color: #374151;
  font-size: 13px;
  padding: 10px 14px;
  cursor: pointer;
  margin-left: auto;
  transition: background 0.15s;
}

.help-btn:hover {
  background: #f9fafb;
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
  gap: 0;
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
  padding: 4px 12px;
  border-radius: 20px;
  margin-bottom: 14px;
  width: fit-content;
}

.badge-init {
  background: #fed7aa;
  color: #9a3412;
}

.badge-progress {
  background: #fef3c7;
  color: #92400e;
}

.badge-pending {
  background: #dbeafe;
  color: #1e40af;
}

.badge-done {
  background: #d1fae5;
  color: #065f46;
}

.badge-cancelled {
  background: #f3f4f6;
  color: #374151;
}

/* ── Card fields ── */
.card-field {
  margin-bottom: 12px;
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

/* ── Card footer ── */
.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 16px;
  padding-top: 14px;
  border-top: 1px solid #e2e8f0;
}

.btn-manage {
  background: #06195e;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 9px 22px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-manage:hover {
  background: #0a2580;
}

.btn-icon-print {
  background: none;
  border: none;
  cursor: pointer;
  padding: 6px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  transition: background 0.15s;
}

.btn-icon-print:hover {
  background: #f1f5f9;
}

/* ── Empty state ── */
.empty-state {
  text-align: center;
  color: #718096;
  font-size: 14px;
  padding: 80px 0;
  display: flex;
  flex-direction: column;
  align-items: center;
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
  width: 420px;
  max-height: 90vh;
  overflow-y: auto;
  padding: 24px;
  position: relative;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
}

.modal-sm {
  width: 360px;
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

.modal-divider {
  border: none;
  border-top: 1px solid #e2e8f0;
  margin: 14px 0;
}

/* ── Manage modal header ── */
.manage-modal-header {
  margin-bottom: 4px;
  padding-right: 24px;
}

.manage-modal-title {
  font-size: 1.1rem;
  font-weight: 800;
  color: #06195e;
  margin: 8px 0 2px;
}

.manage-modal-ref {
  font-size: 12px;
  color: #718096;
  margin: 0;
  letter-spacing: 0.4px;
}

/* ── Modal fields ── */
.modal-field {
  margin-bottom: 12px;
}

.modal-field-label {
  font-size: 11px;
  font-weight: 700;
  color: #718096;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 3px;
}

.modal-field-value {
  font-size: 14px;
  font-weight: 600;
  color: #1a202c;
}

/* ── Documents checklist ── */
.modal-docs-section {
  margin-bottom: 4px;
}

.doc-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: 8px;
  margin-bottom: 6px;
  font-size: 13px;
}

.doc-submitted {
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
}

.doc-missing {
  background: #fffbeb;
  border: 1px solid #fde68a;
}

.doc-icon {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.doc-name {
  flex: 1;
  color: #1a202c;
  font-weight: 500;
}

.doc-status-tag {
  font-size: 11px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 10px;
}

.doc-submitted .doc-status-tag {
  background: #d1fae5;
  color: #065f46;
}

.doc-missing .doc-status-tag {
  background: #fef3c7;
  color: #92400e;
}

/* ── Modal actions ── */
.modal-actions {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.btn-download-form {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
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

.btn-download-form:hover {
  background: #0a2580;
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

.btn-cancel-appt {
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

.btn-cancel-appt:hover {
  background: #b91c1c;
}

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

/* ── Cancel confirm modal ── */
.confirm-icon {
  text-align: center;
  margin-bottom: 12px;
}

.confirm-title {
  font-size: 1.1rem;
  font-weight: 800;
  color: #06195e;
  text-align: center;
  margin-bottom: 8px;
}

.confirm-msg {
  font-size: 13px;
  color: #4a5568;
  text-align: center;
  line-height: 1.6;
}

.badge-info {
  background: #e0f2fe;
  color: #0369a1;
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

@media (max-width: 768px) {
  .app-layout {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto 1fr;
  }

  .leftMenu,
  .header,
  .dashboard-content {
    grid-column: 1;
  }

  .header {
    grid-row: 1;
  }
  .dashboard-content {
    grid-row: 2;
    padding: 20px 15px;
  }
  .leftMenu {
    grid-row: 3;
    width: 100%;
  }

  .search-wrap {
    max-width: 100%;
  }

  .search-wrap input {
    margin-bottom: 0;
  }

  .modal {
    width: 90vw;
  }
  .modal-sm {
    width: 90vw;
  }
}
</style>
