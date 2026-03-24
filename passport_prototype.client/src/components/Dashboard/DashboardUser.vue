<template>
  <div class="app-layout">
    <LeftMenu @navigate="onNavigate" class="leftMenu" />
    <Header class="header" />

    <div class="dashboard-content">
      <h1 class="welcome-title">Dashboard 🛡️</h1>
      <p class="role-info">
        Welcome, {{ firstName }}. Manage your ID application and biometrics appointment here.
      </p>

      <div class="summary-grid">
        <div class="status-card" :class="statusClass">
          <div class="status-icon">
            <span v-if="appData.status === 7">⏳</span>
            <span v-else-if="appData.status === 8">📅</span>
            <span v-else>📝</span>
          </div>
          <div class="status-info">
            <span class="status-number">{{ statusLabel }}</span>
            <span class="status-label">Application Status</span>
          </div>
        </div>

        <div class="status-card scheduled">
          <div class="status-icon">⏰</div>
          <div class="status-info">
            <span class="status-number">{{ formattedDate }}</span>
            <span class="status-label">Appointment Date</span>
          </div>
        </div>
      </div>

      <div class="action-section" v-if="!appData.dateSchedule || appData.status < 7">
        <h2 class="section-title">Schedule Your Biometrics</h2>
        <p class="detail">You haven't requested an appointment yet. Please select a preferred date for your biometrics capture.</p>
        <button @click="showScheduleModal = true" class="save-btn" style="width: auto; padding: 12px 25px;">
          Request Appointment Schedule
        </button>
      </div>

      <div class="info-box" v-if="appData.applicationCode">
        <p><strong>Application Code:</strong> {{ appData.applicationCode }}</p>
        <p class="text-sm text-gray-500">Please keep this code for your reference during your visit.</p>
      </div>

      <LoadingDialog :visible="isLoading" />
    </div>

    <DialogBox :show="showScheduleModal"
               title="Request Appointment Schedule"
               @close="showScheduleModal = false">
      <div class="appointment-setup">
        <div class="setup-intro">
          <p>Please select your preferred date for <strong>Biometrics Capture</strong> at our NPO-PSSIC center.</p>
        </div>

        <div class="date-selector-wrapper">
          <label class="input-label">Select Preferred Date</label>
          <input type="date"
                 v-model="preferredDate"
                 :min="minDate"
                 class="schedule-date-input" />
          <small class="hint-text">Appointments are available Monday to Friday, 8:00 AM - 5:00 PM.</small>
        </div>
      </div>

      <template #footer>
        <button @click="showScheduleModal = false" class="btn-cancel">Cancel</button>
        <button @click="submitScheduleRequest"
                class="btn-confirm"
                :disabled="isSubmitting || !preferredDate">
          {{ isSubmitting ? 'Processing...' : 'Confirm Appointment' }}
        </button>
      </template>
    </DialogBox>
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from "vue";
  import { useAuthStore } from "@/stores/auth";
  import LeftMenu from "@/components/LeftMenu.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import DialogBox from "@/components/DialogBox.vue"; // Ensure this is imported
  import api from "@/api";

  const auth = useAuthStore();
  const firstname = computed(() => auth.firstName ?? "Citizen");

  // State
  const isLoading = ref(false);
  const isSubmitting = ref(false);
  const showScheduleModal = ref(false);
  const preferredDate = ref("");
  const appData = ref({
    dateSchedule: null,
    status: 0,
    applicationCode: ""
  });

  // Minimum date for scheduling (tomorrow)
  const minDate = new Date(Date.now() + 86400000).toISOString().split('T')[0];

  const loadCitizenData = async () => {
    try {
      isLoading.value = true;
      const res = await api.get("/Citizen/my-schedule/date");
      appData.value = res.data;
    } catch (err) {
      console.error("Citizen data fetch error:", err);
    } finally {
      isLoading.value = false;
    }
  };

  const submitScheduleRequest = async () => {
    if (!preferredDate.value) return alert("Please select a date.");

    try {
      isSubmitting.value = true;
      // UPDATED: Pointing to the new Auto-Approve endpoint
      const res = await api.post("/Citizen/my-schedule/request-auto", {
        preferredDate: preferredDate.value
      });

      showScheduleModal.value = false;

      // Trigger a simple local update or re-fetch
      await loadCitizenData();

      // Optional: Alert the user that they should check their email
      alert("Success! Your appointment is confirmed. A barcode has been sent to your email.");
    } catch (err) {
      alert("Scheduling failed: " + (err.response?.data || "Server Error"));
    } finally {
      isSubmitting.value = false;
    }
  };

  const formattedDate = computed(() => {
    if (!appData.value.dateSchedule) return "Not Set";
    return new Date(appData.value.dateSchedule).toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  });

  // Computed status mapping for the new logic
  const statusLabel = computed(() => {
    if (!appData.value.dateSchedule) return "Initial Entry";
    switch (appData.value.status) {
      case 1: return "Scheduled & Confirmed";
      case 2: return "Biometrics Captured";
      default: return "In Progress";
    }
  });

  const statusClass = computed(() => {
    if (appData.value.status === 1) return 'scheduled'; // Blue/Indigo
    if (appData.value.status === 2) return 'captured';  // Green
    return 'for-schedule'; // Yellow
  });

  onMounted(() => {
    loadCitizenData();
  });
</script>

