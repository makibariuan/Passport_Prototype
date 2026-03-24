<template>
  <div class="left-menu">
    <div class="logo-section" @click="goTo('/dashboard')">
      <div class="branding-top">
        <img src="../assets/makati-logo.png" alt="Makati Logo" class="official-logo" />
        <div class="brand-text">
          <span class="gov-text">Republic of the Philippines</span>
          <span class="city-text">City of Makati</span>
        </div>
      </div>
    </div>

    <div class="nav-container">
      <ul>
        <template v-for="item in menuItems" :key="item.name">
          <template v-if="item.children">
            <li class="menu-item dropdown-toggle"
                :class="{ active: isChildActive(item) }"
                @click="toggleDropdown(item.name)">
              <span class="menu-text">{{ item.name }}</span>
              <span class="arrow" :class="{ rotated: openDropdowns[item.name] }">▼</span>
            </li>

            <transition name="slide">
              <ul v-if="openDropdowns[item.name]" class="sub-menu">
                <li v-for="child in item.children"
                    :key="child.name"
                    class="sub-menu-item"
                    :class="{ active: $route.path === child.path }"
                    @click.stop="goTo(child.path)">
                  {{ child.name }}
                </li>
              </ul>
            </transition>
          </template>

          <li v-else
              class="menu-item"
              :class="{ active: $route.path === item.path }"
              @click="goTo(item.path)">
            {{ item.name }}
          </li>
        </template>
      </ul>
    </div>

    <div class="menu-footer">
      <div class="logout-button" @click="logout">Logout</div>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, onUnmounted, watch } from "vue";
  import { useAuthStore } from "@/stores/auth";
  import { useRouter, useRoute } from "vue-router";

  const auth = useAuthStore();
  const router = useRouter();
  const route = useRoute();

  // --- PST TIME LOGIC ---
  const phTime = ref("Loading...");
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
      console.warn("PST Sync failed, using local backup.");
    }
  }

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

  // --- MENU DATA ---
  const allMenus = {
    1: [ // Super Admin
      {
        name: "Manage Users",
        children: [
          { name: "Kit Users", path: "/manage-kit-users" },
          { name: "System Users", path: "/manage-system-users" },
          { name: "Citizens", path: "/manage-citizens" },
        ]
      },
      { name: "Details", path: "/details" },
      { name: "Human Resources", path: "/hr" },
      { name: "Announcements", path: "" },
      { name: "Settings", path: "" },
    ],
    5: [ // Citizen
      { name: "Dashboard", path: "/dashboard" },
      { name: "ID Details", path: "/id-details" },
      //{ name: "Personal Data Sheet", path: "/pds" },
      { name: "Details", path: "/details" },
      //{ name: "Timesheet", path: "/timesheet" },
      //{ name: "Payslip and Compensation", path: "" },
      { name: "Announcements", path: "" },
      { name: "Settings", path: "" },
    ],
    6: [ // HR
      { name: "Dashboard", path: "/dashboard-hr" },
      { name: "ID Details", path: "/id-details" },
      { name: "Personal Data Sheet", path: "/pds" },
      {
        name: "Manage Employees",
        children: [
          { name: "Employee IDs", path: "/manage-employee-ids" },
          { name: "Attendance Management", path: "/attendance-management" },
        ]
      },
      { name: "Timesheet", path: "/timesheet" },
      { name: "Announcements", path: "" },
      { name: "Settings", path: "" },
    ],
    4: [ // Employee
      { name: "Dashboard", path: "/dashboard" },
      { name: "ID Details", path: "/id-details" },
      { name: "Personal Data Sheet", path: "/pds" },
      //{ name: "Details", path: "/details" },
      { name: "Timesheet", path: "/timesheet" },
      { name: "Payslip and Compensation", path: "" },
      { name: "Announcements", path: "" },
      { name: "Settings", path: "" },
    ],
  };

  // --- MENU LOGIC ---

  // 1. Initialize from localStorage so it's "sticky"
  const getSavedState = () => {
    const saved = localStorage.getItem("menu_open_states");
    return saved ? JSON.parse(saved) : {};
  };

  const openDropdowns = ref(getSavedState());

  // 2. Computed menu
  const menuItems = computed(() => {
    const role = auth.userRole ? parseInt(auth.userRole) : -1;
    return allMenus[role] || [];
  });

  // 3. Helper Functions
  const isChildActive = (parent) => {
    return parent.children?.some(child => child.path === route.path);
  };

  // Toggle manually and save to localStorage
  const toggleDropdown = (name) => {
    openDropdowns.value[name] = !openDropdowns.value[name];
    localStorage.setItem("menu_open_states", JSON.stringify(openDropdowns.value));
  };

  // 4. THE PERSISTENT WATCHER
  watch(() => route.path, (newPath) => {
    if (!menuItems.value) return;

    let stateChanged = false;
    menuItems.value.forEach(item => {
      if (item.children) {
        const isInsideThisDropdown = item.children.some(child => child.path === newPath);

        // If we are on a child page, ensure parent is open
        if (isInsideThisDropdown && !openDropdowns.value[item.name]) {
          openDropdowns.value[item.name] = true;
          stateChanged = true;
        }
      }
    });

    if (stateChanged) {
      localStorage.setItem("menu_open_states", JSON.stringify(openDropdowns.value));
    }
  }, { immediate: true });

  // --- LIFECYCLE & ACTIONS ---
  onMounted(async () => {
    fetchPhTime().then(updateLocalClock);
    tickTimer = setInterval(updateLocalClock, 1000);
    syncTimer = setInterval(fetchPhTime, 60000);

    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
  });

  onUnmounted(() => {
    clearInterval(tickTimer);
    clearInterval(syncTimer);
  });

  const goTo = (path) => {
    if (path && route.path !== path) router.push(path);
  };

  const logout = () => {
    localStorage.removeItem("menu_open_states"); // Clear menu memory
    auth.logout();
    router.push("/login");
  };
