<template>
  <div class="header">
    <!-- The main logo div is clickable to go to /admin -->
    <div class="logo" @click="goTo('/admin')">
      <!-- Left side: Logo and Portal Title -->
      <div style="display: grid; grid-template-columns: auto 1fr; align-items: center; gap: 12px;">
        <img src="../assets/makati-logo.png" alt="Makati" />
        <div>
          <p>Makati Senior Citizen Portal</p>
        </div>
      </div>

      <!-- Middle: Application Title -->
      <p class="title">{{ title }}</p>

      <!-- Right side: Profile and Time -->
      <!-- CRITICAL FIX: @click.stop="" prevents clicks here from triggering the parent .logo's click event -->
      <div class="right-section" @click.stop="">
        <div class="profile-container">
          <div class="user-info">
            <p class="username">{{ username }}</p>
            <p class="pst-time">Philippine Standard Time: {{ phTime }}</p>
          </div>
          <div class="icon"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted } from "vue";
  import { useAuthStore } from "@/stores/auth";

  const auth = useAuthStore();
  const username = computed(() => auth.username ?? "User");

  const phTime = ref("Loading...");
  let syncTimer; // fetches every minute
  let tickTimer; // updates every second
  let serverTimeOffset = 0;

  // ✅ Fetch Manila time from worldtimeapi.org and set offset
  async function fetchPhTime() {
    try {
      const res = await fetch("https://worldtimeapi.org/api/timezone/Asia/Manila");
      const data = await res.json();

      if (data?.datetime) {
        const serverNow = new Date(data.datetime);
        const localNow = new Date();
        serverTimeOffset = serverNow - localNow; // difference in ms
      } else {
        phTime.value = "Unavailable";
      }
    } catch (err) {
      phTime.value = "Unavailable";
    }
  }

  // ✅ Update display every second based on last synced time
  function updateLocalClock() {
    const now = new Date(Date.now() + serverTimeOffset);
    phTime.value = now.toLocaleTimeString("en-PH", {
      timeZone: "Asia/Manila",
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: true,
    });
  }

  onMounted(() => {
    // Initial sync
    fetchPhTime().then(updateLocalClock);

    // Tick every second locally
    tickTimer = setInterval(updateLocalClock, 1000);

    // Sync with server every minute
    syncTimer = setInterval(fetchPhTime, 60000);
  });

  onUnmounted(() => {
    clearInterval(tickTimer);
    clearInterval(syncTimer);
  });

  defineProps({ title: String });
</script>

<style scoped>
   /*
  * HEADER STYLES - Fixed Full-Width Top Bar
  * The position: fixed property prevents the header from scrolling away entirely,
  * which is the correct property for a navigation bar that shouldn't disappear.
  */
   .header {
     position: fixed;
     top: 0;
     left: 0;
     width: 100vw;
     height: 85px;
     min-height: 85px;
     z-index: 110;
     background: #004085;
     color: white;
     box-shadow: none;
     border-bottom: 1px solid white;
     display: block;
     padding: 0;
   }

   /* 1. FORCE .logo TO BE THE MAIN FLEX CONTAINER */
   .logo {
     display: flex;
     justify-content: space-between;
     align-items: center;
     width: 100%;
     height: 100%;
     padding: 0 20px;
   }

     /* Targetting the first nested div (the Logo/Portal Title group) - LEFT SECTION */
     .logo > div:first-child {
       flex-shrink: 0;
       padding-right: 20px;
       border-right: 1px solid rgba(255, 255, 255, 0.2);
       cursor: pointer;
     }

       /* Targeting the Logo Image inside the Logo/Portal Title group */
       .logo > div:first-child img {
         width: 45px !important;
         height: 45px !important;
         object-fit: contain;
       }

       /* Targeting the Portal Title <p> tag */
       .logo > div:first-child p {
         margin: 0;
         font-size: 1.2rem;
         font-weight: 700;
         line-height: 1.3;
         color: white;
         font-family: 'Inter', sans-serif, Arial, Helvetica;
       }

   /* 2. MIDDLE SECTION: Application Title/Breadcrumb Styles */
   .title {
     font-family: 'Inter', sans-serif, Arial, Helvetica;
     font-size: 1.4em;
     font-weight: 700;
     color: white;
     flex-grow: 1;
     flex-shrink: 1;
     margin-left: 20px;
     min-width: 0;
     white-space: nowrap;
     overflow: hidden;
     text-overflow: ellipsis;
     cursor: pointer;
   }

   /* 3. RIGHT SECTION: Profile & Time Styles */
   .right-section {
     flex-shrink: 0;
     display: flex;
     align-items: center;
     margin-right: 40px;
     gap: 15px;
   }

   .profile-container {
     display: flex;
     align-items: center;
     gap: 8px;
   }

   .user-info {
     display: flex;
     flex-direction: column;
     align-items: flex-end;
     text-align: right;
   }

   .username {
     color: white;
     font-size: 0.9em;
     font-weight: 600;
     margin: 0;
     font-family: 'Inter', sans-serif, Arial, Helvetica;
   }

   .pst-time {
     color: #002a57;
     background: #f8d745;
     padding: 2px 5px;
     border-radius: 4px;
     font-size: 0.75em;
     font-weight: 700;
     margin: 0;
     font-family: 'Courier New', monospace;
     white-space: nowrap;
   }

   .icon {
     width: 32px;
     height: 32px;
     background-color: #002a57;
     border: 2px solid white;
     border-radius: 50%;
   }

   /* --- RESPONSIVENESS --- */
   @media (max-width: 768px) {
     .header {
       height: 60px;
       min-height: 60px;
     }
   }
</style>
