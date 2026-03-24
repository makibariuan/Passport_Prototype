<template>
  <div class="left-menu">
    <!--<div class="logo" @click="goTo('/admin')">
      <div style="display: grid; grid-template-columns: auto 1fr; align-items: center; gap: 12px;">
        <img src="../assets/makati-logo.png" alt="Makati" />
        <div>
          <p>Makati Senior Citizen Portal</p>
        </div>
      </div>
    </div>-->

    <ul>

      <!-- Main Navigation Items -->
      <li v-for="item in menuItems"
          :key="item.name"
          class="menu-item"
          :class="{ active: $route.path === item.path }"
          @click="handleMenuClick(item)">
        {{ item.name }}
      </li>

      <!-- The "Go Back" button is now always visible at the top of the list -->
      <li class="menu-item back-btn"
          @click="router.back()">
          Go Back
      </li>

      <li class="logout" @click="logout">Logout</li>
    </ul>
  </div>
</template>

<script setup>
  import { computed, onMounted, watch } from "vue";
  import { useAuthStore } from "@/stores/auth";
  import { useRouter, useRoute } from "vue-router";

  const auth = useAuthStore();
  const router = useRouter();
  const route = useRoute();

  // 🧩 Menu configuration per user type
  const allMenus = {
    1: [ // Super Admin
      { name: "Manage Kit Users", path: "/manage-kit-users" },
      { name: "Manage System Users", path: "/manage-system-users" },
      { name: "Manage Citizens", path: "/manage-citizens" },
    ],
    2: [ // System Admin
      { name: "Manage Citizens", path: "/manage-citizens" },
    ],
  };

  // Reactive menu
  const menuItems = computed(() => {
    const type = parseInt(auth.userRole ?? -1);
    return allMenus[type] || [];
  });

  // Handle menu item click
  const handleMenuClick = (item) => {
    // If menu has a custom action (like Go Back), run it
    if (item.action && typeof item.action === "function") {
      item.action();
      return;
    }

    // Otherwise, navigate by path
    if (item.path && route.path !== item.path) {
      router.push(item.path);
    }
  };

  // Logout
  const logout = () => {
    auth.logout();
    router.push("/login");
  };

  // Load user info (optional)
  onMounted(async () => {
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
  });
</script>

<style scoped>
  .left-menu {
    position: fixed;
    top: 85px; /* Match the new header height */
    left: 0;
    height: calc(100% - 85px);
    width: 250px;
    background: #0c2884; /* Slightly deeper, more professional blue */
    display: flex;
    flex-direction: column;
    color: white;
    box-shadow: 4px 0 15px rgba(0, 0, 0, 0.05); /* Softer shadow */
    z-index: 100;
    border-right: 1px solid rgba(255, 255, 255, 0.05);
  }

  .left-menu ul {
    list-style: none;
    padding: 30px 15px; /* More breathing room */
    margin: 0;
    flex: 1;
    display: flex;
    flex-direction: column;
  }

  .left-menu li {
    padding: 14px 18px;
    margin-bottom: 8px;
    cursor: pointer;
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    border-radius: 10px; /* Modern rounded corners */
    font-size: 0.9rem;
    font-weight: 500;
    display: flex;
    align-items: center;
    color: rgba(255, 255, 255, 0.8);
  }

  .left-menu li:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
    transform: translateX(5px);
  }

  .left-menu li.active {
    background: rgba(248, 215, 69, 0.15); /* Soft yellow glow */
    color: #f8d745;
    font-weight: 700;
    border-left: 4px solid #f8d745;
    padding-left: 14px; /* Adjust for border */
  }

  .left-menu li.back-btn {
    color: #f8d745;
    border: 1px solid rgba(248, 215, 69, 0.3);
    margin-bottom: 25px;
    justify-content: center; /* Center "Go Back" text */
    background: rgba(248, 215, 69, 0.05);
  }

  .left-menu li.back-btn:hover {
    background: #f8d745;
    color: #003366;
  }

  .left-menu li.logout {
    margin-top: auto;
    background: rgba(230, 57, 70, 0.1);
    color: #E63946;
    border: 1px solid rgba(230, 57, 70, 0.2);
    font-weight: 700;
    text-align: center;
    justify-content: center;
  }

  .left-menu li.logout:hover {
    background: #E63946;
    color: white;
    transform: none;
  }
</style>
