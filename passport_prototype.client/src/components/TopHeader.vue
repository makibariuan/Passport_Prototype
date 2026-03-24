<template>
  <header :class="['top-header', { 'full-width': !auth.isLoggedIn }]">
    <div class="header-right">
      <div class="pst-container">
        <span class="pst-label">Philippine Standard Time</span>
        <span class="pst-time">{{ phTime }}</span>
        <span class="pst-date">{{ phDate }}</span>
      </div>
    </div>
  </header>
</template>

<script setup>
  import { ref, onMounted, onUnmounted } from "vue";
  import { useAuthStore } from "@/stores/auth";

  const auth = useAuthStore();

  // --- PST TIME LOGIC ---
  const phTime = ref("Loading...");
  const phDate = ref("");
  let syncTimer, tickTimer, serverTimeOffset = 0;

  async function fetchPhTime() {
    try {
      const res = await fetch("https://worldtimeapi.org/api/timezone/Asia/Manila");
      if (!res.ok) throw new Error('API busy');
      const data = await res.json();
      if (data?.datetime) {
        const serverNow = new Date(data.datetime);
        const localNow = new Date();
        serverTimeOffset = serverNow - localNow;
      }
    } catch (err) {
      console.warn("Header PST Sync failed.");
    }
  }

  function updateLocalClock() {
    const now = new Date(Date.now() + serverTimeOffset);

    phDate.value = now.toLocaleDateString("en-PH", {
      timeZone: "Asia/Manila",
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });

    phTime.value = now.toLocaleTimeString("en-PH", {
      timeZone: "Asia/Manila",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: true,
    });
  }

  onMounted(() => {
    fetchPhTime().then(updateLocalClock);
    tickTimer = setInterval(updateLocalClock, 1000);
    syncTimer = setInterval(fetchPhTime, 60000);
  });

  onUnmounted(() => {
    clearInterval(tickTimer);
    clearInterval(syncTimer);
  });
</script>

<style scoped>
  .top-header {
    position: fixed;
    top: 0;
    left: 280px;
    right: 0;
    height: 90px; /* Increased height for better vertical spacing */
    display: flex;
    align-items: center;
    justify-content: flex-end;
    padding: 0 40px; /* Wider side padding for a more "global" feel */
    z-index: 999;
    transition: all 0.3s ease;
    /* Optional: If you want it truly "one with the page", keep background transparent */
    background: transparent;
  }

    .top-header.full-width {
      left: 0;
    }

  .pst-container {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    text-align: right;
    /* Gap adds consistent space between the three rows */
    gap: 1px;
  }

  .pst-label {
    font-size: 0.65rem;
    font-weight: 700;
    color: #94a3b8; /* Lighter slate for the label */
    text-transform: uppercase;
    letter-spacing: 1.2px; /* Increased tracking for a premium feel */
    margin-bottom: 2px;
  }

  .pst-date {
    font-size: 0.9rem;
    font-weight: 500;
    color: #475569; /* Slate-600 */
    letter-spacing: 0.2px;
  }

  .pst-time {
    font-family: 'Courier New', monospace;
    font-size: 1.3rem; /* Slightly larger time */
    font-weight: 800;
    color: #06195e; /* Makati Brand Blue */
    line-height: 1;
    margin-top: 2px;
  }
</style>
