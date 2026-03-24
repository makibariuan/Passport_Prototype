<template>
  <div class="app-layout">
    <LeftMenu class="leftMenu" />

    <div class="dashboard-content">
      <div class="hr-controls-header">
        <h1 class="page-title">Attendance Management</h1>
      </div>

      <div class="tab-container">
        <button v-for="tab in hrTabs" :key="tab"
                :class="['tab-btn', { active: activeTab === tab }]"
                @click="activeTab = tab">
          {{ tab }}
        </button>
      </div>

      <div class="data-section-card">
        <transition name="fade-slide" mode="out-in">
          <div :key="activeTab">

            <div v-if="activeTab === 'Daily Monitor'">
              <div class="filters">
                <label>
                  Reference Date
                  <input type="date" v-model="selectedDate" @change="fetchDailyAttendance" />
                </label>
                <label>
                  Department
                  <select v-model="selectedDept">
                    <option value="">All Departments</option>
                    <option v-for="dept in departmentList" :key="dept" :value="dept">
                      {{ dept }}
                    </option>
                  </select>
                </label>
                <button class="action-btn view-btn" @click="fetchDailyAttendance">Refresh</button>
              </div>

              <div class="table-container">
                <table class="data-table">
                  <thead>
                    <tr>
                      <th>Employee</th>
                      <th>AM In/Out</th>
                      <th>PM In/Out</th>
                      <th>Hrs (OT)</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="log in dailyLogs" :key="log.EmployeeID">
                      <td>
                        <div class="date-approval-cell">
                          <span>{{ log.FullName }}</span>
                          <small class="requested-label">{{ log.EmployeeID }}</small>
                        </div>
                      </td>
                      <td>{{ log.MorningIn }} | {{ log.MorningOut }}</td>
                      <td>{{ log.AfternoonIn }} | {{ log.AfternoonOut }}</td>
                      <td>{{ log.TotalHours }}h <span style="color: #28a745;">({{ log.OTHours }}h)</span></td>
                      <td>
                        <span :class="['status-badge', getStatusClass(log.Status)]">
                          {{ log.Status }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div v-if="activeTab === 'Schedules'">
              <div v-if="selectedEmployeeIds.length > 0" class="batch-action-bar">
                <div class="batch-info">
                  <span class="count-badge">{{ selectedEmployeeIds.length }}</span>
                  Employees Selected
                </div>

                <div class="batch-controls">
                  <label>Apply Shift:</label>
                  <select v-model="selectedDefaultShift" @change="syncShiftTimes">
                    <option value="">-- Choose Default Shift --</option>
                    <option v-for="(times, name) in defaultShifts" :key="name" :value="name">
                      {{ name }} Shift
                    </option>
                  </select>

                  <div v-if="selectedDefaultShift" class="shift-preview-inline">
                    <TimeSelectGroup v-model="activeShiftTimes.morningStart" />
                    <span class="range-dash">-</span>
                    <TimeSelectGroup v-model="activeShiftTimes.afternoonEnd" />
                    <button class="btn-success btn-small" @click="applyBatchSchedule">
                      Confirm Batch Schedule
                    </button>
                  </div>
                </div>
                <button class="btn-ghost" @click="selectedEmployeeIds = []">Cancel</button>
              </div>

              <div class="table-container">
                <table class="data-table">
                  <thead>
                    <tr>
                      <th>
                        <input type="checkbox" @change="toggleSelectAll" :checked="isAllSelected" />
                      </th>
                      <th>Employee ID</th>
                      <th>Full Name</th>
                      <th>Department</th>
                      <th>Current Schedule</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="emp in filteredEmployees" :key="emp.employeeID">
                      <td>
                        <input type="checkbox" :value="emp.employeeID" v-model="selectedEmployeeIds" />
                      </td>
                      <td><small class="requested-label">{{ emp.employeeID }}</small></td>
                      <td>{{ emp.fullName }}</td>
                      <td>{{ emp.department }}</td>
                      <td>
                        <span v-if="emp.effectiveDate && !emp.effectiveDate.startsWith('0001')" class="status-badge activecards">
                          Starts: {{ formatDate(emp.effectiveDate) }}
                        </span>
                        <span v-else class="status-badge schedule">No Schedule Set</span>
                      </td>
                      <td>
                        <button class="action-btn view-btn" @click.stop="openScheduleModal(emp)">
                          View Weekly
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <div v-if="activeTab === 'Appointments'">
              <div class="table-loading-state">Appointment tracking module coming soon.</div>
            </div>

            <div v-if="activeTab === 'Master Logs'">
              <div class="filters">
                <div class="field-group">
                  <label>Upload .DAT File</label>
                  <input type="file" @change="Masterlogs.fileChange" class="date-input" style="padding: 5px;" />
                </div>
                <button class="btn-primary" @click="Masterlogs.uploadFile" :disabled="isLoading">
                  {{ isLoading ? 'Uploading...' : 'Upload Logs' }}
                </button>
                <span v-if="Masterlogs.uploadMessage" class="status-badge activecards" style="margin-left: 10px;">
                  {{ Masterlogs.uploadMessage }}
                </span>
              </div>

              <div class="table-container">
                <table class="data-table">
                  <thead>
                    <tr>
                      <th>Employee ID</th>
                      <th>Work Date</th>
                      <th>Time In</th>
                      <th>Break</th>
                      <th>Time Out</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(record, index) in Masterlogs.records" :key="index">
                      <td>
                        <small class="requested-label">{{ record.employeeID }}</small>
                      </td>
                      <td>{{ record.workDate }}</td>
                      <td>
                        <span :class="record.timeIn ? 'text-success' : 'text-muted'">
                          {{ record.timeIn || '--:--:--' }}
                        </span>
                      </td>
                      <td class="text-muted">
                        {{ record.break || '--:--:--' }}
                      </td>
                      <td>
                        <span :class="record.timeOut ? 'text-primary' : 'text-muted'">
                          {{ record.timeOut || '--:--:--' }}
                        </span>
                      </td>
                    </tr>

                    <tr v-if="!isLoading && Masterlogs.records.length === 0">
                      <td colspan="5" class="table-loading-state">
                        No attendance records found for this selection.
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div class="pagination-footer">
                <button class="btn-ghost" :disabled="Masterlogs.page === 1" @click="Masterlogs.fetchData(Masterlogs.page - 1)">
                  Previous
                </button>
                <span class="page-indicator">Page {{ Masterlogs.page }} of {{ Masterlogs.totalPages }}</span>
                <button class="btn-ghost" :disabled="Masterlogs.page === Masterlogs.totalPages" @click="Masterlogs.fetchData(Masterlogs.page + 1)">
                  Next
                </button>
              </div>
            </div>

          </div>
        </transition>
      </div>
    </div>
  </div>

  <div v-if="showScheduleModal" class="modal-overlay">
    <div class="modal-content" style="width: 800px; max-width: 95vw;">
      <div class="modal-header">
        <h3>Weekly Schedule: {{ selectedEmployee?.fullName }}</h3>
        <div class="header-btns">
          <button @click="isEditing = !isEditing" class="btn-edit-toggle">
            {{ isEditing ? 'Cancel Edit' : 'Edit Schedule' }}
          </button>
        </div>
      </div>

      <div class="modal-body" style="padding: 20px;">
        <div v-if="isEditing" class="effective-date-container">
          <div class="field-group">
            <label>Effective Date:</label>
            <input type="date" v-model="editEffectiveDate" class="date-input" />
          </div>
          <button type="button" @click="copyMondayToWeek" class="btn-copy-monday">
            📋 Copy Monday to Tue-Fri
          </button>
        </div>

        <table class="weekly-table">
          <thead>
            <tr>
              <th>Day</th>
              <th>Rest?</th>
              <th>Morning (In-Out)</th>
              <th>Afternoon (In-Out)</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(day, index) in selectedEmployee?.weeklySchedule" :key="day.dayName">
              <td class="day-name">{{ day.dayName }}</td>
              <td>
                <input type="checkbox" v-model="day.isRestDay" :disabled="!isEditing" />
              </td>
              <td>
                <div class="time-picker-row" v-if="!day.isRestDay">
                  <TimeSelectGroup v-model="day.morningStart" :disabled="!isEditing" />
                  <span class="range-dash">-</span>
                  <TimeSelectGroup v-model="day.morningEnd" :disabled="!isEditing" />
                </div>
                <span v-else class="text-muted">---</span>
              </td>
              <td>
                <div class="time-picker-row" v-if="!day.isRestDay">
                  <TimeSelectGroup v-model="day.afternoonStart" :disabled="!isEditing" />
                  <span class="range-dash">-</span>
                  <TimeSelectGroup v-model="day.afternoonEnd" :disabled="!isEditing" />
                </div>
                <span v-else class="text-muted">---</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="decision-footer">
        <button v-if="isEditing" @click="saveSchedule" class="btn-success" :disabled="isLoading">
          {{ isLoading ? 'Saving...' : '💾 Save Changes' }}
        </button>
        <button @click="showScheduleModal = false" class="btn-secondary">Close</button>
      </div>
    </div>
  </div>

  <DialogBox :show="showGenericAlert"
             :title="dialogTitle"
             :message="dialogMessage"
             @close="showGenericAlert = false" />

  <LoadingDialog :visible="isLoading" />
</template>

<script setup>
  import { ref, onMounted, watch, computed, reactive } from 'vue';
  import api from "@/api";
  import LeftMenu from '@/components/LeftMenu.vue';
  import DialogBox from '@/components/DialogBox.vue';
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import TimeSelectGroup from '@/components/HR/TimeSelectGroup.vue';

  // State
  const showScheduleModal = ref(false);
  const showGenericAlert = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const activeTab = ref('Daily Monitor');
  const hrTabs = ['Daily Monitor', 'Schedules', 'Appointments', 'Master Logs'];
  const allEmployeesList = ref([]);
  const dailyLogs = ref([]);
  const isLoading = ref(false);
  const selectedDate = ref(null);
  const selectedDept = ref("");
  const searchTermSchedules = ref("");
  const selectedDeptSchedules = ref("");
  const isEditing = ref(false);
  const editEffectiveDate = ref(new Date().toISOString().split('T')[0]);
  const selectedEmployee = ref(null);

  // Batch State
  const selectedEmployeeIds = ref([]);
  const selectedDefaultShift = ref("");
  const defaultShifts = ref({
    "Day": { morningStart: "08:00", morningEnd: "12:00", afternoonStart: "13:00", afternoonEnd: "17:00" },
    "Afternoon": { morningStart: "13:00", morningEnd: "17:00", afternoonStart: "18:00", afternoonEnd: "22:00" },
    "Night": { morningStart: "22:00", morningEnd: "02:00", afternoonStart: "03:00", afternoonEnd: "07:00" }
  });
  const activeShiftTimes = ref({
    morningStart: "08:00", morningEnd: "12:00", afternoonStart: "13:00", afternoonEnd: "17:00"
  });

  // Computed
  const isAllSelected = computed(() =>
    filteredEmployees.value.length > 0 && selectedEmployeeIds.value.length === filteredEmployees.value.length
  );

  const filteredEmployees = computed(() => {
    const list = allEmployeesList.value || [];
    return list.filter(emp => {
      const name = (emp.fullName || '').toLowerCase();
      const id = (emp.employeeID || '').toLowerCase();
      const search = searchTermSchedules.value.toLowerCase();
      const matchesSearch = !search || name.includes(search) || id.includes(search);
      const matchesDept = !selectedDeptSchedules.value || emp.department === selectedDeptSchedules.value;
      return matchesSearch && matchesDept;
    });
  });

  const departmentList = computed(() => {
    const depts = allEmployeesList.value.map(e => e.department);
    return [...new Set(depts)].filter(Boolean).sort();
  });

  // Methods
  const toggleSelectAll = (e) => {
    selectedEmployeeIds.value = e.target.checked ? filteredEmployees.value.map(emp => emp.employeeID) : [];
  };

  const syncShiftTimes = () => {
    if (selectedDefaultShift.value) {
      activeShiftTimes.value = { ...defaultShifts.value[selectedDefaultShift.value] };
    }
  };

  async function applyBatchSchedule() {
    if (!confirm(`Apply ${selectedDefaultShift.value} shift to ${selectedEmployeeIds.value.length} employees?`)) return;
    isLoading.value = true;

    // Use standard DayOfWeek: 0=Sun, 1=Mon...
    const weeklyPattern = [1, 2, 3, 4, 5].map(dayNum => ({
      day: dayNum,
      isRestDay: false,
      morningStart: `${activeShiftTimes.value.morningStart}:00`,
      morningEnd: `${activeShiftTimes.value.morningEnd}:00`,
      afternoonStart: `${activeShiftTimes.value.afternoonStart}:00`,
      afternoonEnd: `${activeShiftTimes.value.afternoonEnd}:00`
    }));
    [0, 6].forEach(dayNum => weeklyPattern.push({ day: dayNum, isRestDay: true }));

    try {
      const requests = selectedEmployeeIds.value.map(id => {
        const payload = {
          employeeID: id,
          effectiveDate: new Date().toISOString().split('T')[0],
          weeklyPattern: weeklyPattern
        };
        return api.put(`/hr/work-schedule/${id}`, payload);
      });
      await Promise.all(requests);
      dialogTitle.value = "Batch Success";
      dialogMessage.value = `Successfully updated ${selectedEmployeeIds.value.length} employees.`;
      showGenericAlert.value = true;
      selectedEmployeeIds.value = [];
      await fetchEmployeesForScheduling();
    } catch (err) {
      dialogTitle.value = "Batch Error";
      dialogMessage.value = "Update failed.";
      showGenericAlert.value = true;
    } finally {
      isLoading.value = false;
    }
  }

  const openScheduleModal = (emp) => {
    isEditing.value = false;
    let clonedEmp = JSON.parse(JSON.stringify(emp));
    clonedEmp.weeklySchedule.forEach((day) => {
      if ((day.dayName === 'Saturday' || day.dayName === 'Sunday') && (!day.morningStart || day.morningStart === 'OFF')) {
        day.isRestDay = true;
      }
      if (!day.morningStart || day.morningStart === 'OFF') day.morningStart = "08:00";
      if (!day.morningEnd || day.morningEnd === 'OFF') day.morningEnd = "12:00";
      if (!day.afternoonStart || day.afternoonStart === 'OFF') day.afternoonStart = "13:00";
      if (!day.afternoonEnd || day.afternoonEnd === 'OFF') day.afternoonEnd = "17:00";
    });
    selectedEmployee.value = clonedEmp;
    editEffectiveDate.value = new Date().toISOString().split('T')[0];
    showScheduleModal.value = true;
  };

  const copyMondayToWeek = () => {
    const mon = selectedEmployee.value.weeklySchedule.find(d => d.dayName === 'Monday');
    if (!mon) return;
    const targets = ['Tuesday', 'Wednesday', 'Thursday', 'Friday'];
    selectedEmployee.value.weeklySchedule.forEach(day => {
      if (targets.includes(day.dayName)) {
        day.isRestDay = mon.isRestDay;
        day.morningStart = mon.morningStart;
        day.morningEnd = mon.morningEnd;
        day.afternoonStart = mon.afternoonStart;
        day.afternoonEnd = mon.afternoonEnd;
      }
    });
  };

  async function saveSchedule() {
    isLoading.value = true;
    const dayMap = { "Sunday": 0, "Monday": 1, "Tuesday": 2, "Wednesday": 3, "Thursday": 4, "Friday": 5, "Saturday": 6 };
    const payload = {
      employeeID: selectedEmployee.value.employeeID.trim(),
      effectiveDate: editEffectiveDate.value,
      weeklyPattern: selectedEmployee.value.weeklySchedule.map(day => ({
        day: dayMap[day.dayName],
        isRestDay: day.isRestDay,
        morningStart: day.isRestDay ? null : `${day.morningStart}:00`,
        morningEnd: day.isRestDay ? null : `${day.morningEnd}:00`,
        afternoonStart: day.isRestDay ? null : `${day.afternoonStart}:00`,
        afternoonEnd: day.isRestDay ? null : `${day.afternoonEnd}:00`
      }))
    };
    try {
      await api.put(`/hr/work-schedule/${payload.employeeID}`, payload);
      showScheduleModal.value = false;
      dialogTitle.value = "Success";
      dialogMessage.value = "Schedule updated.";
      showGenericAlert.value = true;
      await fetchEmployeesForScheduling();
    } catch (err) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to save.";
      showGenericAlert.value = true;
    } finally {
      isLoading.value = false;
    }
  }

  const Masterlogs = reactive({
    file: null,
    uploadMessage: '',
    records: [],
    page: 1,
    pageSize: 10,
    totalPages: 1,

    fileChange(event) {
      this.file = event.target.files[0];
      console.log("File selected:", this.file?.name);
    },

    async uploadFile() {
      if (!this.file) {
        alert("Select a file first!");
        return;
      }

      isLoading.value = true;
      const formData = new FormData();
      formData.append("file", this.file);

      console.log("Uploading file to /hr/upload-and-process...");

      try {
        const res = await api.post("/hr/upload-and-process", formData, {
          headers: { "Content-Type": "multipart/form-data" }
        });

        console.log("Upload Response:", res.data);
        this.uploadMessage = res.data;

        // Refresh the report
        await this.fetchData(1);
      } catch (err) {
        console.error("Upload Error Details:", err.response || err);
        this.uploadMessage = err.response?.data || "Upload failed";
      } finally {
        isLoading.value = false;
      }
    },

    async fetchData(requestedPage = 1) {
      isLoading.value = true;
      this.page = requestedPage;

      const requestParams = {
        page: this.page,
        pageSize: this.pageSize
      };

      // Only send date if the user actually picked one
      if (selectedDate.value && selectedDate.value.trim() !== "") {
        requestParams.date = selectedDate.value;
      }

      // Only send employeeId if there's text in the search box
      if (searchTermSchedules.value && searchTermSchedules.value.trim() !== "") {
        requestParams.employeeId = searchTermSchedules.value.trim();
      }

      try {
        const res = await api.get("/HR/hr/daily-attendance", { params: requestParams });

        if (res.data) {
          this.records = res.data.data || [];
          this.totalPages = res.data.totalPages || 1;
          this.totalCount = res.data.totalCount || 0;
        }
        console.log(`Fetched ${this.totalCount} total records from server.`);
      } catch (err) {
        console.error("Fetch Error:", err);
        this.records = [];
      } finally {
        isLoading.value = false;
      }
    }
  });

  async function fetchEmployeesForScheduling() {
    isLoading.value = true;
    try {
      const res = await api.get('/hr/all-work-schedules');
      allEmployeesList.value = res.data;
    } finally { isLoading.value = false; }
  }

  const getStatusClass = (s) => s === 'Present' ? 'activecards' : 'rejected';
  const formatDate = (d) => d ? new Date(d).toLocaleDateString() : '';
  async function fetchDailyAttendance() { /* Implementation */ }

  const refreshTab = () => {
    if (activeTab.value === 'Schedules') fetchEmployeesForScheduling();
    if (activeTab.value === 'Master Logs') Masterlogs.fetchData();
  };

  watch(activeTab, refreshTab);
  onMounted(refreshTab);
