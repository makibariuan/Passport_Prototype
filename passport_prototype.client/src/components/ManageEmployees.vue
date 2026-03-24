<template>
  <div class="app-layout">
    <LeftMenu :current="current" @navigate="onNavigate" class="leftMenu" />
    <Header class="header" />

    <div class="dashboard-content">
      <h2 class="page-title">Manage Employees</h2>

      <div class="action-bar">
        <div class="search-group">
          <input type="text" v-model="searchTerm" placeholder="Search by ID, name, designation, or department..." class="search-input" />
          <i class="fas fa-search search-icon"></i>
        </div>
      </div>

      <div class="data-section-card">
        <div class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>Employee ID</th>
                <th>Full Name</th>
                <th>Designation</th>
                <th>Department Name</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="employee in filteredEmployees" :key="employee.employeeID">
                <td>{{ employee.employeeID }}</td>
                <td>{{ employee.fullName }}</td>
                <td>{{ employee.designation ?? 'N/A' }}</td>
                <td>{{ employee.departmentName ?? 'N/A' }}</td>
                <td>
                  <span :class="{'status-active': employee.status, 'status-inactive': !employee.status}">
                    {{ employee.status ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="action-cell">
                  <button class="action-btn view-btn" @click="viewEmployeeDetails(employee)">View</button>
                </td>
              </tr>
            </tbody>
            <tfoot v-if="!filteredEmployees.length">
              <tr>
                <td colspan="6" class="text-center p-4 text-gray-600">
                  {{ isLoading ? 'Loading...' : 'No Employees found.' }}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>

        <div class="pagination-controls" v-if="employees.length > 0 || currentPage > 1">
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
      </div>

      <DialogBox :show="showDialog" :title="dialogTitle" :message="dialogMessage" @close="showDialog = false" />
      <LoadingDialog :visible="isLoading" />
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, watch, onMounted } from "vue";
  import { useAuthStore } from "@/stores/auth";
  import api from "@/api";

  // Assuming component paths are correct relative to the project structure
  import LeftMenu from "@/components/LeftMenu.vue";
  // Re-including Header based on the layout structure
  //import Header from "@/components/Header.vue";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";

  // --- Dialog & Loading ---
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);
  const searchTerm = ref("");

  // --- Pagination State ---
  const currentPage = ref(1);
  const pageSize = ref(10);
  const employees = ref([]);

  // Determine if there is a next page based on the number of employees returned
  const hasNextPage = computed(() => {
    // This logic relies on the API to return *exactly* pageSize items if there is a next page.
    return employees.value.length === pageSize.value;
  });

  // --- Auth store & user data ---
  const auth = useAuthStore();

  // --- Navigation state ---
  const current = ref("Manage Employees");
  const onNavigate = (item) => {
    current.value = item;
  };

  // Filtered Employees computed property
  const filteredEmployees = computed(() => {
    let list = employees.value;

    // 1. Filtering (only if search term exists)
    if (searchTerm.value) {
      const lowerCaseSearch = searchTerm.value.toLowerCase();
      list = list.filter(employee =>
        // Search by Employee ID
        String(employee.employeeID).includes(lowerCaseSearch) ||
        // Search by Full Name
        employee.fullName?.toLowerCase().includes(lowerCaseSearch) ||
        // Search by Designation
        employee.designation?.toLowerCase().includes(lowerCaseSearch) ||
        // Search by Department Name
        employee.departmentName?.toLowerCase().includes(lowerCaseSearch)
      );
    }

    // 2. Sorting by EmployeeID (Numerical sort, ascending)
    return list.slice().sort((a, b) => {
      // Use employeeID for sorting
      const idA = parseInt(a.employeeID, 10);
      const idB = parseInt(b.employeeID, 10);
      return idA - idB;
    });
  });

  // --- Core Data Loading Function ---
  async function LoadEmployees() {
    try {
      isLoading.value = true;
      // UPDATED API ENDPOINT and kept pagination parameters
      const url = `/employee/employees?page=${currentPage.value}&pageSize=${pageSize.value}`;
      const res = await api.get(url);

      // Assuming the API returns an array of DTOs (employeeID, fullName, etc.)
      employees.value = res.data;

    } catch (err) {
      console.error("AxiosError:", err);
      showDialog.value = true;
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to load employee list. Please check network or authorization.";

      // If no data is returned on a page > 1, attempt to go back
      if (err.response && err.response.status === 404 && currentPage.value > 1) {
        if (employees.value.length === 0) {
          currentPage.value = Math.max(1, currentPage.value - 1);
          LoadEmployees();
          return;
        }
      }

    } finally {
      isLoading.value = false;
    }
  }

  // Placeholder function for future employee detail navigation/modal
  function viewEmployeeDetails(employee) {
    showDialog.value = true;
    dialogTitle.value = "Employee Details";
    dialogMessage.value = `Viewing details for Employee ID: ${employee.employeeID} (${employee.fullName}).`;
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

  // --- Lifecycle Hooks and Watchers ---

  // Watch for changes in currentPage and reload data automatically
  watch(currentPage, () => {
    LoadEmployees();
  });

  // Initial load on mount
  onMounted(async () => {
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
    LoadEmployees();
  });
</script>

<style scoped>
  /* ************************************************************************** */
  /* 1. DASHBOARD GRID LAYOUT STYLES */
  /* ************************************************************************** */

  .app-layout {
    display: grid;
    /* Define columns: 24vw for sidebar, 1fr for main content */
    grid-template-columns: 24vw 1fr;
    /* Define rows: auto for the header, 1fr for the scrolling content area */
    grid-template-rows: auto 1fr;
    min-height: 100vh;
    background-color: #f8f9fa; /* Background color for the main body */
  }

  .leftMenu {
    /* Occupies the entire first column (rows 1 and 2) */
    grid-column: 1;
    grid-row: 1 / span 2;
    z-index: 10;
  }

  .header {
    /* Occupies the second column, first row */
    grid-column: 2;
    grid-row: 1;
    z-index: 5;
  }

  .dashboard-content {
    /* Places content in the second column, second row */
    grid-column: 2;
    grid-row: 2;
    width: 100%;
    min-height: 100%;
    padding: 30px;
    box-sizing: border-box;
    font-family: 'Inter', sans-serif, Arial, Helvetica;
    overflow-y: auto; /* Allow scrolling within the content area */
  }

  /* ************************************************************************** */
  /* 2. PAGE CONTENT & ACTION BAR STYLES */
  /* ************************************************************************** */

  .page-title {
    font-size: 2.2em;
    color: #004085; /* Dark blue title */
    margin-bottom: 25px;
    font-weight: 800;
    /* Removed border-bottom for a cleaner look */
  }

  .action-bar {
    display: flex;
    justify-content: flex-start; /* Aligned search to the left */
    align-items: center;
    margin-bottom: 25px;
    /* Removed background/shadow from action bar, placing focus on the search input */
    padding: 0;
  }

  .search-group {
    position: relative;
    display: flex;
    align-items: center;
    width: 400px; /* Wider search bar for dashboard look */
    max-width: 100%;
  }

  .search-input {
    width: 100%;
    padding: 12px 12px 12px 45px; /* Increased padding */
    border: 1px solid #e0e0e0;
    border-radius: 8px; /* Slightly more rounded corners */
    font-size: 1em;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05); /* Subtle inner shadow */
    transition: border-color 0.2s, box-shadow 0.2s;
  }

    .search-input:focus {
      border-color: #007bff; /* Primary blue on focus */
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.2);
      outline: none;
    }

  .search-icon {
    position: absolute;
    left: 18px; /* Adjusted position */
    color: #777;
    pointer-events: none;
    font-size: 1.1em;
  }

  /* Container for the table, adding a card aesthetic */
  .data-section-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08); /* Stronger shadow for the card */
    padding: 20px;
    overflow: hidden; /* Contains the table shadow */
  }

  /* ************************************************************************** */
  /* 3. DATA TABLE STYLES */
  /* ************************************************************************** */

  .table-container {
    overflow-x: auto;
    /* Removed inner shadow/border as it's now wrapped by data-section-card */
  }

  .data-table {
    width: 100%;
    min-width: 800px;
    border-collapse: collapse; /* Changed to collapse for cleaner lines inside the card */
    background-color: white;
  }

    .data-table th, .data-table td {
      border: none; /* Removed default borders */
      padding: 15px 20px;
      text-align: left;
    }

    .data-table th {
      background-color: #f1f4f8; /* Light header background */
      color: #333;
      text-transform: uppercase;
      font-size: 0.8em;
      font-weight: 700;
      letter-spacing: 0.5px;
      border-bottom: 2px solid #e0e0e0;
    }

    /* Resetting rounded corners on table headers as the container handles the main rounding */
    .data-table thead tr th:first-child {
      border-top-left-radius: 0;
    }

    .data-table thead tr th:last-child {
      border-top-right-radius: 0;
    }

    .data-table tbody tr {
      border-bottom: 1px solid #eeeeee;
      transition: background-color 0.15s;
    }

      .data-table tbody tr:last-child {
        border-bottom: none;
      }

    .data-table tr:hover {
      background-color: #f7f7ff;
    }

    .data-table tbody tr td:nth-child(1),
    .data-table tbody tr td:nth-child(2) {
      color: #004085; /* Highlight key fields */
      font-weight: 600;
    }

  .action-cell {
    white-space: nowrap;
  }

  /* --- BUTTON STYLES --- */
  .action-btn {
    font-size: 0.9em;
    font-weight: 600;
    padding: 6px 14px;
    margin: 0 4px;
    border-radius: 6px;
    cursor: pointer;
    border: 1px solid transparent;
    transition: all 0.2s;
  }

    .action-btn.view-btn {
      background-color: #007bff; /* Primary blue button */
      color: white;
      border-color: #007bff;
    }

      .action-btn.view-btn:hover {
        background-color: #0056b3;
        border-color: #0056b3;
      }

  /* --- STATUS BADGES --- */
  .status-active, .status-inactive {
    padding: 6px 10px;
    border-radius: 50px; /* Pill shape */
    font-weight: 700;
    font-size: 0.75em;
    text-transform: uppercase;
  }

  .status-active {
    background-color: #d4edda;
    color: #155724;
  }

  .status-inactive {
    background-color: #f8d7da;
    color: #721c24;
  }

  /* ************************************************************************** */
  /* 4. PAGINATION STYLES */
  /* ************************************************************************** */

  .pagination-controls {
    display: flex;
    justify-content: flex-end; /* Align pagination controls to the right */
    align-items: center;
    gap: 15px;
    padding: 20px 0 0;
    margin-top: 10px;
    border-top: 1px solid #eee; /* Separator from the table */
  }

  .pagination-btn {
    background-color: #f1f4f8; /* Light gray background */
    color: #004085;
    border: 1px solid #ced4da;
    padding: 8px 12px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: all 0.2s;
  }

    .pagination-btn:hover:not(:disabled) {
      background-color: #e9ecef;
      border-color: #b8c1cb;
    }

    .pagination-btn:disabled {
      background-color: #f8f9fa;
      color: #adb5bd;
      cursor: not-allowed;
      border-color: #f8f9fa;
    }

  .page-info {
    font-weight: 600;
    color: #5a6268;
  }

  /* ************************************************************************** */
  /* 5. RESPONSIVENESS (Adopted from dashboard styles) */
  /* ************************************************************************** */
  @media (max-width: 1200px) {
    .app-layout {
      /* Adjust grid for smaller sidebar width on tablet */
      grid-template-columns: 20vw 1fr;
    }
  }

  @media (max-width: 992px) {
    .app-layout {
      /* Adjust grid for smaller sidebar width on tablet */
      grid-template-columns: 80px 1fr;
    }
  }

  @media (max-width: 768px) {
    .app-layout {
      /* Switch to single column stack on mobile */
      grid-template-columns: 1fr;
      grid-template-rows: auto auto 1fr;
    }

    .leftMenu, .header, .dashboard-content {
      grid-column: 1; /* All elements stack in the first column */
    }

    .leftMenu {
      grid-row: 2; /* Menu under the header (if it becomes a top bar) */
      width: 100%;
      min-height: auto;
    }

    .header {
      grid-row: 1; /* Header stays at the top */
    }

    .dashboard-content {
      grid-row: 3; /* Content below the menu */
      padding: 20px 15px;
    }

    .search-group {
      width: 100%;
    }
  }
</style>
