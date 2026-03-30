<template>
  <div class="app-layout">

    <Header title="Employee ID" class="header" />

    <div class="dashboard-content">

      <h2 class="page-title">Employee ID Managements</h2>

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

                <div class="content-card">
                  <div class="header">
                    <div>
                      <h2 class="sub-title">Biometric Adjudication</h2>
                      <p class="sub">Review and resolve biometric profile conflicts.</p>
                    </div>
                    <div class="search-group">
                      <input type="text" v-model="searchTermAdjudication" placeholder="Search App Code or Name..." class="search" />
                    </div>
                  </div>

                  <div class="table-wrapper">
                    <table class="data-table">
                    </table>
                  </div>
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

                <div v-if="showAdjudicateModal" class="modal-overlay">
                  <div class="modal-content adjudication-modal assessment-style">
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
                          Approve
                        </button>
                        <button @click="processAdjudication(false)" class="btn btn-danger" :disabled="isLoading">
                          Reject
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
  /* ==========================================================================
   1. CORE LAYOUT
   ========================================================================== */
  .app-layout {
    display: flex;
    justify-content: center;
    width: 100%;
    min-height: 100vh;
    padding: 30px 40px;
    box-sizing: border-box;
    background: #eef2f7;
  }

  .dashboard-content {
    width: 100%;
    max-width: 1600px;
    display: flex;
    flex-direction: column;
  }

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
  }

  .page-title {
    font-size: 1.8rem;
    color: #06195e;
    font-weight: 800;
    margin-bottom: 25px;
  }

  /* ==========================================================================
   2. COMPONENTS (TABS, FILTERS, CARDS)
   ========================================================================== */
  .tab-container {
    display: flex;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border: 1px solid #e2e8f0;
    overflow: hidden;
    margin-bottom: 25px;
  }

  .tab-btn {
    flex: 1;
    padding: 14px;
    border: none;
    background: #fff;
    color: #64748b;
    font-weight: 600;
    cursor: pointer;
    transition: 0.3s;
    border-right: 1px solid #e2e8f0;
  }

    .tab-btn.active {
      background: #06195e;
      color: white;
    }

  .filters {
    display: flex;
    gap: 15px;
    padding: 20px;
    background: #f8fafc;
    border-radius: 10px;
    border: 1px solid #e2e8f0;
    margin-bottom: 25px;
    align-items: flex-end;
  }

    .filters label {
      font-weight: 600;
      font-size: 0.9rem;
      display: flex;
      flex-direction: column;
      gap: 5px;
    }

    .search-input, .filters select {
      height: 42px;
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 6px;
      outline: none;
    }

  .data-section-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
    padding: 25px;
  }

  /* ==========================================================================
   3. DATA TABLES
   ========================================================================== */
  .table-container {
    overflow-x: auto;
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
      font-size: 0.75rem;
      font-weight: 700;
      padding: 15px 20px;
      border-bottom: 2px solid #e2e8f0;
      text-align: left;
    }

    .data-table td {
      padding: 15px 20px;
      border-bottom: 1px solid #eee;
      color: #004085;
      font-weight: 600;
    }

    .data-table tr:hover {
      background-color: #f7faff;
    }

  /* ==========================================================================
   4. ADJUDICATION MODAL & COMPARISON
   ========================================================================== */
  .modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.75);
    backdrop-filter: blur(4px);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 2000;
  }

  .modal-content.assessment-style {
    background: #f1f4f8;
    width: 95vw;
    max-width: 1300px;
    max-height: 90vh;
    border-radius: 20px;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    box-shadow: 0 25px 50px rgba(0,0,0,0.25);
  }

  .modal-header {
    background: transparent; /* Removed background */
    color: #06195e; /* Dark text for contrast against light modal */
    padding: 20px 25px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 1px solid #e2e8f0;
  }

  .close-btn {
    background: #f1f5f9; /* Subtle background for the button */
    border: none;
    color: #64748b;
    font-size: 1.25rem;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s ease;
    line-height: 1;
    padding-bottom: 2px; /* Visual nudge for the '×' character */
  }

    .close-btn:hover {
      background: #fee2e2;
      color: #ef4444;
      transform: rotate(90deg);
    }

  .modal-body-scroll {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
  }

  .comparison-grid {
    display: grid;
    grid-template-columns: 1fr 50px 1fr;
    gap: 20px;
    align-items: start;
  }

  .comparison-card {
    background: white;
    border-radius: 12px;
    padding: 20px;
    box-shadow: 0 4px 6px rgba(0,0,0,0.05);
  }

    .comparison-card.hit {
      border-top: 6px solid #3b82f6;
    }

    /* Current Applicant -> Red */
    .comparison-card.current {
      border-top: 6px solid #ef4444;
    }

  .header-info {
    display: flex;
    flex-direction: row; /* Information now in a row */
    align-items: center; /* Vertically centers the photo and info */
    gap: 20px;
    margin-bottom: 20px;
    padding: 10px;
  }

  .adj-photo {
    width: 100px; /* Slightly smaller for row layout */
    height: 100px;
    flex-shrink: 0; /* Prevents photo from squishing */
    object-fit: cover;
    border-radius: 10px;
    border: 1px solid #e2e8f0;
    background: #f8fafc;
  }

  .info-list {
    width: 100%;
    text-align: left; /* Keep text aligned left for readability */
  }

  .biometric-grid {
    margin-top: 20px;
    padding: 15px;
    background: #f8f9ff;
    border-radius: 8px;
  }

  .finger-row {
    display: flex;
    gap: 10px;
    flex-wrap: wrap;
  }

  .f-item img {
    width: 70px;
    height: 90px;
    background: white;
    border: 1px solid #ddd;
    object-fit: contain;
  }

  /* ==========================================================================
   5. BUTTONS & BADGES
   ========================================================================== */
  .decision-footer {
    padding: 20px;
    background: #fff;
    border-top: 1px solid #e2e8f0;
    display: flex;
    justify-content: center;
    gap: 12px;
  }

  .btn {
    padding: 12px 30px;
    border-radius: 8px;
    font-weight: 700;
    cursor: pointer;
    border: none;
    transition: 0.2s;
  }

  /* Simple Success */
  .btn-success {
    background: #ecfdf5;
    color: #059669;
    border-color: #10b981;
  }

  /* Simple Danger */
  .btn-danger {
    background: #fef2f2;
    color: #dc2626;
    border-color: #ef4444;
  }

  .btn:hover:not(:disabled) {
    opacity: 0.9;
    transform: translateY(-1px);
  }

  .status-badge {
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
  }

  /* ==========================================================================
   6. RESPONSIVENESS
   ========================================================================== */
  @media (max-width: 992px) {
    .comparison-grid {
      grid-template-columns: 1fr;
    }

    .vs-divider {
      transform: rotate(90deg);
      padding: 20px 0;
    }

    .app-layout {
      padding: 20px;
    }
  }

  @media (max-width: 768px) {
    .filters {
      flex-direction: column;
      align-items: stretch;
    }

    .tab-btn {
      padding: 10px;
      font-size: 0.8rem;
    }
  }

  /* Animations */
  .fade-slide-enter-active, .fade-slide-leave-active {
    transition: opacity 0.3s, transform 0.3s;
  }

  .fade-slide-enter-from, .fade-slide-leave-to {
    opacity: 0;
    transform: translateY(10px);
  }
</style>
