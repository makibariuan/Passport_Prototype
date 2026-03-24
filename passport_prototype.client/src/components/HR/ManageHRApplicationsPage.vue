<template>
  <div class="app-layout">
    <LeftMenu :current="current" @navigate="onNavigate" class="leftMenu" />
    <Header class="header" />

    <div class="dashboard-content">
      <h2 class="page-title">
        {{ formatTitle(currentStatusType) }} Employees
      </h2>

      <div class="action-bar">
        <div class="search-group">
          <input type="text" v-model="searchTerm" placeholder="Search by ID, name, department, or designation..." class="search-input" />
          <i class="fas fa-search search-icon"></i>
        </div>
      </div>

      <div class="data-section-card">
        <div v-if="isLoading" class="table-loading-state">
          <p class="text-center p-4 text-gray-600">Loading employee list for {{ formatTitle(currentStatusType) }}...</p>
        </div>

        <div v-else-if="employeeList.length === 0" class="table-loading-state">
          <p class="text-center p-4 text-gray-600">No employees found for status: **{{ formatTitle(currentStatusType) }}**.</p>
        </div>

        <div v-else class="table-container">
          <table class="data-table">
            <thead>
              <tr>
                <th>Employee ID</th>
                <th>Full Name</th>
                <th>Department Name</th>
                <th>Designation</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="employee in filteredEmployees" :key="employee.employeeID ?? employee.id">

                <td>{{ employee.employeeID ?? employee.id }}</td>

                <td>{{ employee.fullName ?? `${employee.firstName} ${employee.lastName}` }}</td>

                <td>{{ employee.departmentName ?? employee.departmentName }}</td>

                <td>{{ employee.designation ?? employee.designation }}</td>

                <td>
                  <span :class="['status-badge', getEmployeeStatusDisplay(employee.status).class]">
                    {{ getEmployeeStatusDisplay(employee.status).text }}
                  </span>
                </td>

                <td class="action-cell">
                  <button @click="viewDetails(employee.employeeID ?? employee.id)" class="action-btn view-btn">View Details</button>
                </td>
              </tr>

              <tr v-if="filteredEmployees.length === 0">
                <td colspan="6" class="table-loading-state text-center p-4 text-gray-600">
                  No employees match the search criteria.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, watch, onMounted, computed } from 'vue';
  import { useRoute } from 'vue-router';
  import api from '@/api';

  // Imports
  import LeftMenu from "@/components/LeftMenu.vue";
  //import Header from "@/components/Header.vue";

  const route = useRoute();

  // --- Local State for Data and UI ---
  const employeeList = ref([]);
  const isLoading = ref(false);
  const currentStatusType = ref('');
  const searchTerm = ref('');
  const current = ref("HR Dashboard - Applications");

  // --- Computed Property for Search Filtering (Retained from previous step) ---
  const filteredEmployees = computed(() => {
    if (!searchTerm.value) {
      // If no search term, return the full list
      return employeeList.value;
    }

    const lowerCaseSearch = searchTerm.value.toLowerCase();

    // Filter the raw API data (employeeList)
    return employeeList.value.filter(employee => {

      // Combine EmployeeID and ID for search
      // Check for employeeID, id, or personID for search
      const employeeID = String(employee.employeeID ?? employee.id ?? employee.personID ?? '').toLowerCase();

      // Handle potentially missing fields with null-safe checks
      const fullName = employee.fullName?.toLowerCase() ??
        `${employee.firstName ?? ''} ${employee.lastName ?? ''}`.trim().toLowerCase();

      const departmentName = (employee.departmentName ?? employee.department)?.toLowerCase() ?? ''; // Check both departmentName and department
      const designation = employee.designation?.toLowerCase() ?? ''; // Will be empty string for 'captured' but search works

      return (
        employeeID.includes(lowerCaseSearch) ||
        fullName.includes(lowerCaseSearch) ||
        departmentName.includes(lowerCaseSearch) ||
        designation.includes(lowerCaseSearch)
      );
    });
  });

  // --- Status Mapping (Based on C# snippet) ---
  const STATUS_MAP = {
    0: { text: 'For Schedule', class: 'schedule' },
    1: { text: 'Scheduled', class: 'scheduled' },
    2: { text: 'Captured', class: 'captured' },
    3: { text: 'For Printing', class: 'forprinting' },
    4: { text: 'Card Activated', class: 'activecards' },
    5: { text: 'Adjudication', class: 'adjudication' },
  };

  // --- Utility Functions (Unchanged) ---
  const getEmployeeStatusDisplay = (statusValue) => {
    const statusKey = parseInt(statusValue);
    const status = STATUS_MAP[statusKey];
    if (status) {
      return status;
    }
    return { text: 'Unknown Status', class: 'default' };
  };

  const onNavigate = (item) => {
    current.value = item;
  };

  const formatTitle = (statusType) => {
    if (!statusType) return 'Loading...';
    return statusType.charAt(0).toUpperCase() + statusType.slice(1).replace(/-/g, ' ');
  };

  // --- Update viewDetails to handle redirection ---
  const viewDetails = (employeeId) => {
    // Check if we are currently looking at the 'adjudication' status list
    if (currentStatusType.value === 'adjudication') {
      router.push({
        name: "AdjudicationDetails",
        params: { id: employeeId }
      });
    } else {
      // Standard view for other statuses
      alert(`Viewing details for Employee ID: ${employeeId}`);
    }
  };

  // --- Core Data Loading Function (with data normalization fix) ---
  const fetchEmployees = async (statusType) => {
    isLoading.value = true;
    currentStatusType.value = statusType;
    let endpoint = '';

    // Map the statusType from the URL to the correct API endpoint
    switch (statusType) {
      case 'all':
        endpoint = '/employee/employees';
        break;
      case 'schedule':
        endpoint = '/employee/schedule';
        break;
      case 'scheduled':
        endpoint = '/employee/employees/scheduled';
        break;
      case 'captured':
        // 🚨 CRITICAL ENDPOINT: Returns EmployeeIDPrintDto
        endpoint = '/employee/print-cards';
        break;
      case 'for-printing':
        endpoint = '/employee/employees/for-printing';
        break;
      case 'active-cards':
        endpoint = '/employee/employees/active-cards';
        break;
      case 'adjudication':
        endpoint = '/employee/employees/adjudication'; // Your hypothetical endpoint for hits
        break;
      default:
        console.error('Unknown status type:', statusType);
        isLoading.value = false;
        return;
    }

    try {
      const res = await api.get(endpoint);
      let fetchedData = res.data;

      // 🚨 FIX: Data Normalization for 'captured' endpoint
      if (statusType === 'captured' && Array.isArray(fetchedData)) {
        console.log(`[ManageHRApplicationsPage] Normalizing ${fetchedData.length} 'captured' records.`);

        // Map the EmployeeIDPrintDto fields to the expected EmployeeListDto fields
        fetchedData = fetchedData.map(item => ({
          // Map PersonID (from DTO) to employeeID (expected by table)
          employeeID: item.personID,
          fullName: item.fullName,
          // Map 'department' (from DTO) to 'departmentName' (expected by table)
          departmentName: item.department,
          designation: 'N/A', // Inject missing field
          status: 2, // 🎯 Inject the correct status (2 = Captured)
          // Keep other original fields for reference (e.g., dateCapture, photo)
          ...item
        }));
      }

      // 🎯 CONSOLE LOG for successful data!
      console.log(`[ManageHRApplicationsPage] Data fetched for status: ${statusType}`, fetchedData);

      employeeList.value = fetchedData;
    } catch (error) {
      console.error("Failed to fetch employee list:", error);

      // 🎯 CONSOLE LOG for API error details
      if (error.response) {
        console.error("[API ERROR] Response status:", error.response.status);
        console.error("[API ERROR] Response data:", error.response.data);
      }

      employeeList.value = [];
    } finally {
      isLoading.value = false;
    }
  };

  // --- Lifecycle Hooks and Watchers (Unchanged) ---
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
</script>

