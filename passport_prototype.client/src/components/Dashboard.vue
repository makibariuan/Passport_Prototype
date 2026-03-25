<template>
  <!--<LeftMenu @navigate="onNavigate" class="leftMenu" />-->
  <LeftMenu @navigate="onNavigate" class="leftMenu" />

  <Header class="header" />

  <div class="dashboard-content">
    <h1 class="welcome-title">System User Dashboard 🛡️</h1>
    <p class="role-info">
      Welcome back, {{ username }}. Use the section below to **view** senior citizen records.
    </p>

    <section class="summary-grid">
      <router-link to="/manage-citizens" class="summary-card citizen-card">
        <div class="card-content">
          <p class="card-label">Citizens Created</p>
          <p class="metric-number">{{ statisticsData.citizenCount || 0 }}</p>
          <p class="card-detail">View All Senior Citizen Records</p>
        </div>
        <i class="fas fa-address-card card-icon"></i>
      </router-link>
    </section>
    <div v-if="isLoading" class="loading-overlay">
      <p>Loading data...</p>
      <p>If this persists, check network or authorization.</p>
    </div>

    <div v-if="error" class="error-box">
      <p>{{ error }}</p>
    </div>

    <DialogBox :show="showDialog"
               :title="dialogTitle"
               :message="dialogMessage"
               @close="showDialog = false" />

    <LoadingDialog :visible="isLoading" />
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from "vue";
  import LeftMenu from "./LeftMenu.vue";

  //import Header from "./Header.vue";
  import { useAuthStore } from "@/stores/auth";
  import api from "@/api";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "./LoadingDialog.vue";

  // --- PERSISTENT STATE DEFINITION (The Fix) ---
  // By defining these refs outside the setup function (in the module scope),
  // they act as a simple singleton store. Their values are preserved
  // across component re-mounts (e.g., navigation).
  const statisticsData = ref({});
  const isLoading = ref(false);
  const error = ref(null);

  /**
   * Fetches dashboard statistics from the API, only if data is not already present.
   */
  async function loadStats() {
    // CRITICAL FIX: Only fetch if data is missing AND not currently loading.
    if (Object.keys(statisticsData.value).length > 0 || isLoading.value) {
      console.log("Statistics data is already loaded or being fetched. Skipping API call.");
      return;
    }

    try {
      isLoading.value = true;
      error.value = null; // Clear previous errors
      // NOTE: This endpoint might still return all counts, but we only display CitizenCount
      const res = await api.get(`/admin/statistics`);
      statisticsData.value = res.data;
    } catch (err) {
      console.error("Failed to load statistics data:", err);
      error.value = "Failed to load dashboard statistics. Please check network connectivity.";
      statisticsData.value = {}; // Clear data on failure
    } finally {
      isLoading.value = false;
    }
  }
  // --- END PERSISTENT STATE DEFINITION ---


  // --- Dialog & Auth Initialization (Local State) ---
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");

  // --- Auth store & user data ---
  const auth = useAuthStore();
  const firstname = computed(() => auth.firstName ?? "User");
  const username = computed(() => auth.username ?? "User");
  const userType = computed(() => Number(auth.userType ?? 1));

  // --- Navigation state (local to component) ---
  const current = ref("admin");
  const onNavigate = (item) => {
    current.value = item;
  };

  // --- Component Lifecycle Hook ---
  onMounted(async () => {
    // 1. Ensure user is loaded (important for auth checks)
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
    // 2. Load stats (will only run the API call if data is missing)
    loadStats();
  });
</script>

