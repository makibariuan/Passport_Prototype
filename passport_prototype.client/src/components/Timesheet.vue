<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />
    <div class="dashboard-content">
      <h1 class="page-title">My Timesheet</h1>

      <div class="attendance-hero-grid">
        <div class="data-section-card shadow-sm">
          <div class="card-header-row">
            <h3 class="sub-title" style="border:none; margin:0;">Today's Progress</h3>
            <span :class="['status-badge', todayLog.hasLog ? 'activecards' : 'schedule']">
              {{ todayLog.hasLog ? 'Active' : 'Not Clocked In' }}
            </span>
          </div>

          <div class="punch-grid">
            <div class="punch-item">
              <p class="p-label">Morning In</p>
              <p class="p-time">{{ todayLog.morningIn || '--:--' }}</p>
            </div>
            <div class="punch-item">
              <p class="p-label">Morning Out</p>
              <p class="p-time">{{ todayLog.morningOut || '--:--' }}</p>
            </div>
            <div class="punch-item">
              <p class="p-label">Afternoon In</p>
              <p class="p-time">{{ todayLog.afternoonIn || '--:--' }}</p>
            </div>
            <div class="punch-item">
              <p class="p-label">Afternoon Out</p>
              <p class="p-time">{{ todayLog.afternoonOut || '--:--' }}</p>
            </div>
          </div>

          <div class="total-bar">
            <span>Regular: <strong>{{ todayLog.totalHours || 0 }}h</strong></span>
            <span class="divider">|</span>
            <span>OT: <strong>{{ todayLog.otHours || 0 }}h</strong></span>
          </div>
        </div>

        <!--<div class="data-section-card shadow-sm action-card">
          <h3 class="sub-title" style="border:none; margin-bottom: 10px;">Smart Clock</h3>
          <p class="clock-desc">Remote punch for WFH or field sessions.</p>

          <button @click="handleRemoteClock" :disabled="isLoading" class="action-btn view-btn clock-wide-btn">
            <span v-if="!isLoading">🕒 Punch Log</span>
            <span v-else>Processing...</span>
          </button>

          <p class="helper-text">Action is automatically determined by your schedule.</p>
        </div>-->
      </div>

      <div class="tab-container">
        <button v-for="tab in ['Biometric Logs', 'Weekly Schedule']"
                :key="tab"
                :class="['tab-btn', { active: activeTab === tab }]"
                @click="activeTab = tab; if(tab === 'Weekly Schedule') loadData();">
          {{ tab }}
        </button>
      </div>

      <div class="data-section-card">
        <transition name="fade-slide" mode="out-in">
          <div :key="activeTab">

            <div v-if="activeTab === 'Biometric Logs'" class="table-container">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Mode</th>
                    <th>Department</th>
                    <th>Device</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(log, index) in attendanceRecord" :key="index">
                    <td>{{ log.date }}</td>
                    <td>{{ log.time }}</td>
                    <td>
                      <span :class="['status-badge', log.mode === 'IN' ? 'captured' : 'rejected']">
                        {{ log.mode }}
                      </span>
                    </td>
                    <td>{{ log.departmentName }}</td>
                    <td><small style="color: #94a3b8">{{ log.biometricDeviceID }}</small></td>
                  </tr>
                  <tr v-if="attendanceRecord.length === 0">
                    <td colspan="5" class="table-loading-state">No biometric logs found for this period.</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <div v-else-if="activeTab === 'Weekly Schedule'">
              <div v-if="weeklySchedule && weeklySchedule.length > 0" class="schedule-flex-grid">
                <div v-for="day in weeklySchedule" :key="day.dayName"
                     :class="['day-card', { 'is-rest': day.isRestDay, 'is-today': day.dayName === currentDayName }]">

                  <div class="day-header">
                    <h4 class="day-title">
                      {{ day.dayName }}
                      <span v-if="day.dayName === currentDayName" class="today-tag">TODAY</span>
                    </h4>
                    <span v-if="day.isRestDay" class="rest-badge">OFF</span>
                  </div>

                  <div v-if="!day.isRestDay" class="day-details">
                    <div class="shift-group">
                      <div class="info-item">
                        <label>☀️ Morning</label>
                        <span>{{ day.morningShift }}</span>
                      </div>
                      <div class="info-item">
                        <label>🌤️ Afternoon</label>
                        <span>{{ day.afternoonShift }}</span>
                      </div>
                    </div>
                  </div>
                  <div v-else class="rest-content">
                    <div class="rest-icon">🏠</div>
                    <p class="rest-text">Rest Day</p>
                  </div>
                </div>
              </div>

              <div v-else class="empty-schedule-state">
                <div class="empty-icon">📅</div>
                <p class="empty-text">No work schedule has been created by HR yet.</p>
                <small>Current Date: {{ new Date().toLocaleDateString() }}</small>
              </div>
            </div>

          </div>
        </transition>
      </div>

      <DialogBox :show="showDialog" :title="dialogTitle" :message="dialogMessage" @close="showDialog = false" />
      <LoadingDialog :visible="isLoading" />
    </div>
  </div>
