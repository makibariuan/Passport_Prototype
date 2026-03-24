<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />
    <Header class="header" />

    <div class="dashboard-content">
      <h1 class="welcome-title">ID Application Details 💳</h1>
      <p class="role-info">Manage your biometrics appointment and track your ID card progress.</p>

      <div class="id-details-wrapper">
        <div class="dashboard-grid">
          <div class="stat-card" :class="statusClass">
            <div class="stat-icon-wrapper">
              <span v-if="appData.status === 7">⏳</span>
              <span v-else-if="appData.status === 1">📅</span>
              <span v-else-if="appData.status === 2">📸</span>
              <span v-else>📝</span>
            </div>
            <div class="stat-content">
              <span class="stat-value">{{ statusLabel }}</span>
              <span class="stat-label">Application Status</span>
            </div>
          </div>

          <div class="stat-card stat-indigo">
            <div class="stat-icon-wrapper">⏰</div>
            <div class="stat-content">
              <span class="stat-value">
                {{ formattedDate }} {{ appData.timeSchedule ? '@ ' + appData.timeSchedule : '' }}
              </span>
              <span class="stat-label">Appointment Schedule</span>
            </div>
          </div>
        </div>

        <div class="action-section" v-if="appData.status !== 2">
          <h2 class="section-title">
            {{ appData.dateSchedule ? 'Reschedule Biometrics' : 'Schedule Your Biometrics' }}
          </h2>
          <p class="detail">
            {{
 appData.dateSchedule
        ? 'You have a pending appointment. You can request to change your schedule if needed.'
        : 'You haven\'t requested an appointment yet. Please select a preferred date for your biometrics capture.'
            }}
          </p>
          <button @click="showScheduleModal = true" class="btn-primary">
            {{ appData.dateSchedule ? 'Change Appointment Schedule' : 'Request Appointment Schedule' }}
          </button>
        </div>

        <div class="info-box" v-if="appData.applicationCode">
          <p><strong>Application Code:</strong> {{ appData.applicationCode }}</p>
          <p class="text-sm">Please keep this code for your reference during your visit.</p>
        </div>
      </div>
    </div>

    <DialogBox :show="showScheduleModal" title="ID Appointment" @close="closeScheduleModal">
      <div class="appointment-modal-content">
        <p class="modal-instruction">
          Select your preferred date and time for your Employee ID capturing.
          <br />
          <small style="color: #718096;">Note: This is subject to HR approval.</small>
        </p>

        <div class="auth-input-group" style="margin-top: 20px;">
          <div class="datetime-grid" style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
            <div>
              <label class="auth-input-label">Date</label>
              <input type="date" v-model="preferredDate" :min="minDate" class="auth-input" />
            </div>

            <div>
              <label class="auth-input-label">Time Slot</label>
              <select v-model="preferredTime" class="auth-input">
                <option value="" disabled>Select Time</option>
                <option v-for="slot in timeSlots" :key="slot" :value="slot">
                  {{ slot }}
                </option>
              </select>
            </div>
          </div>
          <small class="hint-text">Available Mon-Fri, 8:00 AM - 5:00 PM.</small>
        </div>

        <div class="button-group-row" style="display: flex; gap: 12px; margin-top: 30px; justify-content: flex-end;">
          <button @click="closeScheduleModal" class="btn-cancel" style="background: none; border: none; cursor: pointer; color: #64748b; font-weight: 600;">
            Cancel
          </button>
          <button @click="submitScheduleRequest" class="btn-primary" :disabled="isSubmitting || !preferredDate || !preferredTime">
            {{ isSubmitting ? 'Processing...' : 'Submit Request' }}
          </button>
        </div>
      </div>
    </DialogBox>

    <LoadingDialog :visible="isLoading" />
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from "vue";
  import api from "@/api";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import LeftMenu from "@/components/LeftMenu.vue";

  const isLoading = ref(false);
  const isSubmitting = ref(false);
  const showScheduleModal = ref(false);
  const preferredDate = ref("");
  const preferredTime = ref("");
  const appData = ref({
    dateSchedule: null,
    timeSchedule: null, // Ensure this exists in your backend response
    status: 0,
    applicationCode: ""
  });

  const minDate = new Date(Date.now() + 86400000).toISOString().split('T')[0];

  // Updated to point to /employee/ instead of /Citizen/
  const loadData = async () => {
    try {
      isLoading.value = true;
      const res = await api.get("/employee/my-schedule/date");
      appData.value = res.data;

      // Pre-fill the form with existing data if available
      if (appData.value.dateSchedule) {
        // Formats the ISO string to YYYY-MM-DD for the date input
        preferredDate.value = new Date(appData.value.dateSchedule)
          .toISOString()
          .split('T')[0];
      }

      if (appData.value.timeSchedule) {
        preferredTime.value = appData.value.timeSchedule;
      }
    } catch (err) {
      console.warn("Error fetching employee schedule info.");
    } finally {
      isLoading.value = false;
    }
  };

  const timeSlots = computed(() => {
    const slots = [];
    for (let hour = 8; hour < 17; hour++) {
      const period = hour >= 12 ? 'PM' : 'AM';
      const displayHour = hour > 12 ? hour - 12 : hour;
      slots.push(`${displayHour}:00 ${period}`);
      slots.push(`${displayHour}:30 ${period}`);
    }
    slots.push("5:00 PM");
    return slots;
  });

  const submitScheduleRequest = async () => {
    try {
      isSubmitting.value = true;
      const payload = {
        preferredDate: preferredDate.value,
        preferredTime: preferredTime.value
      };

      // Using the employee-specific request endpoint
      await api.post("/employee/my-schedule/request", payload);

      showScheduleModal.value = false;
      alert("Success! Your appointment request has been submitted for approval.");
      loadData();
    } catch (err) {
      alert("Scheduling failed: " + (err.response?.data || "Server Error"));
    } finally {
      isSubmitting.value = false;
    }
  };

  const formattedDate = computed(() => {
    if (!appData.value.dateSchedule) return "Not Set";
    return new Date(appData.value.dateSchedule).toLocaleDateString('en-PH', {
      month: 'short', day: 'numeric', year: 'numeric'
    });
  });

  const statusLabel = computed(() => {
    if (!appData.value.dateSchedule) return "No Schedule";
    switch (appData.value.status) {
      case 1: return "Confirmed";
      case 2: return "Captured";
      case 7: return "For Approval";
      default: return "In Progress";
    }
  });

  const statusClass = computed(() => {
    if (appData.value.status === 1) return 'stat-indigo';
    if (appData.value.status === 2) return 'stat-green';
    if (appData.value.status === 7) return 'stat-yellow';
    return 'stat-blue';
  });

  const closeScheduleModal = () => {
    showScheduleModal.value = false;
    // Reset to saved data
    if (appData.value.dateSchedule) {
      preferredDate.value = new Date(appData.value.dateSchedule).toISOString().split('T')[0];
    }
    preferredTime.value = appData.value.timeSchedule || "";
  };

  onMounted(loadData);
</script>

<style scoped>
  @import "@/assets/css/dashboard.css";

  .id-details-wrapper {
    width: 100%;
  }

  .action-section {
    background: white;
    padding: 24px;
    border-radius: 16px;
    border: 1px solid #e2e8f0;
    margin-top: 20px;
  }

  .btn-primary {
    background: #06195e;
    color: white;
    padding: 12px 24px;
    border-radius: 8px;
    font-weight: 700;
    border: none;
    cursor: pointer;
  }

  .info-box {
    background: #edf2f7;
    padding: 15px;
    border-radius: 8px;
    margin-top: 20px;
    border-left: 4px solid #06195e;
  }

  .input-label {
    display: block;
    font-weight: 700;
    color: #06195e;
    margin-bottom: 8px;
    font-size: 0.85rem;
  }

  .schedule-date-input {
    width: 100%;
    padding: 12px;
    border: 1px solid #cbd5e1;
    border-radius: 8px;
  }

  .btn-confirm {
    background: #06195e;
    color: white;
    padding: 10px 20px;
    border-radius: 6px;
    font-weight: 600;
  }

  .btn-cancel {
    margin-right: 10px;
    color: #64748b;
  }
</style>