</script>

<style scoped>
  /* 1. DASHBOARD GRID LAYOUT */
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
    width: 100%;
    height: 100%;
    padding: 30px;
    box-sizing: border-box;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
  }

  /* 2. PAGE TITLES & ACTIONS */
  .hr-controls-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
  }

  .page-title {
    font-size: 1.8rem;
    color: #06195e;
    font-weight: 800;
    margin-bottom: 25px;
  }

  .sub-title {
    font-size: 1.5rem;
    color: #06195e;
    padding-bottom: 10px;
    border-bottom: 2px solid #f0f4f8;
    font-weight: 700;
    margin-bottom: 20px;
  }

  /* 3. TAB STYLING (EmployeeID Boxed Style) */
  .tab-container {
    display: flex;
    width: 100%;
    margin-bottom: 25px;
    background: #ffffff;
    border-radius: 12px;
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
    white-space: nowrap;
  }

    .tab-btn:last-child {
      border-right: none;
    }

    .tab-btn.active {
      background: #06195e;
      color: white;
      font-weight: 700;
    }

  /* 4. FILTERS */
  .filters {
    display: flex;
    gap: 15px;
    margin-bottom: 25px;
    padding: 20px;
    background: #f8fafc;
    border-radius: 10px;
    border: 1px solid #e2e8f0;
    align-items: flex-end;
  }

    .filters label {
      font-weight: 600;
      color: #333;
      display: flex;
      flex-direction: column;
      gap: 5px;
      font-size: 0.95em;
    }

    .filters input, .filters select {
      height: 42px;
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 6px;
      font-size: 0.95rem;
    }

  /* 5. DATA CARD & TABLE */
  .data-section-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
    padding: 25px;
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
    }

    .data-table td {
      padding: 15px 20px;
      border-bottom: 1px solid #eeeeee;
      color: #004085;
      font-weight: 600;
    }

    .data-table tr:hover {
      background-color: #f7faff;
    }

  /* 6. STATUS BADGES */
  .status-badge {
    padding: 6px 12px;
    border-radius: 50px;
    font-weight: 700;
    font-size: 0.75em;
    text-transform: uppercase;
    display: inline-block;
    min-width: 90px;
    text-align: center;
  }

    .status-badge.activecards {
      background-color: #198754;
      color: white;
    }

    .status-badge.rejected {
      background-color: #d14249;
      color: white;
    }

    .status-badge.approval {
      background-color: #17a2b8;
      color: white;
    }

    .status-badge.schedule {
      background-color: #ffc107;
      color: white;
    }

  /* 7. BUTTONS */
  .btn-primary {
    background: #06195e;
    color: white;
    padding: 12px 25px;
    border-radius: 8px;
    font-weight: 700;
    border: none;
    cursor: pointer;
  }

  .action-btn.view-btn {
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 6px;
    padding: 0 20px;
    height: 42px;
    font-weight: 600;
    cursor: pointer;
  }

  .table-loading-state {
    padding: 40px;
    text-align: center;
    color: #6c757d;
    font-style: italic;
  }

  /* TRANSITIONS */
  .fade-slide-enter-active, .fade-slide-leave-active {
    transition: all 0.3s ease;
  }

  .fade-slide-enter-from, .fade-slide-leave-to {
    opacity: 0;
    transform: translateY(10px);
  }

  .schedule-modal-content {
    padding: 10px;
  }

  .weekly-table {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0;
    margin-top: 10px;
    border: 1px solid #f1f5f9;
    border-radius: 8px;
  }

    .weekly-table th {
      background-color: #f8fafc;
      color: #64748b;
      text-transform: uppercase;
      font-size: 0.75rem;
      font-weight: 700;
      padding: 12px 16px;
      border-bottom: 2px solid #e2e8f0;
    }

    .weekly-table td {
      padding: 12px 16px;
      border-bottom: 1px solid #f1f5f9;
      color: #06195e; /* Match system text color */
      font-weight: 600;
    }

  /* Style the Inputs to match filters */
  .time-inputs input, .date-input {
    border: 1px solid #cbd5e1;
    padding: 6px 10px;
    border-radius: 6px;
    color: #334155;
    outline: none;
  }

    .time-inputs input:focus {
      border-color: #06195e;
      box-shadow: 0 0 0 2px rgba(6, 25, 94, 0.1);
    }

  .day-name {
    font-weight: 700;
    color: #06195e;
  }

  .mini-badge {
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.7rem;
    font-weight: 800;
  }

    .mini-badge.off {
      background: #fee2e2;
      color: #dc2626;
    }

    .mini-badge.work {
      background: #dcfce7;
      color: #16a34a;
    }

  .requested-label {
    font-family: 'Courier New', Courier, monospace;
    font-weight: bold;
    color: #334155;
  }

  .header-btns {
    display: flex;
    gap: 10px;
    align-items: center;
  }

  .btn-edit-toggle {
    background: #64748b;
    color: white;
    border: none;
    padding: 8px 15px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    font-size: 0.85rem;
  }

  .effective-date-container {
    margin-bottom: 20px;
    padding: 15px;
    background: #f1f5f9;
    border-radius: 8px;
    display: flex;
    align-items: center;
    gap: 15px;
  }

  .btn-success {
    background-color: #16a34a;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    font-weight: 700;
    cursor: pointer;
  }

  .btn-secondary {
    background-color: #94a3b8;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    font-weight: 700;
    cursor: pointer;
  }

  .text-muted {
    color: #94a3b8;
    font-style: italic;
  }

  .btn-copy-monday {
    background: #e2e8f0;
    color: #475569;
    border: 1px solid #cbd5e1;
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 0.8rem;
    font-weight: 600;
    cursor: pointer;
    margin-left: auto; /* Pushes it to the right */
  }

    .btn-copy-monday:hover {
      background: #cbd5e1;
    }

  .field-group {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  /* Darken the background to make the modal pop */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: rgba(15, 23, 42, 0.6); /* Slate-900 with transparency */
    backdrop-filter: blur(4px); /* Modern touch */
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
  }

  /* Modal Container */
  .modal-content {
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
    border: 1px solid #e2e8f0;
    display: flex;
    flex-direction: column;
    overflow: hidden; /* Keeps header/footer rounded */
  }

  /* Modal Header - Consistent with .page-title color */
  .modal-header {
    background: #ffffff;
    padding: 20px 24px;
    border-bottom: 1px solid #f1f5f9;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

    .modal-header h3 {
      margin: 0;
      color: #06195e;
      font-size: 1.25rem;
      font-weight: 800;
    }

  /* Modal Body */
  .modal-body {
    padding: 24px;
    background: #ffffff;
    max-height: 70vh;
    overflow-y: auto;
  }

  /* Footer - Aligning buttons to the right */
  .decision-footer {
    background: #f8fafc; /* Subtle contrast from body */
    padding: 16px 24px;
    border-top: 1px solid #e2e8f0;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }

  /* Refined Close Button (The 'X') */
  .close-btn {
    background: none;
    border: none;
    font-size: 1.5rem;
    color: #94a3b8;
    cursor: pointer;
    transition: color 0.2s;
    padding: 0;
    line-height: 1;
  }

    .close-btn:hover {
      color: #64748b;
    }

  .time-picker-row {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 5px;
  }

  .range-dash {
    font-weight: bold;
    color: #94a3b8;
  }

  .weekly-table td {
    padding: 10px 5px;
    text-align: center;
  }

  /* Adjust the column widths to prevent jumping when editing */
  .weekly-table th:nth-child(3),
  .weekly-table th:nth-child(4) {
    width: 280px;
  }

  .batch-action-bar {
    position: sticky;
    top: 0;
    z-index: 50;
    background: #06195e;
    color: white;
    padding: 15px 25px;
    margin-bottom: 20px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    gap: 20px;
    box-shadow: 0 4px 15px rgba(6, 25, 94, 0.3);
  }

  .count-badge {
    background: #facc15;
    color: #06195e;
    padding: 2px 10px;
    border-radius: 4px;
    font-weight: 800;
    margin-right: 8px;
  }

  .batch-controls {
    display: flex;
    align-items: center;
    gap: 15px;
    flex-grow: 1;
  }

  .shift-preview-inline {
    display: flex;
    align-items: center;
    gap: 10px;
    background: rgba(255, 255, 255, 0.1);
    padding: 5px 15px;
    border-radius: 8px;
  }

  .btn-small {
    padding: 6px 12px;
    font-size: 0.8rem;
  }

  .btn-ghost {
    background: transparent;
    color: #cbd5e1;
    border: 1px solid #475569;
    cursor: pointer;
    padding: 8px 15px;
    border-radius: 6px;
  }

    .btn-ghost:hover {
      color: white;
      border-color: white;
    }

  /* Pagination Styling */
  .pagination-footer {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 20px;
    margin-top: 25px;
    padding-top: 20px;
    border-top: 1px solid #f1f5f9;
  }

  .page-indicator {
    color: #64748b;
    font-size: 0.9rem;
  }

  .btn-ghost {
    background: transparent;
    border: 1px solid #cbd5e1;
    color: #475569;
    padding: 8px 16px;
    border-radius: 6px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
  }

    .btn-ghost:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-ghost:hover:not(:disabled) {
      background: #f1f5f9;
      border-color: #06195e;
      color: #06195e;
    }
</style>
