<template>
  <div class="app-layout">
    <LeftMenu @navigate="onNavigate" class="leftMenu" />
    <Header class="header" />

    <div class="dashboard-content">
      <div class="content-header">
        <h1 class="welcome-title">Administrator Dashboard 🛡️</h1>
        <p class="role-info">
          Welcome back, <strong>{{ username }}</strong>. System Overview and Personnel Management.
        </p>
      </div>

      <section class="summary-grid">
        <router-link to="/manage-kit-users" class="summary-card kit-user-card">
          <div class="card-body">
            <div class="card-icon-wrapper">
              <i class="fas fa-users-cog"></i>
            </div>
            <div class="card-metrics">
              <span class="card-label">Kit Users</span>
              <h2 class="metric-number">{{ statisticsData.kitUserCount || 0 }}</h2>
            </div>
          </div>
          <div class="card-footer-info">
            <span>Field Enrollment Personnel</span>
            <i class="fas fa-chevron-right"></i>
          </div>
        </router-link>

        <router-link to="/manage-system-users" class="summary-card system-user-card">
          <div class="card-body">
            <div class="card-icon-wrapper">
              <i class="fas fa-user-shield"></i>
            </div>
            <div class="card-metrics">
              <span class="card-label">System Users</span>
              <h2 class="metric-number">{{ statisticsData.systemUserCount || 0 }}</h2>
            </div>
          </div>
          <div class="card-footer-info">
            <span>Dashboard Administrators</span>
            <i class="fas fa-chevron-right"></i>
          </div>
        </router-link>

        <router-link to="/manage-citizens" class="summary-card citizen-card">
          <div class="card-body">
            <div class="card-icon-wrapper">
              <i class="fas fa-address-card"></i>
            </div>
            <div class="card-metrics">
              <span class="card-label">Citizen Records</span>
              <h2 class="metric-number">{{ statisticsData.citizenCount || 0 }}</h2>
            </div>
          </div>
          <div class="card-footer-info">
            <span>Citizen Database</span>
            <i class="fas fa-chevron-right"></i>
          </div>
        </router-link>
      </section>

      <LoadingDialog :visible="isLoading" />

      <DialogBox :show="showDialog"
                 :title="dialogTitle"
                 :message="dialogMessage"
                 @close="showDialog = false" />
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from "vue";
  import LeftMenu from "@/components/LeftMenu.vue";
  import { useAuthStore } from "@/stores/auth";
  import api from "@/api";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";

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
    // Only skip if we actually have data keys
    if (Object.keys(statisticsData.value).length > 0) return;

    try {
      isLoading.value = true;
      // Ensure the path matches your API structure: BASE_URL + /Admin/statistics
      const res = await api.get("/Admin/statistics");

      if (res.data) {
        statisticsData.value = res.data;
        console.log("Stats Loaded:", res.data);
      }
    } catch (err) {
      console.error("API Error:", err.response?.status, err.message);
      error.value = "Could not sync dashboard data.";
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
  /* --- LAYOUT CONSISTENCY --- */
  .app-layout {
    display: grid;
    grid-template-columns: 280px 1fr; /* Exact match to Citizen Dashboard */
    min-height: 100vh;
    background-color: #f4f7f9;
  }

  .dashboard-content {
    padding: 40px;
    box-sizing: border-box;
    font-family: 'Inter', sans-serif;
  }

  .welcome-title {
    font-size: 2.25rem;
    color: #06195e; /* VMS Official Navy */
    margin-bottom: 8px;
    font-weight: 800;
    letter-spacing: -0.025em;
  }

  .role-info {
    margin-bottom: 40px;
    color: #718096;
    font-size: 1.1rem;
  }

  /* --- ADMIN CARD GRID --- */
  .summary-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
    gap: 24px;
  }

  /* --- MODERN ADMIN CARDS --- */
  .summary-card {
    background: white;
    border-radius: 20px;
    display: flex;
    flex-direction: column;
    text-decoration: none;
    border: 1px solid rgba(0, 0, 0, 0.05);
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    overflow: hidden;
    border-left: 6px solid #e2e8f0; /* Consistent with Citizen Status Cards */
  }

    .summary-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    }

  .card-body {
    padding: 30px;
    display: flex;
    align-items: center;
    gap: 20px;
  }

  .card-icon-wrapper {
    width: 70px;
    height: 70px;
    border-radius: 16px;
    background: #f8fafc;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.8rem;
    transition: all 0.3s ease;
  }

  .card-metrics {
    display: flex;
    flex-direction: column;
  }

  .card-label {
    font-size: 0.85rem;
    font-weight: 700;
    color: #718096;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .metric-number {
    font-size: 2.8rem;
    font-weight: 800;
    color: #06195e;
    margin: 0;
    line-height: 1.1;
  }

  .card-footer-info {
    background: #f8fafc;
    padding: 15px 30px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.85rem;
    color: #a0aec0;
    font-weight: 600;
    border-top: 1px solid #edf2f7;
  }

  /* --- CARD-SPECIFIC ACCENTS --- */
  .kit-user-card {
    border-left-color: #ecc94b;
  }
    /* Yellow */
    .kit-user-card .card-icon-wrapper {
      color: #ecc94b;
      background: #fffdf2;
    }

  .system-user-card {
    border-left-color: #4299e1;
  }
    /* Blue */
    .system-user-card .card-icon-wrapper {
      color: #4299e1;
      background: #ebf8ff;
    }

  .citizen-card {
    border-left-color: #48bb78;
  }
    /* Green */
    .citizen-card .card-icon-wrapper {
      color: #48bb78;
      background: #f0fff4;
    }

  /* --- NOTIFICATIONS / OVERLAYS --- */
  .error-box {
    position: fixed;
    bottom: 30px;
    right: 30px;
    background: #fff5f5;
    color: #c53030;
    padding: 16px 24px;
    border-radius: 12px;
    border-left: 4px solid #c53030;
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    z-index: 1000;
  }

  /* --- RESPONSIVENESS --- */
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
  }
</style>
