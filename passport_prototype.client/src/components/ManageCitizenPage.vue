<template>
  <div id="app-layout">
    <Header />

    <div class="main-body-wrapper">
      <LeftMenu :current="current" @navigate="onNavigate" />

      <div class="content-area">
        <div class="page-content">
          <h2 class="page-title">Manage Citizens (Kit Assignment)</h2>

          <div class="action-bar">
            <div class="search-group">axio
              <input type="text" v-model="searchTerm" placeholder="Search by ID or Application Code..." class="search-input" />
              <i class="fas fa-search search-icon"></i>
            </div>
            <button class="add-btn refresh-btn" @click="LoadCitizens">Refresh Data</button>
          </div>

          <div class="table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Application Code</th>
                  <th>Kit Name Assigned</th>
                  <th>First Name</th>
                  <th>Last Name</th>
                  <th>Biometric Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="citizen in filteredCitizens" :key="citizen.citizenID">
                  <td>{{ citizen.citizenID }}</td>
                  <td>{{ citizen.applicationCode }}</td>
                  <td>{{ citizen.latestKitName || 'N/A' }}</td>
                  <td>{{ citizen.firstName }}</td>
                  <td>{{ citizen.lastName }}</td>
                  <td>{{ citizen.latestBiometricStatus }}</td>
                  <td class="action-cell">
                    <!-- ONLY show Release button if latestBiometricStatus is 1 (Success) -->
                    <button v-if="citizen.latestBiometricStatus === 1"
                            class="action-btn release-btn"
                            @click="openReleaseConfirmation(citizen)"
                            :disabled="isLoading">
                      Release
                    </button>
                  </td>
                </tr>
              </tbody>
              <tfoot v-if="!filteredCitizens.length">
                <tr>
                  <td colspan="8" class="text-center p-4 text-gray-600">
                    {{ isLoading ? 'Loading...' : 'No Citizen records found on this page.' }}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>

          <div class="pagination-controls mt-6" v-if="citizens.length > 0 || currentPage > 1">
            <button @click="prevPage"
                    :disabled="currentPage === 1 || isLoading"
                    class="pagination-btn prev-btn">
              &larr; Previous
            </button>
            <span class="page-info">
              Page {{ currentPage }}
            </span>
            <button @click="nextPage"
                    :disabled="!hasNextPage || isLoading"
                    class="pagination-btn next-btn">
              Next &rarr;
            </button>
          </div>

          <!-- Release Confirmation Modal -->
          <div v-if="showConfirmModal && citizenToRelease" class="form-overlay" @click.self="cancelRelease">
            <div class="form-popup">
              <h3>Confirm Kit Release</h3>

              <p class="mb-4 text-base leading-relaxed">
                You are about to release the kit assignment for citizen
                <strong class="text-red-700">{{ citizenToRelease.applicationCode }} (ID: {{ citizenToRelease.citizenID }})</strong>.
                This action will attempt to clear their assigned kit and biometric status.
              </p>

              <div class="form-actions">
                <button type="button" class="cancel-btn" @click="cancelRelease" :disabled="isLoading">Cancel</button>
                <button type="button" class="action-btn release-btn" @click="executeRelease" :disabled="isLoading">
                  Yes, Release Kit
                </button>
              </div>
            </div>
          </div>

          <DialogBox :show="showDialog" :title="dialogTitle" :message="dialogMessage" @close="showDialog = false" />

          <LoadingDialog :visible="isLoading" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, watch, onMounted } from "vue";
  import { useAuthStore } from "@/stores/auth";
  import api from "@/api";

  // Assuming these components exist in the directory structure
  import LeftMenu from "@/components/LeftMenu.vue";
  //import Header from "./Header.vue";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "./LoadingDialog.vue";

  // --- Dialog & Loading ---
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);
  const searchTerm = ref("");

  // --- Pagination State ---
  const currentPage = ref(1);
  const pageSize = ref(10);
  const citizens = ref([]);
  const hasNextPage = computed(() => citizens.value.length === pageSize.value);

  // --- Release Confirmation State ---
  const showConfirmModal = ref(false);
  const citizenToRelease = ref(null); // Stores the citizen object pending release

  // --- Auth store ---
  const auth = useAuthStore();
  const users = ref([]);

  // --- Navigation state ---
  const current = ref("admin");
  const onNavigate = (item) => {
    current.value = item;
  };

  // --- Filtering Logic ---
  const filteredCitizens = computed(() => {
    let list = citizens.value;

    if (searchTerm.value) {
      const lowerCaseSearch = searchTerm.value.toLowerCase();
      list = list.filter(citizen =>
        // Added null/undefined checks (|| '') to prevent potential crash if fields are missing
        String(citizen.citizenID).includes(lowerCaseSearch) ||
        (citizen.applicationCode || '').toLowerCase().includes(lowerCaseSearch) ||
        (citizen.firstName || '').toLowerCase().includes(lowerCaseSearch) ||
        (citizen.lastName || '').toLowerCase().includes(lowerCaseSearch)
      );
    }

    // Sort by ID (Numerical sort, ascending)
    return list.slice().sort((a, b) => {
      const idA = parseInt(a.citizenID, 10);
      const idB = parseInt(b.citizenID, 10);
      return idA - idB;
    });
  });

  // --- Core Data Loading Function ---
  async function LoadCitizens() {
    try {
      isLoading.value = true;
      // API call to fetch citizen list
      const res = await api.get(`/admin/applicant-list?page=${currentPage.value}&pageSize=${pageSize.value}`);

      const newCitizens = res.data || [];
      citizens.value = newCitizens;
      hasNextPage.value = newCitizens.length === pageSize.value;

      // Logic to handle empty pages when navigating back
      if (newCitizens.length === 0 && currentPage.value > 1) {
        hasNextPage.value = false;
        currentPage.value = Math.max(1, currentPage.value - 1);
        if (currentPage.value > 0) {
          await LoadCitizens();
        }
      }

    } catch (err) {
      console.error("AxiosError:", err);
      if (err.response && err.response.status === 404 && currentPage.value === 1) {
        citizens.value = []; // No records at all
      } else {
        showDialog.value = true;
        dialogTitle.value = "Error Loading Data";
        dialogMessage.value = "Failed to load citizen data. Please check the network connection.";
      }
    } finally {
      isLoading.value = false;
    }
  }

  // --- Pagination Actions ---
  function prevPage() {
    if (currentPage.value > 1) {
      currentPage.value--;
    }
  }

  function nextPage() {
    if (hasNextPage.value) {
      currentPage.value++;
    }
  }

  // --- RELEASE LOGIC (Updated to use Confirmation Modal) ---

  /**
   * Opens the confirmation modal for kit release.
   * @param {Object} citizen - The citizen object to release the kit from.
   */
  function openReleaseConfirmation(citizen) {
    if (isLoading.value) return;
    citizenToRelease.value = citizen;
    showConfirmModal.value = true;
  }

  /**
   * Closes the confirmation modal and clears the state.
   */
  function cancelRelease() {
    showConfirmModal.value = false;
    citizenToRelease.value = null;
  }

  /**
   * Executes the kit release API call after confirmation.
   * This calls the /admin/reset-to-kit endpoint.
   */
  async function executeRelease() {
    const citizen = citizenToRelease.value;

    // Guard clause checking for the only required field.
    if (!citizen || !citizen.applicationCode) {
      cancelRelease();
      showDialog.value = true;
      dialogTitle.value = "Error";
      dialogMessage.value = "Cannot release kit. Citizen application code is missing.";
      return;
    }

    cancelRelease(); // Close modal immediately

    try {
      isLoading.value = true;

      // ATTEMPT 3: Setting kitName and kitUser to null.
      // The previous attempt (using empty strings "") failed because the server rejected it,
      // explicitly stating 'Kit Name and Kit User are required.' Null is the last attempt
      // before assuming this is the wrong endpoint for 'release'.
      await api.put("/admin/reset-to-kit", {
        applicationCode: citizen.applicationCode,
      });


      // Success Handling
      showDialog.value = true;
      dialogTitle.value = "Kit Released Successfully";
      dialogMessage.value = `✅ Citizen ${citizen.applicationCode} released.`;

    } catch (err) {
      console.error("Kit Release Failed:", err);
      // Error Handling
      showDialog.value = true;
      dialogTitle.value = "Error";
      let errorMessage = "Failed to release kit assignment due to an unexpected error or network issue.";

      // Improved error reporting for 400 status
      if (err.response) {
        const status = err.response.status;
        const dataMessage = err.response.data?.message;

        if (status === 400) {
          // If the server returns a specific message, use it
          errorMessage = `Server Error (400 - Bad Request): ${dataMessage || 'The server rejected the request. It seems the API requires valid (non-null/non-empty) Kit Name and Kit User even for a reset operation, suggesting this might be the wrong endpoint.'}`;
        } else {
          errorMessage = `Server Error (${status}): Could not complete the release action.`;
        }
      }
      dialogMessage.value = errorMessage;

    } finally {
      isLoading.value = false;
      // 3. Reload the current page to reflect the status and kit change
      LoadCitizens();
    }
  }

  // --- Lifecycle Hooks and Watchers ---

  // Watch for changes in the current page to reload citizen list
  watch(currentPage, () => {
    LoadCitizens();
  });

  onMounted(async () => {
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
    // Initial load of citizens for page 1
    LoadCitizens();
  });
