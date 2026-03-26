<template>
  <div class="app-layout">
    <!--<LeftMenu class="leftMenu" />-->

    <Header title="Employee ID" class="header" />

    <div class="dashboard-content">

      <h2 class="page-title">Employee ID Managements</h2>

      <!--<div class="tab-container action-bar">
        <button v-for="tab in tabs"
                :key="tab"
                @click="activeTab = tab"
                class="tab-btn"
                :class="{ active: activeTab === tab }">
          {{ tab }}
        </button>
      </div>-->

      <div class="data-section-card">
        <div class="tab-content">
          <transition name="fade-slide" mode="out-in">
            <div :key="activeTab" class="form-wrapper">

              <div v-if="activeTab === 'All Employees'">
                <h2 class="sub-title">All Employees</h2>

                <div class="action-bar filters">
                  <div class="search-group">
                    <label>
                      Search:
                      <input type="text"
                             v-model="searchTermAll"
                             placeholder="Search by ID or Name..."
                             class="search-input" />
                    </label>
                  </div>

                  <div class="filter-group">
                    <label>
                      Department:
                      <select v-model="selectedDepartmentAll">
                        <option value="">All Departments</option>
                        <option v-for="dept in departmentListAll" :key="dept" :value="dept">
                          {{ dept }}
                        </option>
                      </select>
                    </label>
                  </div>
                </div>

                <div class="table-container">
                  <table class="data-table">
                    <thead>
                      <tr>
                        <th>Employee ID</th>
                        <th>Full Name</th>
                        <th>Department</th>
                        <th>Designation</th>
                        <th>Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="emp in filteredAllEmployees" :key="emp.employeeID || emp.id">
                        <td>{{ emp.employeeID || emp.id }}</td>
                        <td>{{ emp.fullName || `${emp.firstName} ${emp.lastName || emp.surname}` }}</td>
                        <td>{{ emp.departmentName || emp.department || 'N/A'}}</td>
                        <td>{{ emp.designation || 'N/A' }}</td>
                        <td>
                          <span :class="['status-badge', getStatusDisplay(emp.status).class]">
                            {{ getStatusDisplay(emp.status).text }}
                          </span>
                        </td>
                      </tr>
                      <tr v-if="filteredAllEmployees.length === 0 && !isLoading">
                        <td colspan="5" class="text-center p-4">No employees found.</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <div v-if="activeTab === 'Adjudication'">
                <h2 class="sub-title">Biometric Adjudication</h2>

                <div class="filters">
                  <div class="search-group">
                    <label>Search: <input type="text" v-model="searchTermAdjudication" placeholder="App Code or Name..." class="search-input" /></label>
                  </div>
                  <label>
                    Department:
                    <select v-model="selectedDepartmentAdjudication">
                      <option value="">All</option>
                      <option v-for="dept in departmentListAdjudication" :key="dept" :value="dept">{{ dept }}</option>
                    </select>
                  </label>
                </div>

                <div class="table-container">
                  <table class="data-table">
                    <thead>
                      <tr>
                        <th>App Code</th>
                        <th>Name</th>
                        <th>Department</th>
                        <th>Captured Date</th>
                        <th>Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="emp in filteredAdjudication" :key="emp.personID">
                        <td>{{ emp.applicationCode }}</td>
                        <td>{{ emp.fullName }}</td>
                        <td>{{ emp.departmentName }}</td>
                        <td>{{ formatDate(emp.requestedDate) }}</td>
                        <td>
                          <button @click="openAdjudication(emp)" class="action-btn view-btn">
                            Adjudicate
                          </button>
                        </td>
                      </tr>
                      <tr v-if="filteredAdjudication.length === 0">
                        <td colspan="6" style="text-align: center; padding: 20px;">
                          No biometric hits found for adjudication.
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <div v-if="showAdjudicateModal" class="modal-overlay">
                <div class="modal-content adjudication-modal full-width">
                  <div class="modal-header">
                    <h3>Biometric Adjudication Resolution</h3>
                    <button @click="showAdjudicateModal = false" class="close-btn">&times;</button>
                  </div>

                  <div class="modal-body-scroll">
                    <div class="comparison-grid" v-if="adjCurrent && adjHit">

                      <div class="comparison-card hit">
                        <div class="card-tag tag-hit">Original Template</div>
                        <div class="header-info">
                          <img :src="adjHit.photo ? `data:image/jpeg;base64,${adjHit.photo}` : '/default-user.png'" class="adj-photo">
                          <div class="info-list">
                            <div class="info-item"><label>Name:</label> <span>{{ adjHit.fullName }}</span></div>
                            <div class="info-item"><label>Application Code:</label> <span>{{ adjHit.applicationCode }}</span></div>
                            <div class="info-item"><label>Department:</label> <span>{{ adjHit.departmentName }}</span></div>
                          </div>
                        </div>

                        <div class="biometric-grid">
                          <h5>Left Hand</h5>
                          <div class="finger-row">
                            <div class="f-item" v-if="adjHit.fingers.l1"><label>Thumb</label><img :src="`data:image/png;base64,${adjHit.fingers.l1}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.l2"><label>Index</label><img :src="`data:image/png;base64,${adjHit.fingers.l2}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.l3"><label>Middle</label><img :src="`data:image/png;base64,${adjHit.fingers.l3}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.l4"><label>Ring</label><img :src="`data:image/png;base64,${adjHit.fingers.l4}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.l5"><label>Little</label><img :src="`data:image/png;base64,${adjHit.fingers.l5}`"></div>
                          </div>

                          <h5>Right Hand</h5>
                          <div class="finger-row">
                            <div class="f-item" v-if="adjHit.fingers.r1"><label>Thumb</label><img :src="`data:image/png;base64,${adjHit.fingers.r1}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.r2"><label>Index</label><img :src="`data:image/png;base64,${adjHit.fingers.r2}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.r3"><label>Middle</label><img :src="`data:image/png;base64,${adjHit.fingers.r3}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.r4"><label>Ring</label><img :src="`data:image/png;base64,${adjHit.fingers.r4}`"></div>
                            <div class="f-item" v-if="adjHit.fingers.r5"><label>Little</label><img :src="`data:image/png;base64,${adjHit.fingers.r5}`"></div>
                          </div>
                        </div>
                      </div>

                      <div class="vs-divider"></div>

                      <div class="comparison-card current">
                        <div class="card-tag tag-new">Current Applicant</div>
                        <div class="header-info">
                          <img :src="adjCurrent.photo ? `data:image/jpeg;base64,${adjCurrent.photo}` : '/default-user.png'" class="adj-photo">
                          <div class="info-list">
                            <div class="info-item"><label>Name:</label> <span>{{ adjCurrent.fullName }}</span></div>
                            <div class="info-item"><label>Application Code:</label> <span>{{ adjCurrent.applicationCode }}</span></div>
                            <div class="info-item"><label>Department:</label> <span>{{ adjCurrent.departmentName }}</span></div>
                          </div>
                        </div>

                        <div class="biometric-grid">
                          <h5>Left Hand</h5>
                          <div class="finger-row">
                            <div class="f-item" v-if="adjCurrent.fingers.l1"><label>Thumb</label><img :src="`data:image/png;base64,${adjCurrent.fingers.l1}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.l2"><label>Index</label><img :src="`data:image/png;base64,${adjCurrent.fingers.l2}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.l3"><label>Middle</label><img :src="`data:image/png;base64,${adjCurrent.fingers.l3}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.l4"><label>Ring</label><img :src="`data:image/png;base64,${adjCurrent.fingers.l4}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.l5"><label>Little</label><img :src="`data:image/png;base64,${adjCurrent.fingers.l5}`"></div>
                          </div>

                          <h5>Right Hand</h5>
                          <div class="finger-row">
                            <div class="f-item" v-if="adjCurrent.fingers.r1"><label>Thumb</label><img :src="`data:image/png;base64,${adjCurrent.fingers.r1}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.r2"><label>Index</label><img :src="`data:image/png;base64,${adjCurrent.fingers.r2}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.r3"><label>Middle</label><img :src="`data:image/png;base64,${adjCurrent.fingers.r3}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.r4"><label>Ring</label><img :src="`data:image/png;base64,${adjCurrent.fingers.r4}`"></div>
                            <div class="f-item" v-if="adjCurrent.fingers.r5"><label>Little</label><img :src="`data:image/png;base64,${adjCurrent.fingers.r5}`"></div>
                          </div>
                        </div>
                      </div>


                    </div>

                  </div> <div class="decision-footer">

                    <div class="button-group">
                      <button @click="processAdjudication(true)" class="btn btn-success" :disabled="isLoading">
                        ✅ Approve
                      </button>
                      <button @click="processAdjudication(false)" class="btn btn-danger" :disabled="isLoading">
                        ❌ Reject
                      </button>
                    </div>

                  </div>
                </div>
              </div>


            </div>
          </transition>
        </div>
      </div>

      <DialogBox :show="showDialog"
                 :title="dialogTitle"
                 :message="dialogMessage"
                 @close="showDialog = false" />

      <LoadingDialog :visible="isLoading" />
    </div>
  </div>