<style scoped>
  /* --- MODERN APP LAYOUT --- */
  .app-layout {
    display: grid;
    /* Aligned with the 280px sidebar width */
    grid-template-columns: 280px 1fr;
    grid-template-rows: auto 1fr;
    min-height: 100vh;
    background-color: #f4f7f9;
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
    padding: 40px;
    box-sizing: border-box;
    font-family: 'Inter', sans-serif;
    overflow-y: auto;
  }

  .welcome-title {
    font-size: 2.25rem;
    /* Aligned with Left Menu Official Blue */
    color: #06195e;
    margin-bottom: 8px;
    font-weight: 800;
    letter-spacing: -0.025em;
  }

  .role-info {
    margin-bottom: 40px;
    color: #718096;
    font-size: 1.1rem;
    font-weight: 400;
  }

  /* --- SUMMARY GRID --- */
  .summary-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 24px;
    margin-bottom: 40px;
  }

  /* --- MODERN STATUS CARDS --- */
  .status-card {
    background: white;
    padding: 24px;
    border-radius: 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    border: 1px solid rgba(0, 0, 0, 0.05);
    /* Changed from border-top to border-left to match the sidebar's active indicator */
    border-left: 6px solid #e2e8f0;
    position: relative;
    overflow: hidden;
  }

    .status-card.clickable {
      cursor: pointer;
    }

    .status-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    }

  .status-icon {
    font-size: 2.2rem;
    width: 64px;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #f8fafc;
    border-radius: 14px;
  }

  .status-info {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    text-align: right;
  }

  .status-number {
    font-size: 2.5rem;
    font-weight: 800;
    color: #06195e; /* Consistent Navy */
    line-height: 1;
  }

  .status-label {
    font-size: 0.85rem;
    color: #4a5568;
    margin-top: 8px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  /* --- COLOR CODING (Border-Left consistent with Sidebar Active State) --- */
  .status-card.for-schedule {
    border-left-color: #ecc94b;
  }
  /* Yellow */
  .status-card.approval {
    border-left-color: #4299e1;
  }
  /* Blue */
  .status-card.scheduled {
    border-left-color: #667eea;
  }
  /* Indigo */
  .status-card.captured {
    border-left-color: #48bb78;
  }
  /* Green */
  .status-card.validated {
    border-left-color: #38b2ac;
  }
  /* Teal */
  .status-card.for-printing {
    border-left-color: #9f7aea;
  }
  /* Purple */
  .status-card.active-cards {
    border-left-color: #f56565;
  }
  /* Red */

  /* Include the modern CSS you provided in your prompt here */
  /* (omitted for brevity, but use the "Modern App Layout" styles you shared) */

  .info-box {
    background: #edf2f7;
    padding: 15px;
    border-radius: 8px;
    margin-top: 20px;
    border-left: 4px solid #06195e;
  }

  .text-sm {
    font-size: 0.8rem;
  }

  /* --- APPOINTMENT SECTION STYLING --- */
  .action-section {
    background: white;
    padding: 30px;
    border-radius: 16px;
    border: 1px solid #e2e8f0;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.03);
    margin-bottom: 24px;
  }

  .section-title {
    color: #06195e;
    font-size: 1.4rem;
    font-weight: 700;
    margin-bottom: 10px;
  }

  .detail {
    color: #4a5568;
    line-height: 1.6;
    margin-bottom: 20px;
  }

  /* --- DIALOG CONTENT STYLING --- */
  .appointment-setup {
    padding: 10px 0;
  }

  .setup-intro {
    margin-bottom: 25px;
    color: #2d3748;
    font-size: 1.05rem;
  }

  .date-selector-wrapper {
    background: #f8fafc;
    padding: 20px;
    border-radius: 12px;
    border: 1px solid #e2e8f0;
  }

  .input-label {
    display: block;
    font-weight: 700;
    color: #06195e;
    margin-bottom: 8px;
    text-transform: uppercase;
    font-size: 0.85rem;
  }

  .schedule-date-input {
    width: 100%;
    padding: 12px 15px;
    border: 2px solid #cbd5e1;
    border-radius: 8px;
    font-size: 1.1rem;
    font-family: 'Inter', sans-serif;
    transition: border-color 0.2s;
  }

    .schedule-date-input:focus {
      outline: none;
      border-color: #06195e;
    }

  .hint-text {
    display: block;
    margin-top: 10px;
    color: #718096;
    font-size: 0.85rem;
  }

  /* --- DIALOG FOOTER BUTTONS --- */
  .btn-confirm {
    background: #06195e;
    color: white;
    border: none;
    padding: 12px 28px;
    border-radius: 8px;
    font-weight: 700;
    cursor: pointer;
    transition: all 0.2s;
  }

    .btn-confirm:hover:not(:disabled) {
      background: #0a2485;
      transform: translateY(-1px);
    }

    .btn-confirm:disabled {
      background: #cbd5e0;
      cursor: not-allowed;
    }

  .btn-cancel {
    background: transparent;
    color: #4a5568;
    border: 1px solid #cbd5e0;
    padding: 12px 28px;
    border-radius: 8px;
    font-weight: 600;
    margin-right: 12px;
    cursor: pointer;
  }

    .btn-cancel:hover {
      background: #f7fafc;
    }

  /* RESPONSIVENESS */
  @media (max-width: 1024px) {
    .app-layout {
      grid-template-columns: 80px 1fr;
    }

    .dashboard-content {
      padding: 25px;
    }
  }

  @media (max-width: 768px) {
    .app-layout {
      grid-template-columns: 1fr;
    }

    .leftMenu {
      display: none;
    }

    .summary-grid {
      grid-template-columns: 1fr;
    }
  }
</style>