</script>

<style scoped>
  /* --- BASE LAYOUT (Adapted from SystemUser page) --- */
  #app-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }

  .main-body-wrapper {
    display: flex;
    flex: 1;
    padding-top: 60px; /* Space for fixed Header */
  }

  .content-area {
    flex-grow: 1;
    overflow-y: auto;
    background-color: #f8f9fa;
    margin-left: 250px; /* Space for fixed LeftMenu */
  }

  .page-content {
    padding: 30px;
    min-height: 100%;
  }

  .page-title {
    font-size: 2em;
    color: #0d47a1;
    margin-bottom: 25px;
    font-weight: 700;
    border-bottom: 2px solid #e0e0e0;
    padding-bottom: 10px;
  }

  /* --- ACTION BAR (Search and Refresh Button) --- */
  .action-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
    background-color: white;
    padding: 15px;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  }

  .search-group {
    position: relative;
    display: flex;
    align-items: center;
    width: 350px; /* Slightly wider search */
  }

  .search-input {
    width: 100%;
    padding: 10px 10px 10px 40px;
    border: 1px solid #ccc;
    border-radius: 6px;
    font-size: 1em;
    transition: border-color 0.2s, box-shadow 0.2s;
  }

    .search-input:focus {
      border-color: #0d47a1;
      box-shadow: 0 0 0 3px rgba(13, 71, 161, 0.2);
      outline: none;
    }

  .search-icon {
    position: absolute;
    left: 15px;
    color: #777;
    pointer-events: none;
  }

  .refresh-btn {
    background-color: #4caf50;
    color: white;
    border: none;
    padding: 10px 18px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s, transform 0.1s;
  }

    .refresh-btn:hover {
      background-color: #43a047;
      transform: translateY(-1px);
    }

    .refresh-btn:active {
      transform: translateY(0);
    }

  /* --- DATA TABLE --- */
  .table-container {
    overflow-x: auto;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .data-table {
    width: 100%;
    min-width: 900px;
    border-collapse: separate;
    border-spacing: 0;
    background-color: white;
  }

    .data-table th, .data-table td {
      border: none;
      padding: 14px 18px;
      text-align: left;
    }

    .data-table td {
      color: #111;
    }

    .data-table th {
      background-color: #0d47a1;
      color: white;
      text-transform: uppercase;
      font-size: 0.85em;
      font-weight: 600;
      letter-spacing: 0.5px;
    }


    /* Table Row Visuals */
    .data-table thead tr th:first-child {
      border-top-left-radius: 8px;
    }

    .data-table thead tr th:last-child {
      border-top-right-radius: 8px;
    }

    .data-table tbody tr {
      border-bottom: 1px solid #eee;
    }

      .data-table tbody tr:last-child {
        border-bottom: none;
      }

    .data-table tr:nth-child(even) {
      background-color: #f7f7ff;
    }

    .data-table tr:hover {
      background-color: #e3f2fd;
    }

    /* Key Columns Highlight */
    .data-table tbody tr td:nth-child(1),
    .data-table tbody tr td:nth-child(2) {
      color: #333;
      font-weight: 600;
    }

  .action-cell {
    white-space: nowrap;
  }


  /* --- BUTTON STYLES --- */
  .action-btn {
    font-size: 0.9em;
    font-weight: 500;
    padding: 8px 12px;
    margin: 0 4px;
    border-radius: 4px;
    cursor: pointer;
    border: none;
    color: white;
    transition: background-color 0.2s;
  }

  /* Style for 'Release' (Reset Action) */
  .release-btn {
    background-color: #e53935; /* Red for destructive/reset action */
  }

    .release-btn:hover:not(:disabled) {
      background-color: #d32f2f;
    }

  .action-btn:disabled {
    background-color: #ccc;
    cursor: not-allowed;
    opacity: 0.7;
  }


  /* --- STATUS BADGES --- */
  .status-cell {
    font-weight: bold;
    font-size: 0.85em;
    padding: 4px 8px;
    border-radius: 4px;
    text-align: center;
  }

  .status-active { /* Success/Complete */
    background-color: #e8f5e9;
    color: #388e3c;
  }

  .status-inactive { /* Failed/Mismatched */
    background-color: #ffebee;
    color: #d32f2f;
  }

  .status-pending { /* Pending/In Process */
    background-color: #fffde7;
    color: #856404;
  }


  /* --- PAGINATION STYLES --- */
  .pagination-controls {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 15px;
    padding: 15px 0;
  }

  .pagination-btn {
    background-color: #0d47a1;
    color: white;
    border: none;
    padding: 10px 15px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s;
  }

    .pagination-btn:hover:not(:disabled) {
      background-color: #1976d2;
    }

    .pagination-btn:disabled {
      background-color: #bdbdbd;
      cursor: not-allowed;
      opacity: 0.6;
    }

  .page-info {
    font-weight: 600;
    color: #333;
  }

  /* --- MODAL/FORM STYLES (Confirmation Modal) --- */
  .form-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.6);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .form-popup {
    background: white;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.4);
    width: 90%;
    max-width: 400px; /* Smaller size for confirmation */
    animation: fadeInScale 0.2s ease-out;
    color: #1a1a1a;
  }

  @keyframes fadeInScale {
    from {
      opacity: 0;
      transform: scale(0.95);
    }

    to {
      opacity: 1;
      transform: scale(1);
    }
  }

  .form-popup h3 {
    margin-top: 0;
    color: #e53935; /* Red title for caution */
    border-bottom: 2px solid #ffcdd2;
    padding-bottom: 10px;
    margin-bottom: 20px;
    font-weight: 700;
  }

  .form-actions {
    margin-top: 25px;
    display: flex;
    justify-content: flex-end;
    gap: 10px;
  }

  .cancel-btn {
    background-color: #e0e0e0; /* Cancel button */
    color: #333;
    border: none;
    padding: 10px 15px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s;
  }

    .cancel-btn:hover:not(:disabled) {
      background-color: #bdbdbd;
    }

    .cancel-btn:disabled {
      cursor: not-allowed;
      opacity: 0.7;
    }

  /* Re-using release-btn style for the "Yes" confirmation button */
</style>