</script>

<style scoped>
  .left-menu {
    position: fixed;
    top: 0;
    left: 0;
    height: 100%;
    width: 280px;
    background: #06195e;
    display: flex;
    flex-direction: column;
    color: white;
    box-shadow: 4px 0 10px rgba(0, 0, 0, 0.2);
    z-index: 1000;
  }

  .nav-container {
    flex-grow: 1; /* Takes up all available middle space */
    overflow-y: auto; /* Allows scrolling if the menu is long */
  }

  /* --- BRANDING & PST STYLES --- */
  .logo-section {
    padding: 25px 20px; /* Balanced padding */
    cursor: pointer;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  /* TOP ROW: Back to original large scale */
  .branding-top {
    display: flex;
    align-items: center; /* This vertically centers the logo relative to the text block */
    gap: 15px;
  }

  .official-logo {
    width: 55px; /* Restored to original large size */
    height: auto;
    flex-shrink: 0;
  }

  .brand-text {
    display: flex;
    flex-direction: column;
    justify-content: center;
  }

  .gov-text {
    font-size: 0.65rem; /* Original size */
    text-transform: uppercase;
    letter-spacing: 0.5px;
    opacity: 0.8;
    font-weight: 400;
    white-space: nowrap;
  }

  .city-text {
    font-size: 1.3rem; /* Original bold size */
    font-weight: 700;
    line-height: 1.2;
    margin-bottom: 4px;
    white-space: nowrap;
  }

  /* --- MENU ITEMS --- */
  .left-menu ul {
    list-style: none;
    padding: 10px 12px;
    margin: 0;
    display: flex;
    flex-direction: column;
  }

  .left-menu li {
    padding: 12px 18px;
    margin-bottom: 5px;
    cursor: pointer;
    border-radius: 8px;
    transition: all 0.2s ease;
    font-size: 0.95rem;
    color: rgba(255, 255, 255, 0.8);
    display: flex;
    align-items: center;
  }

    .left-menu li:hover {
      background: #243E90;
      color: white;
      transform: translateX(5px);
    }

    .left-menu li.active {
      background: #3D539D;
      color: white;
      font-weight: 600;
      box-shadow: inset 4px 0 0 0 #f8d745;
    }

  .logout-button {
    padding: 14px;
    cursor: pointer;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.05);
    color: #ffbaba;
    border: 1px solid rgba(255, 255, 255, 0.1);
    display: flex;
    justify-content: center;
    font-weight: 700;
    text-transform: uppercase;
    transition: all 0.2s ease;
  }

    .logout-button:hover {
      background: #c53030;
      color: white;
    }

  .menu-footer {
    padding: 10px 18px 25px 18px; /* Keeps padding consistent with menu items */
    border-top: 1px solid rgba(255, 255, 255, 0.1);
  }

  /* --- DROPDOWN ANIMATION --- */
  .expand-enter-active,
  .expand-leave-active {
    transition: all 0.3s ease-in-out;
    overflow: hidden;
    max-height: 300px; /* High enough to cover all sub-menu items */
  }

  .expand-enter-from,
  .expand-leave-to {
    max-height: 0;
    opacity: 0;
    transform: translateY(-10px);
  }

  /* Dropdown specific */
  .dropdown-toggle {
    justify-content: space-between;
  }

  /* --- REFINED ARROW TRANSITION --- */
  .arrow {
    font-size: 0.7rem;
    transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    opacity: 0.6;
  }

    .arrow.rotated {
      transform: rotate(180deg);
    }

  /* --- SLIDE ANIMATION --- */
  .slide-enter-active,
  .slide-leave-active {
    transition: all 0.35s cubic-bezier(0.4, 0, 0.2, 1);
    overflow: hidden;
    max-height: 400px; /* Adjust if your sub-menus are extremely long */
  }

  .slide-enter-from,
  .slide-leave-to {
    max-height: 0;
    opacity: 0;
    transform: translateY(-8px); /* Subtle lift as it disappears */
  }

  /* Ensure the sub-menu doesn't have vertical padding that breaks the animation */
  .sub-menu {
    padding: 0 0 0 15px !important;
    margin: 0 !important;
    border-left: 1px solid rgba(255, 255, 255, 0.1);
    list-style: none;
  }

  /* Move the spacing to the items instead */
  .sub-menu-item {
    padding: 12px 18px !important;
    margin-top: 2px;
    font-size: 0.85rem !important;
    transition: background 0.5s ease, color 0.5s ease;
  }

    .sub-menu-item.active {
      color: #f8d745 !important;
      font-weight: 700 !important;
    }
</style>