</template>

<script setup>
  import api from "@/api";
  import { ref, onMounted, computed, watch, nextTick } from "vue";
  import { useRoute, useRouter } from "vue-router";
  import { useAuthStore } from "@/stores/auth";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";

  const route = useRoute();
  const router = useRouter();

  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);

  //const tabs = ["All Employees", "Schedule Biometrics", "For Approval", "Scheduled", "Captured Biometrics", "Adjudication", "Generate Cards", "Activate Cards"];
  const tabs = ["Adjudication"];
  const activeTab = ref("Adjudication");

  const auth = useAuthStore();
  const personId = computed(() => auth.userId); // logged in user’s ID
  const personEmail = computed(() => auth.email);
  const firstName = computed(() => auth.firstName);
  const lastName = computed(() => auth.lastName);

  // --- Added for Layout (Required for LeftMenu) ---
  const current = ref("Employee ID");
  // Simple onNavigate handler to satisfy LeftMenu component requirement
  const onNavigate = (item) => {
    current.value = item;
  };
  // --- End added setup ---

  const genders = ref([]);
  const civilStatuses = ref([]);
  const employeeForSchedule = ref([]);
  const appointmentDate = ref("");
  const selectedDepartment = ref("");
  const selectedRows = ref([]);
  const selectedRowsCapture = ref([]); // Tracks selected IDs for this tab

  const employeeForPrinting = ref([]);
  const selectedDepartmentPrinting = ref("");
  const selectedRowsPrinting = ref([])

  const employeeForActivation = ref([]);
  const selectedDepartmentActivation = ref("");
  const selectedRowsActivation = ref([]);

  // --- Scheduled State & Logic ---
  const selectedDepartmentScheduled = ref("");
  const employeesScheduled = ref([]);

  const departmentListScheduled = computed(() => {
    const list = employeesScheduled.value || [];
    const depts = list.map(e => e.departmentName);
    return [...new Set(depts)].filter(Boolean);
  });

  // --- Capture State & Logic ---
  const selectedDepartmentCapture = ref("");
  const employeesCaptured = ref([]);

  const departmentListCapture = computed(() => {
    const list = employeesCaptured.value || [];
    const depts = list.map(e => e.departmentName);
    return [...new Set(depts)].filter(Boolean);
  });

  // 2. Add Adjudication States
  const employeeForAdjudication = ref([]);
  const selectedDepartmentAdjudication = ref("");
  const showAdjudicateModal = ref(false);
  const adjRemarks = ref("");
  const adjCurrent = ref(null);
  const adjHit = ref(null);

  // 3. Adjudication Computed Logic
  const departmentListAdjudication = computed(() => {
    const depts = (employeeForAdjudication.value || []).map(e => e.departmentName);
    return [...new Set(depts)].filter(Boolean).sort();
  });

  const refreshTab = async () => {
    switch (activeTab.value) {
      //case "All Employees":
      //  await getAllEmployees();
      //  break;
      //case "Schedule Biometrics":
      //  await getCardsForSchedule(); // Status 0 -> GET /schedule
      //  break;
      //case "For Approval": // <--- ADD THIS CASE
      //  await fetchApprovalList();
      //  break;
      //case "Scheduled":
      //  await getScheduledEmployees(); // Status 1 -> GET /employees/scheduled
      //  break;
      //case "Captured Biometrics":
      //  await getCaptured(); // Status 2 -> GET /employees/captured
      //  break;

      //Adjudication

        case "Adjudication":
          await getPendingAdjudication();
          break;

      //Adjudication


      //case "Generate Cards":
      //  await getEmployeesForPrinting(); // Status 4 -> GET /employees/for-printing
      //  break;
      //case "Activate Cards":
      //  await getCardsForActivation(); // Status 5, 6 -> GET /employees/printed and active-cards
      //  break;
    }

    // Optional: Show a small success dialog or toast
    console.log(`${activeTab.value} data refreshed.`);
  };

  // --- Status Mapping (Matches your Backend Enum) ---
  //const STATUS_MAP = {
  //  0: { text: 'For Schedule', class: 'schedule' },
  //  1: { text: 'Scheduled', class: 'scheduled' },
  //  2: { text: 'Captured', class: 'captured' },
  //  3: { text: 'Adjudication', class: 'adjudication' }, // Based on your Dashboard mapping
  //  4: { text: 'For Printing', class: 'forprinting' },
  //  5: { text: 'For Activation', class: 'foractivation' },
  //  6: { text: 'Active', class: 'activecards'},
  //  7: { text: 'For Approval', class: 'approval' },
  //  99: { text: 'Rejected', class: 'rejected' },
  //};

  const STATUS_MAP = {
    0: { text: 'For Schedule', class: 'schedule' },
    1: { text: 'Scheduled', class: 'scheduled' },
    2: { text: 'Captured', class: 'captured' },
    3: { text: 'Adjudication', class: 'adjudication' }, // Based on your Dashboard mapping
    4: { text: 'For Printing', class: 'forprinting' },
    5: { text: 'For Activation', class: 'foractivation' },
    6: { text: 'Active', class: 'activecards' },
    7: { text: 'For Approval', class: 'approval' },
    99: { text: 'Rejected', class: 'rejected' },
  };

  // Helper function to get display properties
  const getStatusDisplay = (statusValue) => {
    const status = STATUS_MAP[statusValue];
    return status || { text: 'Unknown', class: 'default' };
  };

  // --- All Employees State ---
  const allEmployeesList = ref([]);
  const searchTermAll = ref("");
  const selectedDepartmentAll = ref("");

  // Add these new lines right here:
  const searchTermSchedule = ref("");
  const searchTermApproval = ref("");
  const searchTermScheduled = ref("");
  const searchTermCapture = ref("");
  const searchTermAdjudication = ref("");
  const searchTermPrinting = ref("");
  const searchTermActivation = ref("");

  // --- Computed: Filter logic for All Employees ---
  const filteredAllEmployees = computed(() => {
    const list = allEmployeesList.value || []; // Guard added
    return list.filter(emp => {
      const search = (searchTermAll.value || '').toLowerCase();
      const matchesSearch = !search ||
        (emp.employeeID || '').toString().toLowerCase().includes(search) ||
        (emp.fullName || `${emp.firstName ?? ''} ${emp.lastName ?? emp.surname ?? ''}`).toLowerCase().includes(search);
      const deptValue = emp.departmentName || emp.department || '';
      const matchesDept = !selectedDepartmentAll.value || deptValue === selectedDepartmentAll.value;
      return matchesSearch && matchesDept;
    });
  });

  const filteredEmployees = computed(() => {
    const list = employeeForSchedule.value || []; // Guard added
    return list.filter((e) => {
      const search = (searchTermSchedule.value || '').toLowerCase();
      const matchesSearch = !search ||
        (e.employeeID || '').toString().toLowerCase().includes(search) ||
        (e.fullName || '').toLowerCase().includes(search);
      const matchesDept = !selectedDepartment.value || e.departmentName === selectedDepartment.value;
      return matchesSearch && matchesDept;
    });
  });

  const filteredApproval = computed(() => {
    const list = approvalList.value || []; // Guard added
    return list.filter(emp => {
      const search = (searchTermApproval.value || '').toLowerCase();
      const matchesSearch = !search ||
        (emp.employeeID || '').toString().toLowerCase().includes(search) ||
        (emp.fullName || '').toLowerCase().includes(search);
      const matchesDept = !selectedDepartmentApproval.value || emp.departmentName === selectedDepartmentApproval.value;
      return matchesSearch && matchesDept;
    });
  });

  const filteredScheduled = computed(() => {
    const list = employeesScheduled.value || []; // Guard added
    return list.filter(e => {
      const search = (searchTermScheduled.value || '').toLowerCase();
      const matchesSearch = !search ||
        (e.employeeID || '').toString().toLowerCase().includes(search) ||
        (e.fullName || '').toLowerCase().includes(search);
      const matchesDept = !selectedDepartmentScheduled.value || e.departmentName === selectedDepartmentScheduled.value;
      return matchesSearch && matchesDept;
    });
  });

  const filteredCapture = computed(() => {
    const list = employeesCaptured.value || []; // Already guarded
    return list.filter((e) => {
      const search = (searchTermCapture.value || '').toLowerCase();
      const matchesSearch = !search ||
        (e.employeeID || '').toString().toLowerCase().includes(search) ||
        (e.fullName || '').toLowerCase().includes(search);
      const matchesDept = !selectedDepartmentCapture.value || e.departmentName === selectedDepartmentCapture.value;
      return matchesSearch && matchesDept;
    });
  });

  const filteredAdjudication = computed(() => {
    const list = employeeForAdjudication.value || [];

    return list.filter(e => {
      // 1. Safe search string
      const search = (searchTermAdjudication.value || '').toLowerCase();
      const appCode = (e.applicationCode || '').toString().toLowerCase();
      const name = (e.fullName || '').toLowerCase();

      const matchesSearch = !search || appCode.includes(search) || name.includes(search);

      // 2. Safe department comparison
      // Use || "" to ensure we aren't comparing against undefined
      const selectedDept = selectedDepartmentAdjudication.value || "";
      const matchesDept = selectedDept === "" || e.departmentName === selectedDept;

      return matchesSearch && matchesDept;
    });
  });

  const filteredCards = computed(() => {
    const list = employeeForPrinting.value || []; // Already guarded
    return list.filter((e) => {
      const search = (searchTermPrinting.value || '').toLowerCase();
      const matchesSearch = !search ||
        (e.applicationCode || '').toString().toLowerCase().includes(search) ||
        (e.fullName || '').toLowerCase().includes(search);
      const matchesDept = !selectedDepartmentPrinting.value || e.department === selectedDepartmentPrinting.value;
      return matchesSearch && matchesDept;
    });
  });

  const filteredCardsActivation = computed(() => {
    const list = employeeForActivation.value || [];
    return list.filter(e => {
      const search = searchTermActivation.value.toLowerCase();
      const matchesSearch = !search ||
        e.fullName.toLowerCase().includes(search) ||
        e.employeeID?.toLowerCase().includes(search);

      const matchesDept = !selectedDepartmentActivation.value ||
        e.departmentName === selectedDepartmentActivation.value;

      return matchesSearch && matchesDept;
    });
  });

  // Reactive Variables for Approval Tab
  const approvalList = ref([]);
  const selectedDepartmentApproval = ref("");
  const selectedRowsApproval = ref([]);

  // 1. Fetch the data from your new API
  const fetchApprovalList = async () => {
    isLoading.value = true;
    try {
      const res = await api.get('/hr/schedule/pending-approvals');

      // Guard against undefined with (res.data || [])
      approvalList.value = (res.data || []).map(emp => {
        // Use RequestedDate from API, or fallback to DateSchedule
        const rDate = emp.requestedDate || emp.dateSchedule;

        return {
          ...emp,
          // The display-only reference
          requestedDate: rDate,
          // The editable model for the input box
          dateScheduleLocal: rDate ? rDate.split(' ')[0] : ""
        };
      });
    } catch (err) {
      console.error("Error fetching approvals:", err);
      approvalList.value = [];
    } finally {
      isLoading.value = false;
    }
  };

  // 2. Add Tab-Switching Logic
  // Ensure that when 'For Approval' is clicked, it automatically loads the data
  watch(activeTab, (newTab) => {
    if (newTab === 'For Approval') {
      fetchApprovalList();
    }
  });

  // 4. Generate unique departments for the dropdown
  const departmentListApproval = computed(() => {
    const list = approvalList.value || [];
    const depts = list.map(a => a.departmentName);
    return [...new Set(depts)].filter(d => d);
  });

  // --- Computed: Unique Department list for the dropdown ---
  const departmentListAll = computed(() => {
    const list = allEmployeesList.value || []; // Ensure it's at least an empty array
    const depts = list.map(e => e.departmentName || e.department);
    return [...new Set(depts)].filter(Boolean);
  });

  // Fix for Single Approval
  const approveSingleSchedule = async (app) => {
    isLoading.value = true;

    try {
      // LOGIC: If the input is blank, use the already requested date
      const finalDate = app.dateScheduleLocal || app.requestedDate;

      if (!finalDate) {
        alert("No date found to confirm.");
        return;
      }

      const payload = {
        date: finalDate,
        personIDs: [app.personID]
      };

      // Corrected Path: Ensure this matches your working /api/HR/ prefix
      await api.post('/HR/schedule/update', payload);

      dialogTitle.value = "Schedule Confirmed";
      dialogMessage.value = `The appointment for ${app.fullName} has been finalized.`;
      showDialog.value = true;

      await fetchApprovalList();
    } catch (err) {
      console.error("Confirm Error:", err.response?.data || err);
      alert("Failed to confirm schedule. Error: " + (err.response?.data?.title || "Check console"));
    } finally {
      isLoading.value = false;
    }
  };

  const approveBatchSchedule = async () => {
    if (selectedRowsApproval.value.length === 0) return;

    isLoading.value = true;
    try {
      // For batching, we find the first available date in the selection
      const firstSelected = approvalList.value.find(a => selectedRowsApproval.value.includes(a.personID));
      const finalDate = firstSelected?.dateScheduleLocal || firstSelected?.requestedDate;

      const payload = {
        date: finalDate,
        personIDs: selectedRowsApproval.value
      };

      await api.post('/HR/schedule/update', payload);

      dialogTitle.value = "Batch Success";
      dialogMessage.value = "Selected schedules confirmed.";
      showDialog.value = true;

      selectedRowsApproval.value = [];
      await fetchApprovalList();
    } catch (err) {
      console.error(err);
      alert("Batch confirmation failed.");
    } finally {
      isLoading.value = false;
    }
  };

  // ✅ Select All logic for Approval Tab
  const selectAllApproval = computed({
    get: () =>
      filteredApproval.value.length > 0 &&
      filteredApproval.value.every((app) => selectedRowsApproval.value.includes(app.personID)),
    set: (value) => {
      if (value) {
        // Add all visible filtered employees to selection
        const visibleIds = filteredApproval.value.map((app) => app.personID);
        selectedRowsApproval.value = [...new Set([...selectedRowsApproval.value, ...visibleIds])];
      } else {
        // Remove only the currently visible filtered employees from selection
        const visibleIds = filteredApproval.value.map((app) => app.personID);
        selectedRowsApproval.value = selectedRowsApproval.value.filter(id => !visibleIds.includes(id));
      }
    },
  });

  // --- Scheduled Reschedule State ---
  const rescheduleDate = ref("");
  const selectedRowsScheduled = ref([]);

  // ✅ Select All logic for Scheduled Tab
  const selectAllScheduled = computed({
    get: () =>
      filteredScheduled.value.length > 0 &&
      filteredScheduled.value.every((e) => selectedRowsScheduled.value.includes(e.personID)),
    set: (value) => {
      if (value) {
        // Use personID to match the template :value
        const visibleIds = filteredScheduled.value.map((e) => e.personID);
        selectedRowsScheduled.value = [...new Set([...selectedRowsScheduled.value, ...visibleIds])];
      } else {
        const visibleIds = filteredScheduled.value.map((e) => e.personID);
        selectedRowsScheduled.value = selectedRowsScheduled.value.filter(
          (id) => !visibleIds.includes(id)
        );
      }
    },
  });

  const selectAllCapture = computed({
    get: () =>
      filteredCapture.value.length > 0 &&
      filteredCapture.value.every((e) => selectedRowsCapture.value.includes(e.employeeID)),
    set: (value) => {
      if (value) {
        // Add all visible filtered employees to selection
        const visibleIds = filteredCapture.value.map((e) => e.employeeID);
        selectedRowsCapture.value = [...new Set([...selectedRowsCapture.value, ...visibleIds])];
      } else {
        // Remove visible filtered employees from selection
        const visibleIds = filteredCapture.value.map((e) => e.employeeID);
        selectedRowsCapture.value = selectedRowsCapture.value.filter(id => !visibleIds.includes(id));
      }
    },
  });

  // ✅ Reschedule API Call
  const rescheduleSelected = async () => {
    if (!rescheduleDate.value) {
      alert("Please select a new appointment date.");
      return;
    }

    // Ensure we are sending the IDs collected in selectedRowsScheduled
    const cleanIDs = selectedRowsScheduled.value.map(id => Number(id));

    isLoading.value = true;
    try {
      const payload = {
        date: rescheduleDate.value,
        personIDs: cleanIDs
      };

      // Corrected path with /employee prefix
      const res = await api.post(`/hr/schedule/reschedule`, payload);

      dialogTitle.value = "Success";
      dialogMessage.value = res.data.message;
      showDialog.value = true;

      selectedRowsScheduled.value = [];
      rescheduleDate.value = "";
      await getScheduledEmployees(); // Refresh the list

    } catch (error) {
      console.error("Reschedule Error:", error);
      alert(error.response?.data?.message || "Failed to reschedule.");
    } finally {
      isLoading.value = false;
    }
  };

  async function approveSelectedCaptured() {
    if (selectedRowsCapture.value.length === 0) return;

    const confirmed = confirm(`Are you sure you want to approve ${selectedRowsCapture.value.length} records?`);
    if (!confirmed) return;

    isLoading.value = true;
    try {
      // Mapping each selected ID to an API call
      const promises = selectedRowsCapture.value.map(id =>
        api.post('/hr/employees/validate', { employeeId: id.toString() })
      );

      await Promise.all(promises);

      dialogTitle.value = "Success";
      dialogMessage.value = "Selected employees have been validated and moved to Generate Cards.";
      showDialog.value = true;

      // Reset selection and refresh list
      selectedRowsCapture.value = [];
      await getCaptured();

    } catch (error) {
      console.error("Validation failed:", error);
      dialogTitle.value = "Partial Success/Error";
      dialogMessage.value = "Some records could not be validated. Please check the console.";
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  }

  // ---------- WATCHERS --------------

  // watcher for "Schedule Biometrics" tab
  watch(selectedDepartment, async (newVal) => {
    if (!newVal) {
      selectedRows.value = [];
      return;
    }
    await nextTick();
    selectedRows.value = filteredEmployees.value.map((e) => e.personID);
  });

  // FIX: watcher for "Scheduled" tab
  watch(selectedDepartmentScheduled, async (newVal) => {
    if (!newVal) {
      selectedRowsScheduled.value = [];
      return;
    }
    await nextTick();
    // Map to employeeID instead of personID
    selectedRowsScheduled.value = filteredScheduled.value.map((e) => e.employeeID);
  });

  watch(selectedDepartmentPrinting, async (newVal) => {
    if (!newVal) {
      selectedRowsPrinting.value = [];
      return;
    }

    // Wait until table updates before selecting
    await nextTick();

    // Automatically select employees from the chosen department
    selectedRowsPrinting.value = filteredCards.value.map((e) => e.applicationCode);
  });  // ✅ Track selected rows

  // --- API Fetch Function ---
  async function getAllEmployees() {
    try {
      isLoading.value = true;
      const res = await api.get('/hr/employees');
      allEmployeesList.value = res.data;
    } catch (err) {
      console.error("Failed to load all employees:", err);
    } finally {
      isLoading.value = false;
    }
  }

  async function getCardsForSchedule() {
    try {
      isLoading.value = true;
      const res = await api.get(`/hr/schedule`);

      // Map the incoming data to ensure it has a fullName property
      employeeForSchedule.value = (res.data || []).map(emp => ({
        ...emp,
        fullName: emp.fullName || `${emp.firstName ?? ''} ${emp.lastName ?? ''}`.trim() || 'No Name'
      }));
    } catch (err) {
      console.error("Failed to load data:", err);
      employeeForSchedule.value = []; // Prevent crash
    } finally {
      isLoading.value = false;
    }
  }

  async function getScheduledEmployees() {
    try {
      isLoading.value = true;
      const res = await api.get(`/hr/employees/scheduled`);

      // 🔍 Log the data to see the exact field name for the date
      console.log("Scheduled Employees Raw Data:", res.data);

      employeesScheduled.value = res.data;

      if (res.data.length > 0) {
        console.log("Sample Employee Object Keys:", Object.keys(res.data[0]));
      }

    } catch (err) {
      console.error("Failed to load scheduled list:", err);
    } finally {
      isLoading.value = false;
    }
  }

  async function getCaptured() {
    try {
      isLoading.value = true;
      // We fetch employees with Status 1 (Scheduled) to perform the Capture
      const res = await api.get(`/hr/employees/captured`);
      employeesCaptured.value = res.data;
    } catch (err) {
      console.error("Failed to load capture list:", err);
    } finally {
      isLoading.value = false;
    }
  }

  async function getPendingAdjudication() {
    try {
      isLoading.value = true;
      const res = await api.get(`/hr/adjudication`);
      console.log("1. API Raw Response:", res.data); // Should show an array

      employeeForAdjudication.value = res.data || [];
      console.log("2. Assigned to Ref:", employeeForAdjudication.value.length, "items");
    } catch (err) {
      console.error("Adjudication fetch failed:", err);
    } finally {
      isLoading.value = false;
    }
  }

  // Helper to handle base64 images from backend
  const renderImage = (base64) => {
    if (!base64) return '/placeholder-image.png'; // Fallback
    return base64.startsWith('data:image') ? base64 : `data:image/png;base64,${base64}`;
  };

  async function getEmployeesForPrinting() {
    try {
      isLoading.value = true;
      // 🟢 UPDATED: Matches the [HttpGet("employees/for-printing")] in your C# Controller
      const res = await api.get(`/hr/employees/for-printing`);
      employeeForPrinting.value = res.data;
    } catch (err) {
      console.error("Failed to load printing data:", err);
    } finally {
      isLoading.value = false;
    }
  }

  async function getCardsForActivation() {
    try {
      isLoading.value = true;

      // Fetch both status 5 (Printed/For Activation) and 6 (Active)
      const [res5, res6] = await Promise.all([
        api.get('/hr/employees/printed'),
        api.get('/hr/employees/active-cards')
      ]);

      // Combine the arrays
      employeeForActivation.value = [...(res5.data || []), ...(res6.data || [])];

    } catch (err) {
      console.error("Failed to load cards for activation:", err);
    } finally {
      isLoading.value = false;
    }
  }

  // 5. Modal Logic
  const openAdjudication = async (emp) => {

    console.log(emp);
    adjCurrent.value = null;
    adjHit.value = null;
    isLoading.value = true;

    try {
      const response = await api.get(`/hr/adjudication/compare/${emp.personID}/${emp.afisPersonHit}`);

      // Log the entire payload to see the structure
      console.log("Full API Response:", response.data);

      const applicant = response.data.applicant || response.data.Applicant;
      const match = response.data.match || response.data.Match;

      // Specific log for fingerprints
      if (applicant && applicant.fingers) {
        console.log("--- Applicant Biometrics ---");
        console.log("Thumb (L1) Type:", typeof applicant.fingers.L1);
        console.log("Thumb (L1) Value Sample:", applicant.fingers.L1 ? applicant.fingers.L1.substring(0, 30) + "..." : "NULL");
      }

      if (match && match.fingers) {
        console.log("--- Match Biometrics ---");
        console.log("Thumb (L1) Type:", typeof match.fingers.L1);
        console.log("Thumb (L1) Value Sample:", match.fingers.L1 ? match.fingers.L1.substring(0, 30) + "..." : "NULL");
      }

      adjCurrent.value = applicant;
      adjHit.value = match;
      showAdjudicateModal.value = true;

    } catch (error) {
      console.error("Adjudication Load Error:", error);
    } finally {
      isLoading.value = false;
    }
  };

  const processAdjudication = async (isUnique) => {
    // 1. Validation for Duplicate (Hit) status
    //if (!isUnique && (!adjRemarks.value || adjRemarks.value.trim() === "")) {
    //  alert("Please enter remarks explaining why this is a duplicate.");
    //  return;
    //}

    isLoading.value = true;
    try {
      // 2. Map frontend state to Backend DTO (AdjudicationDecisionDto)
      const payload = {
        applicationCode: adjCurrent.value.applicationCode, // Using ApplicationCode as required by API
        isConfirmedHit: !isUnique,                        // If Unique=true, then Hit=false
        remarks: adjRemarks.value || "No remarks provided"
      };

      // 3. Call the correct endpoint: /employee/adjudicate/decision
      await api.post('/hr/adjudicate/decision', payload);

      // 4. Cleanup and UI Refresh
      showAdjudicateModal.value = false;
      adjRemarks.value = ""; // Clear remarks for next use

      // Refresh the list to remove the processed record
      await getPendingAdjudication();

      dialogTitle.value = "Success";
      dialogMessage.value = isUnique ? "Applicant cleared successfully." : "Duplicate hit confirmed. Application rejected.";
      showDialog.value = true;

    } catch (err) {
      console.error("Adjudication Save Error:", err);
      alert("Failed to save adjudication decision: " + (err.response?.data?.message || "Server Error"));
    } finally {
      isLoading.value = false;
    }
  };

  // 6. Update onMounted
  onMounted(() => {
    // Check if a tab was passed in the URL (e.g., /employee-id?tab=Adjudication)
    if (route.query.tab && tabs.includes(route.query.tab)) {
      activeTab.value = route.query.tab;
    }
  });

  // Optional: Watch for changes if the user navigates
  // while already on the EmployeeID page
  watch(() => route.query.tab, (newTab) => {
    if (newTab && tabs.includes(newTab)) {
      activeTab.value = newTab;
    }
  });

  /**
   For Printing
  */

  async function downloadCSV() {
    const selectedIDs = selectedRowsPrinting.value;

    if (selectedIDs.length === 0) {
      dialogTitle.value = "No Selection";
      dialogMessage.value = "Please select at least one record to download.";
      showDialog.value = true;
      return;
    }

    try {
      isLoading.value = true;

      // 1. Download CSV file
      const response = await api.post("/hr/print-cards/csv", selectedIDs, {
        responseType: "blob", // Important for binary data
      });

      // CHECK: Ensure the response is actually a CSV and not an error hidden in a blob
      if (response.data.type === "application/json") {
        // If the backend sent an error message as JSON, but Axios wrapped it in a Blob
        throw new Error("Backend returned a JSON error instead of a file.");
      }

      // 2. Build and trigger download
      const blob = new Blob([response.data], { type: "text/csv" });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `IDPrintFile_${new Date().getTime()}.csv`);
      document.body.appendChild(link);
      link.click();

      // Cleanup
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      // 3. Refresh list and Clear selection
      // Wrap these in a secondary try/catch if you don't want a refresh failure
      // to trigger the "Download Failed" error message.
      try {
        await getCardsForPrinting();
        selectedRowsPrinting.value = [];
      } catch (refreshErr) {
        console.warn("File downloaded, but list failed to refresh:", refreshErr);
      }

      // 4. Show success message
      dialogTitle.value = "Success";
      dialogMessage.value = "CSV downloaded and list updated successfully!";
      showDialog.value = true;

    } catch (err) {
      console.error("CSV download process error:", err);

      // Only show the error dialog if the actual download failed
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to generate or download CSV file. Please try again.";
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  }


  // 🔹 Department list (unique)
  const departmentListPrinting = computed(() => {
    const depts = employeeForPrinting.value.map((e) => e.department);
    return [...new Set(depts)].filter(Boolean);
  });

  // 🔹 When department is selected, auto-select matching employees
  const onDepartmentPrintingChange = () => {
    if (!selectedDepartmentPrinting.value) {
      selectedRows.value = [];
      return;
    }

    // Auto-select all employees in the chosen department
    selectedRows.value = filteredCards.value.map(e => e.personID);
  };

  // ✅ Select All checkbox
  const selectAllPrinting = computed({
    get: () =>
      filteredCards.value.length > 0 &&
      filteredCards.value.every((e) => selectedRowsPrinting.value.includes(e.applicationCode)),
    set: (value) => {
      if (value) {
        selectedRowsPrinting.value = [
          ...new Set([
            ...selectedRowsPrinting.value,
            ...filteredCards.value.map((e) => e.applicationCode),
          ]),
        ];
      } else {
        selectedRowsPrinting.value = selectedRowsPrinting.value.filter(
          (id) => !filteredCards.value.some((e) => e.applicationCode === id)
        );
      }
    },
  });


  // ✅ Toggle all checkboxes
  const toggleSelectAllPrinting = () => {
    if (selectAll.value) {
      // Deselect all visible
      selectedRows.value = [];
    } else {
      // Select all visible
      selectedRows.value = filteredCards.value.map(e => e.applicationCode);
    }
  };

  /**
  End
  */

  // 🔹 Department list (unique)
  const departmentList = computed(() => {
    const depts = employeeForSchedule.value.map((e) => e.departmentName);
    return [...new Set(depts)].filter(Boolean);
  });

  // 🔹 When department is selected, auto-select matching employees
  const onDepartmentChange = () => {
    if (!selectedDepartment.value) {
      selectedRows.value = [];
      return;
    }

    // Auto-select all employees in the chosen department
    selectedRows.value = filteredEmployees.value.map(e => e.personID);
  };

  watch(selectedDepartment, async (newVal) => {
    if (!newVal) {
      selectedRows.value = [];
      return;
    }

    // Wait until table updates before selecting
    await nextTick();

    // Automatically select employees from the chosen department
    selectedRows.value = filteredEmployees.value.map((e) => e.personID);
  });  // ✅ Track selected rows

  // ✅ Select All checkbox
  const selectAll = computed({
    get: () =>
      filteredEmployees.value.length > 0 &&
      filteredEmployees.value.every((e) => selectedRows.value.includes(e.personID)),
    set: (value) => {
      if (value) {
        selectedRows.value = [
          ...new Set([
            ...selectedRows.value,
            ...filteredEmployees.value.map((e) => e.personID),
          ]),
        ];
      } else {
        selectedRows.value = selectedRows.value.filter(
          (id) => !filteredEmployees.value.some((e) => e.personID === id)
        );
      }
    },
  });


  // ✅ Toggle all checkboxes
  const toggleSelectAll = () => {
    if (selectAll.value) {
      // Deselect all visible
      selectedRows.value = [];
    } else {
      // Select all visible
      selectedRows.value = filteredEmployees.value.map(e => e.personID);
    }
  };

  // ✅ Example action for selected rows
  const handleBulkAction = () => {
    alert(
      `Scheduling ${selectedRows.value.length} selected applications: ${selectedRows.value.join(
        ", "
      )}`
    );
  };

  // ✅ Date formatting
  const formatDate = (dateStr) => {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    if (isNaN(date)) return '';
    return date.toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  // 🆕 ADDED: Logic for Activate Cards
  // 🔹 Department list (unique) for activation
  const departmentListActivation = computed(() => {
    // 🟢 UPDATED: Use departmentName from DTO
    const depts = employeeForActivation.value.map((e) => e.departmentName);
    return [...new Set(depts)].filter(Boolean);
  });

  // 🔹 Watcher for department change to auto-select
  watch(selectedDepartmentActivation, async (newVal) => {
    if (!newVal) {
      selectedRowsActivation.value = [];
      return;
    }
    await nextTick();
    // Automatically select employees from the chosen department
    // 🟢 UPDATED: Use employeeID as the identifier
    selectedRowsActivation.value = filteredCardsActivation.value.map((e) => e.employeeID);
  });

  // ✅ Select All checkbox for activation
  const selectAllActivation = computed({
    get: () =>
      filteredCardsActivation.value.length > 0 &&
      // 🟢 UPDATED: Use employeeID
      filteredCardsActivation.value.every((e) => selectedRowsActivation.value.includes(e.employeeID)),
    set: (value) => {
      if (value) {
        selectedRowsActivation.value = [
          ...new Set([
            ...selectedRowsActivation.value,
            // 🟢 UPDATED: Use employeeID
            ...filteredCardsActivation.value.map((e) => e.employeeID),
          ]),
        ];
      } else {
        selectedRowsActivation.value = selectedRowsActivation.value.filter(
          // 🟢 UPDATED: Use employeeID
          (id) => !filteredCardsActivation.value.some((e) => e.employeeID === id)
        );
      }
    },
  });

  // ------------------ Load Data ------------------
  // Load dropdown options
  //async function loadLookups() {
  //  try {
  //    isLoading.value = true; // show hourglass
  //    const g = await api.get("/employee/gender");
  //    genders.value = g.data;

  //    const cs = await api.get("/employee/civilstatus");
  //    civilStatuses.value = cs.data;
  //  } catch (err) {
  //    console.error("AxiosError:", err);

  //    if (err.response) {
  //      console.error("🔴 Backend responded with error:");
  //      console.error("Status:", err.response.status);
  //      console.error("Data:", err.response.data);
  //    } else if (err.request) {
  //      console.error("🔴 No response received from server");
  //      console.error("Request:", err.request);
  //    } else {
  //      console.error("🔴 Axios setup error:", err.message);
  //    }
  //  } finally {
  //    isLoading.value = false;  // hide hourglass
  //  }
  //}


  // ✅ Function to call backend API
  const scheduleSelected = async () => {
    if (!appointmentDate.value) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Please select an appointment date.";
      showDialog.value = true;
      return;
    }
    if (!selectedRows.value.length) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Please select at least one record.";
      showDialog.value = true;
      return;
    }

    isLoading.value = true;
    try {
      const payload = {
        date: appointmentDate.value,
        personIDs: selectedRows.value,
      };

      await api.post(`/hr/schedule/update`, payload);

      employeeForSchedule.value = employeeForSchedule.value.filter(
        (a) => !selectedRows.value.includes(a.personID)
      );

      dialogTitle.value = "Success";
      dialogMessage.value = "Appointment schedule saved successfully!";
      showDialog.value = true;

      selectedRows.value = [];
      appointmentDate.value = "";
    } catch (error) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Error Saving appointment schedule";
      showDialog.value = true;

    } finally {
      isLoading.value = false;
    }
  };

  // 🆕 ADDED: The requested function for activating cards
  /**
   * Function to call the API to activate selected cards.
   * API: POST /employee/employees/active-cards
   */
  async function activateSelectedCards() {
    const selectedIDs = selectedRowsActivation.value;

    if (selectedIDs.length === 0) {
      dialogTitle.value = "No Selection";
      dialogMessage.value = "Please select at least one card to activate.";
      showDialog.value = true;
      return;
    }

    isLoading.value = true;
    try {
      // 🔹 Step 1: Call the API to activate cards
      // The payload (selectedIDs) now contains EmployeeIDs
      const response = await api.post("/hr/employees/active-cards", selectedIDs);

      // 🔹 Step 2: Refresh the list after activation
      await getCardsForActivation();

      // 🔹 Step 3: Clear selected rows
      selectedRowsActivation.value = [];

      // 🔹 Step 4: Show success message
      dialogTitle.value = "Success";
      dialogMessage.value = `${selectedIDs.length} card${selectedIDs.length === 1 ? '' : 's'} activated successfully!`;
      showDialog.value = true;

    } catch (error) {
      console.error("Card activation failed:", error);
      dialogTitle.value = "Error";
      dialogMessage.value = "Error activating cards. Please check the console for details.";
      showDialog.value = true;

    } finally {
      isLoading.value = false;
    }
  }

  // --- Place this updated onMounted at the bottom of your <script setup> ---
  onMounted(async () => {
    // 1. Sync activeTab with URL query
    if (route.query.tab && tabs.includes(route.query.tab)) {
      activeTab.value = route.query.tab;
    }

    // 2. Fetch data for the SPECIFIC tab currently active on reload
    await refreshTab();

    // 3. Load other necessities
  //  loadLookups();
  });

  // --- Add the Watcher to handle tab switching automatically ---
  watch(activeTab, async (newTab) => {
    // Clear selections when moving between tabs
    selectedRows.value = [];
    selectedRowsScheduled.value = [];
    selectedRowsCapture.value = []; // Added this line
    selectedRowsPrinting.value = [];
    selectedRowsActivation.value = [];

    await refreshTab();
  });</script>