<style scoped>
   /*
  * 1. BASE LAYOUT & SPACING
  * Sets the dashboard container relative to the fixed Header and Sidebar
  */
   .dashboard-content {
     /* CRITICAL FIXES for fixed components (Header: 85px, assumed Sidebar: 250px) */
     margin-top: 85px;
     margin-left: 250px;
     width: calc(100% - 250px);
     min-height: calc(100vh - 85px);
     padding: 30px;
     background-color: #f8f9fa; /* Lighter, cleaner background */
     box-sizing: border-box; /* Include padding/border in element's total width and height */
     font-family: 'Inter', sans-serif, Arial, Helvetica;
   }

   .welcome-title {
     font-size: 2.2em;
     color: #004085; /* Match header blue */
     margin-bottom: 5px;
     font-weight: 800;
   }

   .role-info {
     margin-bottom: 30px;
     color: #5a6268;
     font-style: italic;
     font-size: 1.05em;
   }

   /* * 2. SUMMARY CARD GRID LAYOUT
  * Uses CSS Grid for responsive, flexible columns
  */
   .summary-grid {
     display: grid;
     /* Auto-fit creates columns that are at least 300px wide, max 1fr (equal width) */
     /* Adjust grid to look better with only one card. It will now occupy one column. */
     grid-template-columns: minmax(300px, 400px);
     gap: 30px;
     margin-bottom: 40px;
     /* Optional: Center the single card horizontally if desired, but default left-align is fine. */
     /* justify-content: center; */
   }

   /* * 3. SUMMARY CARD STYLES (Aesthetics)
  */
   .summary-card {
     display: flex;
     justify-content: space-between;
     align-items: center;
     padding: 25px;
     background-color: white;
     border-radius: 12px; /* Rounded corners */
     text-decoration: none; /* Remove underline from router-link */
     /* Subtle lift effect */
     box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
     transition: transform 0.2s ease, box-shadow 0.2s ease;
     cursor: pointer;
     border-left: 6px solid transparent; /* Placeholder for color-coding */
   }

     .summary-card:hover {
       transform: translateY(-5px); /* Lift on hover */
       box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
     }

   .card-content {
     flex-grow: 1;
   }

   .card-label {
     font-size: 1.1em;
     color: #333;
     margin: 0;
     font-weight: 600;
     text-transform: uppercase;
     letter-spacing: 0.5px;
   }

   .metric-number {
     font-size: 3.5em;
     color: #004085;
     margin: 5px 0 0px;
     font-weight: 800;
     line-height: 1;
   }

   .card-detail {
     color: #777;
     font-size: 0.9em;
     margin: 0;
   }

   .card-icon {
     font-size: 3.5em;
     margin-left: 20px;
     opacity: 0.6;
   }

   /* Color Coding for Cards */
   /* REMOVED: .kit-user-card styles */
   /* REMOVED: .system-user-card styles */

   .citizen-card {
     border-left-color: #28a745; /* Green */
   }

     .citizen-card .card-icon {
       color: #28a745;
     }

   /* * 4. RESPONSIVENESS (Mobile/Tablet Adjustments)
  */
   @media (max-width: 1024px) {
     /* Smaller screens: Reduce sidebar space */
     .dashboard-content {
       margin-left: 100px; /* Assuming narrower sidebar on tablet */
       width: calc(100% - 100px);
     }
   }

   @media (max-width: 768px) {
     /* Mobile: Full width, no sidebar margin */
     .dashboard-content {
       margin-left: 0;
       margin-top: 60px; /* Match responsive header height (60px) */
       width: 100%;
       padding: 20px;
     }

     .welcome-title {
       font-size: 1.8em;
     }

     .metric-number {
       font-size: 3em;
     }

     .summary-grid {
       grid-template-columns: 1fr; /* Stack cards vertically */
     }
   }

   /* --- OVERLAY & ERROR STYLES (Kept for completeness, slightly adjusted) --- */

   .loading-overlay, .error-box {
     padding: 15px;
     border-radius: 8px;
     position: fixed;
     bottom: 20px;
     right: 20px;
     box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
     z-index: 1001;
     font-weight: bold;
   }

   .loading-overlay {
     background-color: #e3f2fd; /* Light blue */
     color: #004085;
   }

   .error-box {
     background-color: #f8d7da; /* Light red */
     color: #721c24;
   }

   /* Existing utility styles (rest of the file) */
   .section-title {
     font-size: 1.5em;
     color: #333;
     border-bottom: 2px solid #ccc;
     padding-bottom: 5px;
     margin-top: 30px;
     margin-bottom: 15px;
   }

   /* The rest of the existing styles for buttons, tables, forms, etc. remain unchanged... */
   .add-btn {
     background-color: #4caf50;
     color: white;
     border: none;
     padding: 10px 15px;
     border-radius: 5px;
     cursor: pointer;
     margin-bottom: 20px;
     transition: background-color 0.2s;
   }

     .add-btn:hover {
       background-color: #43a047;
     }

   .user-table {
     width: 100%;
     border-collapse: collapse;
     box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
     background-color: white;
   }

     .user-table th, .user-table td {
       border: 1px solid #ddd;
       padding: 12px 15px;
       text-align: left;
     }

     .user-table th {
       background-color: #004085; /* Match primary color */
       color: white;
       text-transform: uppercase;
       font-size: 0.9em;
     }

     .user-table tr:nth-child(even) {
       background-color: #f9f9f9;
     }

     .user-table tr:hover {
       background-color: #f1f1f1;
     }

   /* --- BUTTON STYLES --- */
   .status-btn {
     background-color: #ff9800;
     color: white;
     border: none;
     padding: 6px 10px;
     border-radius: 4px;
     cursor: pointer;
     margin-right: 8px;
     transition: background-color 0.2s;
   }

     .status-btn:hover {
       background-color: #fb8c00;
     }

   .reset-btn {
     background-color: #2196f3;
     color: white;
     border: none;
     padding: 6px 10px;
     border-radius: 4px;
     cursor: pointer;
     transition: background-color 0.2s;
   }

     .reset-btn:hover {
       background-color: #1e88e5;
     }

   /* --- STATUS STYLES --- */
   .status-active, .status-inactive {
     padding: 4px 8px;
     border-radius: 4px;
     font-weight: bold;
     font-size: 0.85em;
   }

   .status-active {
     background-color: #e8f5e9;
     color: #388e3c;
   }

   .status-inactive {
     background-color: #ffebee;
     color: #d32f2f;
   }

   /* Modal/Form Styles */
   .form-overlay {
     position: fixed;
     top: 0;
     left: 0;
     width: 100%;
     height: 100%;
     background-color: rgba(0, 0, 0, 0.5);
     display: flex;
     justify-content: center;
     align-items: center;
     z-index: 1000;
   }

   .form-popup {
     background: white;
     padding: 30px;
     border-radius: 10px;
     box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
     width: 90%;
     max-width: 450px;
   }

     .form-popup h3 {
       margin-top: 0;
       color: #004085;
       border-bottom: 1px solid #eee;
       padding-bottom: 10px;
       margin-bottom: 20px;
     }

     .form-popup label {
       display: block;
       margin-top: 10px;
       font-weight: bold;
       color: #333;
     }

     .form-popup input, .form-popup select {
       width: 100%;
       padding: 10px;
       margin-top: 5px;
       border: 1px solid #ccc;
       border-radius: 5px;
       box-sizing: border-box;
     }

   .validation-error {
     color: #d32f2f;
     font-size: 0.85em;
     margin: 5px 0 0;
   }

   .form-actions {
     margin-top: 20px;
     display: flex;
     justify-content: flex-end;
     gap: 10px;
   }

   .save-btn {
     background-color: #004085;
     color: white;
     border: none;
     padding: 10px 15px;
     border-radius: 5px;
     cursor: pointer;
     transition: background-color 0.2s;
   }

     .save-btn:hover {
       background-color: #0056b3;
     }

   .cancel-btn {
     background-color: #bbb;
     color: #333;
     border: none;
     padding: 10px 15px;
     border-radius: 5px;
     cursor: pointer;
     transition: background-color 0.2s;
   }

     .cancel-btn:hover {
       background-color: #999;
     }

   .metric-number {
     font-size: 3em;
     color: #0d47a1;
     margin: 5px 0;
     font-weight: 700;
   }

   .detail {
     color: #777;
     margin-bottom: 20px;
   }

   .action-button {
     padding: 10px 20px;
     border: none;
     border-radius: 5px;
     color: white;
     font-weight: bold;
     cursor: pointer;
   }

     .action-button.success {
       background-color: #4caf50;
     }
</style>