</template>

<script setup>
  console.log("⚡ Script Setup is initializing...");
  import { ref, onMounted, computed, watch } from "vue";
  import api from "@/api";
  import { useAuthStore } from "@/stores/auth";
  import LeftMenu from '@/components/LeftMenu.vue'
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";

  const auth = useAuthStore();

  // State
  const activeTab = ref("Biometric Logs");
  const isLoading = ref(false);
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const attendanceRecord = ref([]);
  const weeklySchedule = ref([]);
  const todayLog = ref({
    morningIn: "--:--", morningOut: "--:--",
    afternoonIn: "--:--", afternoonOut: "--:--",
    totalHours: 0, otHours: 0, hasLog: false
  });

  const dummyLogs = [
    { "employeeID": "1140009", "workDate": "2026-02-02", "morningIn": "07:49:23", "morningOut": null, "afternoonIn": null, "afternoonOut": "17:02:01", "attendanceStatus": "Present" },
    { "employeeID": "1140021", "workDate": "2026-02-02", "morningIn": "07:32:14", "morningOut": null, "afternoonIn": null, "afternoonOut": "17:00:19", "attendanceStatus": "Present" },
    { "employeeID": "1140050", "workDate": "2026-02-02", "morningIn": "07:30:16", "morningOut": null, "afternoonIn": null, "afternoonOut": "17:04:07", "attendanceStatus": "Present" },
    //{ "employeeID": "1140124", "workDate": "2026-02-02", "morningIn": "06:31:13", "morningOut": null, "afternoonIn": null, "afternoonOut": "17:00:07", "attendanceStatus": "Present" }
    // ... add more from your list as needed
  ];

  // Date Logic
  const currentDayName = new Intl.DateTimeFormat('en-US', { weekday: 'long' }).format(new Date());

  // Computed
  const employeeId = computed(() => auth.employeeID);

  const todaySchedule = computed(() => {
    return weeklySchedule.value.find(s => s.dayName === currentDayName) || null;
  });

  // Watcher: Replaces onMounted to handle the dynamic ID from the JWT
  watch(
    employeeId,
    (newId) => {
      console.log("👀 Watcher observed employeeID:", newId);
      // Trigger loadData regardless, because fetchSchedule doesn't need the ID in the URL
      loadData();
    },
    { immediate: true }
  );

  async function loadData() {
    isLoading.value = true;
    try {
      // We still try to fetch real data, but we'll fallback to dummy if needed
      await Promise.allSettled([
        fetchTodaySummary(),
        fetchSchedule(),
        fetchAttendance()
      ]);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchAttendance() {
    try {
      // Mapping Dummy Data to the Table Structure
      // Since dummy data has multiple punches per row, we "flatten" them for the biometric list
      const flattenedLogs = [];

      dummyLogs.forEach(log => {
        if (log.morningIn) {
          flattenedLogs.push({
            date: log.workDate,
            time: log.morningIn,
            mode: 'IN',
            departmentName: 'General HQ',
            biometricDeviceID: 'DEV-01'
          });
        }
        if (log.afternoonOut) {
          flattenedLogs.push({
            date: log.workDate,
            time: log.afternoonOut,
            mode: 'OUT',
            departmentName: 'General HQ',
            biometricDeviceID: 'DEV-01'
          });
        }
      });

      attendanceRecord.value = flattenedLogs;

      // Update Today's Progress using the first record as an example
      const myDummy = dummyLogs[0];
      todayLog.value = {
        morningIn: myDummy.morningIn || "--:--",
        morningOut: myDummy.morningOut || "--:--",
        afternoonIn: myDummy.afternoonIn || "--:--",
        afternoonOut: myDummy.afternoonOut || "--:--",
        totalHours: 8,
        otHours: 0,
        hasLog: true
      };

    } catch (e) {
      console.error("Error setting dummy data", e);
    }
  }

  async function fetchTodaySummary() {
    try {
      const res = await api.get('/employee/my-attendance-today');
      if (res.data?.hasLog) {
        todayLog.value = res.data;
        console.log("✅ Today's summary loaded.");
      }
    } catch (e) { console.warn("Today Summary unavailable"); }
  }

  async function fetchSchedule() {
    try {
      const res = await api.get('/employee/my-schedule');
      const rawList = res.data.weeklySchedule || res.data.WeeklySchedule;
      if (Array.isArray(rawList)) {
        weeklySchedule.value = rawList;
        console.log("✅ Weekly schedule loaded.");
      }
    } catch (e) { console.error("Schedule API error:", e.message); }
  }

  async function handleRemoteClock() {
    try {
      isLoading.value = true;
      const res = await api.post('/employee/attendance/remote-clockin');
      dialogTitle.value = "Punch Successful";
      dialogMessage.value = `${res.data.message} at ${res.data.time}`;
      showDialog.value = true;
      await loadData();
    } catch (err) {
      dialogTitle.value = "Action Failed";
      dialogMessage.value = err.response?.data?.message || "Check your internet or schedule.";
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  }
</script>

<style scoped>
  /* 1. LAYOUT */
  .app-layout {
    display: grid;
    grid-template-columns: 280px 1fr;
    grid-template-rows: auto 1fr;
    height: 100vh;
    background-color: #f4f7f9;
  }

  .leftMenu {
    grid-column: 1;
    grid-row: 1 / span 2;
    z-index: 100;
  }

  .dashboard-content {
    grid-column: 2;
    grid-row: 2;
    padding: 30px;
    box-sizing: border-box;
    overflow-y: auto;
  }

  /* 2. TITLES */
  .page-title {
    font-size: 1.8rem;
    color: #06195e;
    font-weight: 800;
    margin-bottom: 25px;
  }

  /* 3. HERO GRID (Punches & Action) */
  .attendance-hero-grid {
    display: grid;
    grid-template-columns: 1.5fr 1fr;
    gap: 20px;
    margin-bottom: 30px;
  }

  .card-header-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
  }

  .punch-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 12px;
    margin-bottom: 20px;
  }

  .punch-item {
    background: #f8fafc;
    padding: 12px;
    border-radius: 8px;
    text-align: center;
    border: 1px solid #e2e8f0;
  }

  .p-label {
    font-size: 0.65rem;
    color: #64748b;
    font-weight: 700;
    text-transform: uppercase;
    margin-bottom: 4px;
  }

  .p-time {
    font-size: 1.1rem;
    font-weight: 700;
    color: #06195e;
    margin: 0;
  }

  .total-bar {
    border-top: 1px solid #f1f5f9;
    padding-top: 15px;
    display: flex;
    justify-content: center;
    gap: 20px;
    color: #475569;
    font-weight: 600;
  }

  /* 4. CLOCK BUTTON */
  .clock-wide-btn {
    width: 100%;
    height: 50px !important;
    font-size: 1.1rem !important;
    font-weight: 700 !important;
    margin: 15px 0;
  }

  .clock-desc {
    font-size: 0.9rem;
    color: #64748b;
  }

  .helper-text {
    font-size: 0.75rem;
    color: #94a3b8;
    text-align: center;
    width: 100%;
  }

  /* 5. TABS & TABLES (Directly from your Reference) */
  .tab-container {
    display: flex;
    width: 100%;
    background: #ffffff;
    border-radius: 12px 12px 0 0;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border: 1px solid #e2e8f0;
    overflow: hidden;
  }

  .tab-btn {
    flex: 1;
    padding: 14px 10px;
    text-align: center;
    background: #fff;
    border: none;
    cursor: pointer;
    color: #64748b;
    font-weight: 600;
    transition: all 0.3s ease;
    border-right: 1px solid #e2e8f0;
  }

    .tab-btn.active {
      background: #06195e;
      color: white;
      font-weight: 700;
    }

  .data-section-card {
    background: white;
    border-radius: 0 0 12px 12px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
    padding: 25px;
    border: 1px solid #e2e8f0;
    border-top: none;
  }

  .data-table {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0;
  }

    .data-table th {
      background-color: #f1f4f8;
      color: #333;
      text-transform: uppercase;
      font-size: 0.8em;
      font-weight: 700;
      padding: 15px 20px;
      border-bottom: 2px solid #e0e0e0;
      text-align: center; /* Centers the header text */
    }

    .data-table td {
      padding: 15px 20px;
      color: #004085;
      font-weight: 600;
      border-bottom: 1px solid #eeeeee;
      text-align: center; /* Centers the body data */
      vertical-align: middle;
    }

      /* Optional: Keep specific columns left-aligned if preferred */
      /* e.g., If you want the Date column to stay left-aligned for better scanning: */
/*      .data-table th:first-child,
      .data-table td:first-child {
        text-align: left;
      }*/

  /* 6. STATUS BADGES */
  .status-badge {
    padding: 6px 12px;
    border-radius: 50px;
    font-weight: 700;
    font-size: 0.75em;
    text-transform: uppercase;
    display: inline-block;
    text-align: center;
    min-width: 80px;
  }

    .status-badge.captured {
      background-color: #28a745;
      color: white;
    }
    /* Used for IN */
    .status-badge.rejected {
      background-color: #d14249;
      color: white;
    }
    /* Used for OUT */
    .status-badge.activecards {
      background-color: #198754;
      color: white;
    }

    .status-badge.schedule {
      background-color: #ffc107;
      color: white;
    }

  /* 7. SCHEDULE GRID */
  .schedule-flex-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 15px;
  }

  .day-card {
    padding: 20px;
    background: #f8fafc;
    border-radius: 12px;
    border: 1px solid #e2e8f0;
  }

  .day-title {
    color: #06195e;
    font-weight: 800;
    margin-bottom: 15px;
    border-bottom: 1px solid #e2e8f0;
    padding-bottom: 5px;
  }

  .info-item {
    display: flex;
    justify-content: space-between;
    padding: 5px 0;
    border-bottom: 1px solid #edf2f7;
    font-size: 0.9rem;
  }

    .info-item label {
      font-weight: 700;
      color: #64748b;
    }

  .is-rest {
    background: #fff5f5;
    border-color: #fed7d7;
  }

  .rest-indicator {
    color: #e53e3e;
    font-weight: 800;
    text-align: center;
    margin-top: 10px;
  }

  /* 8. BUTTONS */
  .action-btn.view-btn {
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
  }

    .action-btn.view-btn:hover {
      background-color: #0056b3;
    }

  .fade-slide-enter-active, .fade-slide-leave-active {
    transition: all 0.3s ease;
  }

  .fade-slide-enter-from {
    opacity: 0;
    transform: translateY(10px);
  }

  .fade-slide-leave-to {
    opacity: 0;
    transform: translateY(-10px);
  }

  /* Schedule Specific Enhancements */
  .day-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12px;
  }

  .rest-badge {
    background: #fee2e2;
    color: #ef4444;
    font-size: 0.7rem;
    padding: 2px 8px;
    border-radius: 4px;
    font-weight: 800;
  }

  .shift-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .ot-group {
    margin-top: 12px;
    padding-top: 8px;
    border-top: 1px dashed #cbd5e1;
  }

  .ot-item span {
    color: #f59e0b; /* Amber for OT visibility */
    font-weight: 700;
  }

  .rest-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 10px 0;
  }

  .rest-icon {
    font-size: 1.5rem;
    margin-bottom: 5px;
  }

  .rest-text {
    color: #94a3b8;
    font-size: 0.85rem;
    font-weight: 700;
    text-transform: uppercase;
  }

  /* Ensure the grid is responsive */
  .schedule-flex-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
    gap: 15px;
    padding-top: 10px;
  }

  .is-today {
    border: 2px solid #06195e !important;
    background: #f0f4ff !important;
    transform: scale(1.02);
    box-shadow: 0 4px 12px rgba(6, 25, 94, 0.1);
  }

  .today-tag {
    font-size: 0.6rem;
    background: #06195e;
    color: white;
    padding: 2px 6px;
    border-radius: 4px;
    margin-left: 8px;
    vertical-align: middle;
  }

  .no-data-container {
    padding: 40px 20px;
    text-align: center;
    background: #f8fafc;
    border-radius: 8px;
    border: 1px dashed #cbd5e1;
  }

  .empty-state-card {
    color: #64748b;
    font-weight: 500;
  }

  .table-loading-state {
    color: #94a3b8;
  }

  .empty-schedule-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 60px 20px;
    background: #f8fafc;
    border: 2px dashed #e2e8f0;
    border-radius: 12px;
    text-align: center;
  }

  .empty-icon {
    font-size: 3rem;
    margin-bottom: 15px;
    opacity: 0.5;
  }

  .empty-text {
    font-size: 1.1rem;
    font-weight: 700;
    color: #475569;
    margin-bottom: 5px;
  }

  .empty-schedule-state small {
    color: #94a3b8;
  }
</style>
