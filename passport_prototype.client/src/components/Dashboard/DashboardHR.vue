<template>
  <div class="app-layout">
    <LeftMenu @navigate="onNavigate" class="leftMenu" />
    <Header class="header" />

    <div class="dashboard-content">
      <h1 class="welcome-title">HR Dashboard 📊</h1>
      <p class="role-info">
        Welcome back, {{ firstname }}. Here is a summary of all employee ID card progress statuses.
      </p>

      <div class="dashboard-grid">
        <template v-if="Object.keys(idStats).length">

          <div class="stat-card clickable" @click="goToDetails('all')">
            <div class="stat-icon-wrapper">👥</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.totalEmployees || 0 }}</div>
              <div class="stat-label">All Employees</div>
            </div>
          </div>

          <div class="stat-card stat-yellow clickable" @click="goToDetails('schedule')">
            <div class="stat-icon-wrapper">📅</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.forSchedule || 0 }}</div>
              <div class="stat-label">For Schedule</div>
            </div>
          </div>

          <div class="stat-card stat-blue clickable" @click="goToDetails('approval')">
            <div class="stat-icon-wrapper">⏳</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.scheduleRequests || 0 }}</div>
              <div class="stat-label">For Approval</div>
            </div>
          </div>

          <div class="stat-card stat-indigo clickable" @click="goToDetails('scheduled')">
            <div class="stat-icon-wrapper">📝</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.scheduled || 0 }}</div>
              <div class="stat-label">Scheduled</div>
            </div>
          </div>

          <div class="stat-card stat-green clickable" @click="goToDetails('captured')">
            <div class="stat-icon-wrapper">📸</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.capturedPendingAFIS || 0 }}</div>
              <div class="stat-label">Captured</div>
            </div>
          </div>

          <div class="stat-card stat-teal clickable" @click="goToDetails('adjudication')">
            <div class="stat-icon-wrapper">✅</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.pendingAdjudication || 0 }}</div>
              <div class="stat-label">Adjudication</div>
            </div>
          </div>

          <div class="stat-card stat-purple clickable" @click="goToDetails('for-printing')">
            <div class="stat-icon-wrapper">🖨️</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.readyForPrinting || 0 }}</div>
              <div class="stat-label">For Printing</div>
            </div>
          </div>

          <div class="stat-card stat-red clickable" @click="goToDetails('active-cards')">
            <div class="stat-icon-wrapper">🔓</div>
            <div class="stat-content">
              <div class="stat-value">{{ idStats.printed || 0 }}</div>
              <div class="stat-label">For Activation</div>
            </div>
          </div>

        </template>
        <template v-else>
          <p>Loading stats...</p>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from "vue";
  import { useRouter } from "vue-router";
  import LeftMenu from "@/components/LeftMenu.vue";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import { useAuthStore } from "@/stores/auth";
  import api from "@/api";

  // --- Dialog & Loading ---
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);

  // --- Router ---
  const router = useRouter();

  // --- Auth store & user data ---
  const auth = useAuthStore();
  const firstname = computed(() => {
    return auth.firstName || auth.firstname || auth.user?.firstName || auth.user?.firstname || "User";
  });
  const username = computed(() => auth.username ?? "User");
  const userType = computed(() => Number(auth.userType ?? 1));

  // --- Status stats ---
  const idStats = ref({
    totalEmployees: 0,
    forSchedule: 0,         // Status 0
    scheduleRequests: 0,    // Status 7
    scheduled: 0,           // Status 1
    captured: 0,            // Status 2
    pendingAdjudication: 0, // Status 3
    readyForPrinting: 0,    // Status 4
    forActivation: 0,       // Status 5 🆕
    activeCards: 0,         // Status 6 🆕
  });

  // --- Navigation state ---
  const current = ref("Dashboard");
  const onNavigate = (item) => {
    current.value = item;
  };

  // 🎯 UPDATED: Function to handle card click and redirection
  const goToDetails = (statusType) => {
    let targetTab = "";

    if (statusType === 'all') targetTab = "All Employees";
    if (statusType === 'schedule') targetTab = "Schedule Biometrics";
    if (statusType === 'approval') targetTab = "For Approval";
    if (statusType === 'scheduled') targetTab = "Scheduled";
    if (statusType === 'captured') targetTab = "Captured Biometrics";
    if (statusType === 'adjudication') targetTab = "Adjudication";
    if (statusType === 'validated') targetTab = "Generate Cards";
    if (statusType === 'for-printing') targetTab = "Generate Cards";
    if (statusType === 'active-cards') targetTab = "Activate Cards";

    if (!targetTab) targetTab = "All Employees";

    // Use the PATH that we know works in your LeftMenu
    router.push({
      path: "/manage-employee-ids",
      query: { tab: targetTab }
    });
  };

  // --- Load user + stats on mount ---
  onMounted(async () => {
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
    loadStats();
  });

  // --- Fetch ID stats ---
  async function loadStats() {
    try {
      isLoading.value = true;
      const res = await api.get("/hr/id-stats");
      console.log("API Response Stats:", res.data); // 🔍 Check your console with F12
      idStats.value = res.data;
    } catch (err) {
      console.error("AxiosError:", err);
      if (err.response) {
        console.error("Status:", err.response.status);
        console.error("Data:", err.response.data);
      } else if (err.request) {
        console.error("No response received from server", err.request);
      } else {
        console.error("Axios setup error:", err.message);
      }

      // Log out the user on error if there's no response data
      if (!err.response && typeof auth.logout === "function") {
        auth.logout(); // this should clear user data and redirect to login
      } else {
        showDialog.value = true;
        dialogTitle.value = "Error";
        dialogMessage.value = "Failed to load dashboard stats. Please log in again.";
      }
    } finally {
      isLoading.value = false;
    }
  }
</script>

<style scoped>
  @import "@/assets/css/dashboard.css";
</style>