<style scoped>
  /*
  * NOTE: The style block remains identical to your provided ManageEmployees styles,
  * ensuring the look and feel is consistent.
  * The only addition needed is a style for '.status-badge.default' if you want a color
  * for unknown statuses. I've left the original status colors.
  */

  /* ************************************************************************** */
  /* 1. DASHBOARD GRID LAYOUT STYLES (Copied from DashboardHR/ManageEmployees) */
  /* ************************************************************************** */
  .app-layout {
    display: grid;
    grid-template-columns: 24vw 1fr;
    grid-template-rows: auto 1fr;
    min-height: 100vh;
    background-color: #f8f9fa;
  }

  .leftMenu {
    grid-column: 1;
    grid-row: 1 / span 2;
    z-index: 10;
  }

  .header {
    grid-column: 2;
    grid-row: 1;
    z-index: 5;
  }

  .dashboard-content {
    grid-column: 2;
    grid-row: 2;
    width: 100%;
    min-height: 100%;
    padding: 30px;
    box-sizing: border-box;
    font-family: 'Inter', sans-serif, Arial, Helvetica;
    overflow-y: auto;
  }

  /* ************************************************************************** */
  /* 2. PAGE CONTENT & ACTION BAR STYLES (Copied from ManageEmployees) */
  /* ************************************************************************** */

  .page-title {
    font-size: 2.2em;
    color: #004085;
    margin-bottom: 25px;
    font-weight: 800;
  }

  .action-bar {
    display: flex;
    justify-content: flex-start;
    align-items: center;
    margin-bottom: 25px;
    padding: 0;
  }

  .search-group {
    position: relative;
    display: flex;
    align-items: center;
    width: 400px;
    max-width: 100%;
  }

  .search-input {
    width: 100%;
    padding: 12px 12px 12px 45px;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    font-size: 1em;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
    transition: border-color 0.2s, box-shadow 0.2s;
  }

    .search-input:focus {
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.2);
      outline: none;
    }

  .search-icon {
    position: absolute;
    left: 18px;
    color: #777;
    pointer-events: none;
    font-size: 1.1em;
  }

  .data-section-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
    padding: 20px;
    overflow: hidden;
  }

  /* ************************************************************************** */
  /* 3. DATA TABLE STYLES */
  /* ************************************************************************** */

  .table-container {
    overflow-x: auto;
  }

  .data-table {
    width: 100%;
    min-width: 800px;
    border-collapse: collapse;
    background-color: white;
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
      background-color: #f7f7ff;
    }

    .data-table tbody tr td:nth-child(1),
    .data-table tbody tr td:nth-child(2),
    .data-table tbody tr td:nth-child(3),
    .data-table tbody tr td:nth-child(4) {
      color: #004085;
      font-weight: 600;
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
      background-color: #007bff;
      color: white;
      border-color: #007bff;
    }

      .action-btn.view-btn:hover {
        background-color: #0056b3;
        border-color: #0056b3;
      }

  /* --- STATUS BADGES (Adapted from DashboardHR colors) --- */
  .status-badge {
    padding: 6px 10px;
    border-radius: 50px;
    font-weight: 700;
    font-size: 0.75em;
    text-transform: uppercase;
    color: white;
    min-width: 80px;
    display: inline-block;
    text-align: center;
  }

    /* Colors matching DashboardHR card left borders: */
    .status-badge.schedule {
      background-color: #ffc107;
      color: #343a40;
    }
    /* Yellow */
    .status-badge.scheduled {
      background-color: #007bff;
    }
    /* Blue */
    .status-badge.captured {
      background-color: #28a745;
    }
    /* Green */
    .status-badge.forprinting {
      background-color: #6f42c1;
    }
    /* Purple */
    .status-badge.activecards {
      background-color: #dc3545;
    }
    /* Red */
    .status-badge.all {
      background-color: #004085;
    }
    /* Dark Blue (For generic 'all' filter view) */
    .status-badge.default {
      background-color: #6c757d;
    }
  /* Grey for unknown status */


  /* ************************************************************************** */
  /* 4. RESPONSIVENESS */
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

    .search-group {
      width: 100%;
    }
  }
</style>
