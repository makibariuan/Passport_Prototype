<template>
  <div id="app">
    <TopHeader />

    <main :class="['main-content', { 'with-header': auth.isLoggedIn }]">
      <router-view />
    </main>

    <div v-if="showDialog" class="dialog-overlay">
      <div class="dialog-box">
        <h3>{{ dialogTitle }}</h3>
        <p>{{ dialogMessage }}</p>
        <button @click="closeDialog" class="dialog-btn">OK</button>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, onBeforeUnmount, watch } from "vue";
  import { useRouter } from "vue-router";
  import { useAuthStore } from "@/stores/auth";

  import TopHeader from "@/components/TopHeader.vue";

  const router = useRouter();
  const auth = useAuthStore();

  // --- Idle timer setup ---
  const idleTimeout = ref(null);
  const idleLimit = 15 * 60 * 1000; // 15 minutes

  // Dialog state
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");

  // --- Reset idle timer ---
  const resetIdleTimer = () => {
    if (idleTimeout.value) clearTimeout(idleTimeout.value);
    idleTimeout.value = setTimeout(() => showIdleDialog(), idleLimit);
  };

  // --- Show idle dialog ---
  const showIdleDialog = () => {
    if (!auth.isLoggedIn) return;

    showDialog.value = true;
    dialogTitle.value = "Session Expired";
    dialogMessage.value =
      "You have been logged out due to inactivity or token expiration.";
  };

  // --- Close dialog and logout ---
  const closeDialog = () => {
    showDialog.value = false;
    auth.idleLogoutAction();
    router.push("/login");
  };

  // --- Activity listeners ---
  const addActivityListeners = () => {
    ["mousemove", "keydown", "click"].forEach((evt) =>
      window.addEventListener(evt, resetIdleTimer)
    );
  };

  const removeActivityListeners = () => {
    ["mousemove", "keydown", "click"].forEach((evt) =>
      window.removeEventListener(evt, resetIdleTimer)
    );
  };

  // --- Watch login state to start/stop idle timer ---
  watch(
    () => auth.isLoggedIn,
    (loggedIn) => {
      if (loggedIn) {
        resetIdleTimer();
        addActivityListeners();
      } else {
        if (idleTimeout.value) clearTimeout(idleTimeout.value);
        removeActivityListeners();
      }
    },
    { immediate: true }
  );

  // --- On mount, check token and setup timers ---
  onMounted(() => {
    if (auth.token) {
      if (!auth.checkSession()) {
        showDialog.value = true;
        dialogTitle.value = "Session Expired";
        dialogMessage.value = "Your session expired. Please log in again.";
        router.push("/login");
        return;
      }

      // Token valid → setup timers
      auth.startAutoLogoutTimer();
      resetIdleTimer();
      addActivityListeners();
    }
  });

  // --- Cleanup ---
  onBeforeUnmount(() => {
    if (idleTimeout.value) clearTimeout(idleTimeout.value);
    removeActivityListeners();
  });
</script>

<style>
  /* 1. Global Reset & Modern Variables */
  :root {
    --primary-red: #a52a2a;
    --dark-red: #800000;
    --bg-gray: #f4f7f9; /* Slightly lighter, cleaner gray than #e1e6ea */
    --text-main: #2d3748;
    --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
  }

  html, body {
    height: 100%;
    margin: 0;
    padding: 0;
    /* Using a cleaner system font stack like VMS */
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    color: var(--text-main);
    background: var(--bg-gray);
    -webkit-font-smoothing: antialiased;
  }

  #app {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
    width: 100%;
  }

  .main-content {
    flex-grow: 1;
    width: 100%;
    /* Standardized background for all pages */
    background: var(--bg-gray);
  }

  /* 2. Modernized Dialog Overlay */
  .dialog-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5); /* Slightly darker for better focus */
    backdrop-filter: blur(2px); /* Soft blur effect like high-end apps */
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    padding: 20px;
  }

  /* 3. VMS-Style Dialog Box */
  .dialog-box {
    background: white;
    padding: 30px;
    border-radius: 16px; /* Rounder corners like VMS */
    width: 100%;
    max-width: 400px;
    text-align: center;
    box-shadow: var(--shadow-lg);
    border: 1px solid rgba(0,0,0,0.05);
    animation: dialogAppear 0.3s ease-out;
  }

  @keyframes dialogAppear {
    from {
      opacity: 0;
      transform: translateY(20px);
    }

    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .dialog-box h3 {
    font-size: 1.5rem;
    font-weight: 700;
    margin: 0 0 12px 0;
    color: #1a202c; /* Neutral dark for headings */
  }

  /* Specifically style the "Expired" title to stand out */
  .dialog-box h3 {
    color: var(--primary-red);
  }

  .dialog-box p {
    font-size: 1rem;
    line-height: 1.5;
    color: #4a5568;
    margin-bottom: 24px;
  }

  /* 4. Polished Button Style */
  .dialog-btn {
    width: 100%; /* Full width button for mobile-friendly touch */
    padding: 12px 24px;
    background: var(--primary-red);
    border: none;
    color: white;
    border-radius: 10px;
    cursor: pointer;
    font-size: 1rem;
    font-weight: 600;
    letter-spacing: 0.025em;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 4px 6px rgba(165, 42, 42, 0.2);
  }

    .dialog-btn:hover {
      background: var(--dark-red);
      transform: translateY(-2px);
      box-shadow: 0 6px 12px rgba(165, 42, 42, 0.3);
    }

    .dialog-btn:active {
      transform: translateY(0);
    }
</style>