<style scoped>
  /* ************************************************************************** */
  /* 1. DASHBOARD GRID LAYOUT STYLES (Copied from ManageHRApplicationsPage.vue) */
  /* ************************************************************************** */
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

  .header {
    grid-column: 2;
    grid-row: 1;
    z-index: 90;
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

  /* ************************************************************************** */
  /* 2. PAGE CONTENT & ACTION BAR STYLES (Copied/Adapted from ManageHRApplicationsPage.vue) */
  /* ************************************************************************** */

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

  .action-bar {
    /* Base style for the row where tabs are placed */
    display: flex;
    justify-content: flex-start;
    align-items: center;
    margin-bottom: 25px;
    padding: 0;
  }

  /* Tabs Styling - Adopted to match dashboard aesthetic */
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

    .tab-btn:hover:not(.active) {
      background: #e0e0e0;
      color: #06195e;
    }

    .tab-btn.active {
      background: #06195e;
      color: white;
      font-weight: 700;
    }

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
      flex-grow: 0;
      font-size: 0.95em;
    }

    .filters input,
    .filters select,
    .action-btn {
      height: 42px; /* Fixed height makes them align perfectly */
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 6px;
      font-size: 0.95rem;
      box-sizing: border-box; /* Critical for keeping padding inside the height */
      outline: none;
      transition: border-color 0.2s;
    }

      .action-btn:hover:not(:disabled) {
        background-color: #2980b9;
      }

      .action-btn:disabled {
        background-color: #bdc3c7;
        cursor: not-allowed;
      }

  /* Fix for the "Bigger Date Container" */
  input[type="date"] {
    /* Some browsers add extra padding to date pickers; this normalizes it */
    padding: 6px 10px;
    cursor: pointer;
  }

  .filters button:disabled {
    background-color: #6c757d;
    cursor: not-allowed;
  }

  /* Focus states */
  .filters input:focus,
  .filters select:focus {
    border-color: #3498db;
  }

  .data-section-card {
    background: white;
    border-radius: 0 12px 12px 12px; /* Top-left is sharp to align with tabs */
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
    padding: 25px;
    overflow: hidden;
    border: none;
  }

  /* ************************************************************************** */
  /* 3. DATA TABLE STYLES (Copied from ManageHRApplicationsPage.vue) */
  /* ************************************************************************** */

  .table-container {
    overflow-x: auto;
  }

  .data-table {
    width: 100%;
    border-collapse: separate;
    border-spacing: 0;
  }

    .data-table th, .data-table td {
      border: none;
      padding: 15px 20px;
      text-align: left;
    }

    .data-table th {
      background-color: #f1f4f8;
      color: #333;
      text-transform: uppercase;
      font-size: 0.8em;
      font-weight: 700;
      letter-spacing: 0.5px;
      border-bottom: 2px solid #e0e0e0;
    }

    .data-table tbody tr {
      border-bottom: 1px solid #eeeeee;
      transition: background-color 0.15s;
    }

      .data-table tbody tr:last-child {
        border-bottom: none;
      }

    .data-table tr:hover {
      background-color: #f7faff;
    }

    .data-table tbody tr td:nth-child(1),
    .data-table tbody tr td:nth-child(2),
    .data-table tbody tr td:nth-child(3),
    .data-table tbody tr td:nth-child(4),
    .data-table tbody tr td:nth-child(5),
    .data-table tbody tr td:nth-child(6) {
      color: #004085;
      font-weight: 600;
    }

    .data-table input[type="checkbox"] {
      width: auto;
      margin: 0;
      accent-color: #007bff; /* Use primary color for checkboxes */
    }

  .action-cell {
    white-space: nowrap;
  }

  /* Table Loading/Empty State */
  .table-loading-state {
    padding: 20px;
    text-align: center;
    color: #6c757d;
  }

  .table-photo {
    width: 50px;
    height: 75px;
    border-radius: 3px;
    border: 1px solid #ddd;
    object-fit: cover;
  }

  .table-date-input {
    border: 1px solid #ddd;
    padding: 4px 8px;
    border-radius: 4px;
    font-family: inherit;
    font-size: 0.9rem;
    color: #333;
  }

  .status-badge.pending {
    background-color: #fff3cd;
    color: #856404;
    border: 1px solid #ffeeba;
  }


  /* --- BUTTON STYLES (Copied from ManageHRApplicationsPage.vue) --- */

  .action-btn.view-btn {
    /* Used for "Save Schedule" */
    background-color: #007bff;
    color: white;
    border-color: #007bff;
  }

    .action-btn.view-btn:hover {
      background-color: #0056b3;
      border-color: #0056b3;
    }

  /* Custom style for the Download button */
  .action-btn.download-btn {
    background-color: #28a745; /* Green */
    border-color: #28a745;
  }

    .action-btn.download-btn:hover {
      background-color: #1e7e34;
      border-color: #1c7430;
    }


  /* --- STATUS BADGES (Copied from ManageHRApplicationsPage.vue) --- */
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

    /* Colors matching DashboardHR card left borders: */
    /* Status 0: "For Schedule" -> schedule (Yellow) */
    .status-badge.schedule {
      background-color: #ffc107;
      color: white;
    }

    /* Status 7: "For Approval" -> approval (Teal/Cyan) */
    .status-badge.approval {
      background-color: #17a2b8; /* Info Cyan */
      color: white;
    }

    /* Status 1: "Scheduled" -> scheduled (Blue) */
    .status-badge.scheduled {
      background-color: #007bff;
      color: white;
    }
    /* Captured (2), For Printing (3), Active Cards (4) kept for reference */
    .status-badge.captured {
      background-color: #28a745;
      color: white;
    }

    /* Status 3: "Pending Adjudication" -> adjudication (Orange) */
    .status-badge.adjudication {
      background-color: #fd7e14; /* Safety Orange */
      color: white;
    }

    /* Status 99: "Rejected" -> rejected (Dark Grey/Black) */
    .status-badge.rejected {
      background-color: #d14249; /* Red */
      color: white;
    }

    .status-badge.forprinting {
      background-color: #6f42c1;
      color: white;
    }

    /* Status 5: "For Activation" -> foractivation (Orange-Yellow or Bright Green) */
    .status-badge.foractivation {
      background-color: #20c997; /* Teal-Green: Indicates ready but not yet final */
      color: white;
      /*      border: 1px solid #1ba37a;*/
    }

    /* Status 6: "Active" -> activecards (Deep Success Green) */
    .status-badge.activecards {
      background-color: #198754;
      color: white;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1); /* Slight shadow to show it's "Final" */
    }

    .status-badge.default {
      background-color: #6c757d;
      color: white;
    }

  /* Tab transition styles (optional but good practice) */
  .fade-slide-enter-active,
  .fade-slide-leave-active {
    transition: opacity 0.3s ease, transform 0.3s ease;
  }

  .fade-slide-enter-from,
  .fade-slide-leave-to {
    opacity: 0;
    transform: translateY(10px);
  }

  .date-approval-cell {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .requested-label {
    font-size: 0.75rem;
    color: #718096;
    font-weight: 600;
    display: block;
  }

  .table-date-input {
    padding: 4px;
    border: 1px solid #cbd5e0;
    border-radius: 4px;
    font-size: 0.9rem;
  }

  /* ************************************************************************** */
  /* 4. RESPONSIVENESS (Copied from ManageHRApplicationsPage.vue) */
  /* ************************************************************************** */
  @media (max-width: 1200px) {
    .app-layout {
      grid-template-columns: 20vw 1fr;
    }
  }

  @media (max-width: 992px) {
    .app-layout {
      grid-template-columns: 80px 1fr;
    }

    .tab-container {
      flex-wrap: wrap;
      border: none;
      box-shadow: none;
      background: none;
    }

    .tab-btn {
      flex: 1 1 auto;
      border: 1px solid #e0e0e0;
      border-radius: 6px;
      margin: 5px;
    }
  }

  @media (max-width: 768px) {
    .app-layout {
      grid-template-columns: 1fr;
      grid-template-rows: auto auto 1fr;
    }

    .leftMenu, .header, .dashboard-content {
      grid-column: 1;
    }

    .leftMenu {
      grid-row: 2;
      width: 100%;
      min-height: auto;
    }

    .header {
      grid-row: 1;
    }

    .dashboard-content {
      grid-row: 3;
      padding: 20px 15px;
    }

    .filters {
      flex-direction: column;
      align-items: stretch;
    }

      .filters input[type="date"],
      .filters select,
      .filters .action-btn {
        min-width: 100%;
        max-width: 100%;
      }
  }

  /* Search input specifically */
  .search-input {
    width: 100%;
    min-width: 200px;
  }

  .comparison-grid {
    display: grid;
    grid-template-columns: 1fr 60px 1fr;
    gap: 20px;
    padding: 10px;
  }

  .card-tag {
    font-size: 0.75rem;
    font-weight: bold;
    margin-bottom: 10px;
    color: #fff;
    padding: 2px 8px;
    border-radius: 4px;
    display: inline-block;
  }

  .tag-new {
    background: #007bff;
  }

  .tag-hit {
    background: #dc3545;
  }

  .adj-photo {
    width: 100%;
    height: 180px;
    object-fit: cover;
    background: #eee;
    border-radius: 4px;
    margin-bottom: 10px;
  }

  .vs-divider {
    font-weight: bold;
    color: #999;
    text-align: center;
  }

  .btn {
    padding: 10px 20px;
    border-radius: 6px;
    cursor: pointer;
    border: none;
    font-weight: bold;
  }

  .btn-success {
    background: #28a745;
    color: white;
  }

  .btn-danger {
    background: #dc3545;
    color: white;
  }

  /* Status Badge Base Styles */
  .status-badge {
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 0.85rem;
    font-weight: 600;
    display: inline-block;
    text-transform: uppercase;
  }

    /* Specific Style for Pending AFIS */
    .status-badge.processing {
      background-color: #fff4e5; /* Light Orange/Cream */
      color: #b76e00; /* Darker Orange/Brown */
      border: 1px solid #ffe2b3;
      display: flex;
      align-items: center;
      gap: 6px;
      width: fit-content;
    }

      /* The Animated Pulse Dot */
      .status-badge.processing::before {
        content: "";
        width: 8px;
        height: 8px;
        background-color: #ffa500;
        border-radius: 50%;
        display: inline-block;
        animation: pulse-dot 1.5s infinite;
        margin-right: 6px;
      }

  @keyframes pulse-dot {
    0% {
      transform: scale(0.95);
      box-shadow: 0 0 0 0 rgba(255, 165, 0, 0.7);
    }

    70% {
      transform: scale(1);
      box-shadow: 0 0 0 6px rgba(255, 165, 0, 0);
    }

    100% {
      transform: scale(0.95);
      box-shadow: 0 0 0 0 rgba(255, 165, 0, 0);
    }
  }

  .btn-primary, .submit-btn {
    background: #06195e;
    color: white;
    padding: 12px 25px;
    border-radius: 8px;
    font-weight: 700;
    border: none;
    cursor: pointer;
    transition: all 0.2s;
  }

    .btn-primary:hover {
      background: #04103d;
      transform: translateY(-1px);
    }

  /* --- COMPARISON AREA --- */
  .modal-body-scroll {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
    background-color: #f1f4f8; /* Light gray background to make white cards pop */
  }

  .comparison-grid {
    display: grid;
    grid-template-columns: 1fr 60px 1fr;
    gap: 15px;
    align-items: start;
  }

  /* --- BIOMETRICS SECTION --- */
  .biometric-grid {
    margin-top: 20px;
    padding: 15px;
    background: #f8f9ff;
    border-radius: 8px;
    border: 1px solid #eef0f7;
  }

    .biometric-grid h5 {
      color: #004085;
      margin: 10px 0;
      text-transform: uppercase;
      font-size: 0.85rem;
      letter-spacing: 1px;
      border-left: 3px solid #004085;
      padding-left: 8px;
    }

  .finger-row {
    display: flex;
    justify-content: flex-start;
    gap: 12px;
    flex-wrap: wrap;
  }

  .f-item label {
    font-size: 10px;
    font-weight: 800;
    color: #666;
    margin-bottom: 4px;
    display: block;
  }

  .f-item img {
    width: 90px;
    height: 110px;
    background: white;
    border: 1px solid #ccc;
    border-radius: 4px;
    object-fit: contain;
    box-shadow: 0 2px 5px rgba(0,0,0,0.05);
  }

  .header-info {
    display: flex;
    gap: 15px;
    align-items: center;
  }

  /* Modal Layout */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.75);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 2000;
    backdrop-filter: blur(4px); /* Modern touch */
  }

  .adjudication-modal.full-width {
    background: #fdfdfd;
    width: 95vw;
    max-width: 1400px;
    height: 90vh;
    display: flex;
    flex-direction: column;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0,0,0,0.3);
    overflow: hidden;
  }

  /* --- HEADERS --- */
  .modal-header {
    background: #004085; /* Matches your page theme */
    color: white;
    padding: 15px 25px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

    .modal-header h3 {
      margin: 0;
      font-weight: 700;
      letter-spacing: 0.5px;
    }

  .close-btn {
    background: none;
    border: none;
    color: white;
    font-size: 2rem;
    cursor: pointer;
    line-height: 1;
  }

  /* --- DIVIDER --- */
  .vs-divider {
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 900;
    font-size: 1.5rem;
    color: #004085;
    opacity: 0.5;
  }

  /* --- CARDS --- */
  .comparison-card {
    background: white;
    border-radius: 10px;
    padding: 20px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    border: 1px solid #e0e0e0;
  }

    .comparison-card.current {
      border-top: 5px solid #dc3545;
    }

    .comparison-card.hit {
      border-top: 5px solid #007bff;
    }

  /* --- INFO LIST STYLES (MATCHING MAIN STYLE) --- */
  .info-list {
    display: flex;
    flex-direction: column;
    gap: 8px;
    flex: 1;
  }

  .info-item {
    display: flex;
    gap: 10px;
    font-size: 0.95em;
    border-bottom: 1px solid #f0f0f0;
    padding: 6px 0;
  }

    .info-item label {
      font-weight: 700;
      color: #06195e;
      min-width: 90px;
    }

    .info-item span {
      color: #333;
      font-weight: 600;
    }

  /* Photo and Fingerprint sizing */
  .adj-photo {
    width: 150px;
    height: 150px;
    object-fit: cover;
    border-radius: 8px;
    border: 1px solid #ccc;
  }

  .finger-row {
    display: flex;
    justify-content: space-around;
    gap: 10px;
    margin-top: 10px;
  }

  .f-item img {
    width: 80px;
    height: 100px;
    background: #eee;
    border: 1px solid #ddd;
    object-fit: contain;
  }

  /* --- FOOTER --- */
  .decision-footer {
    padding: 20px 40px;
    background: white;
    border-top: 2px solid #e0e0e0;
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .button-group {
    display: flex;
    gap: 20px;
  }

  .btn {
    padding: 12px 30px;
    border-radius: 8px;
    font-size: 1rem;
    font-weight: 700;
    cursor: pointer;
    transition: transform 0.1s, background 0.2s;
  }

    .btn:active {
      transform: scale(0.98);
    }

  .btn-success {
    background: #28a745;
    color: white;
    border: none;
  }

  .btn-danger {
    background: #dc3545;
    color: white;
    border: none;
  }

  .decision-footer textarea {
    width: 100%;
    height: 80px;
    margin-bottom: 15px;
    padding: 10px;
    border-radius: 4px;
  }
</style>
