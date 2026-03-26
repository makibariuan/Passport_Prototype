<template>
  <div class="left-menu">
    <div class="logo-section" @click="goTo('/dashboard')">
      <img src="../assets/dfa-logo.png" alt="DFA Logo" class="official-logo" />

      <div class="brand-text">
        <span class="gov-text">Passport Online Registration &</span>
        <span class="city-text">Application System</span>
      </div>
    </div>

    <nav class="nav-menu">
      <ul>
        <li
          v-for="item in navItems"
          :key="item.path"
          :class="{ active: isActive(item.path) }"
          @click="goTo(item.path)"
        >
          <component :is="item.icon" class="nav-icon" />
          <p>{{ item.label }}</p>
        </li>
      </ul>
    </nav>

    <div class="menu-footer">
      <div class="user-info">
        <Settings class="w-5 h-5 shrink-0" />
        <span class="username">
          {{ fullName }}
        </span>
      </div>
      <div class="logout-button" @click="logout">Logout</div>
    </div>
  </div>
</template>

<script setup>
import { useRouter, useRoute } from "vue-router";
import { User, Users, Calendar, FileSearch, BookText, CheckSquare, Settings } from "@lucide/vue";
import { useAuthStore } from "@/stores/auth";
import { ref, computed, onMounted } from "vue";
import axios from "axios";

const Auth = useAuthStore();

const user = ref({
  surname: "",
  firstName: "",
});

const fetchPersonal = async () => {
  try {
    const { data } = await axios.get(
      "https://localhost:5000/api/PassportPersonalInformations/My-Profile",
      {
        headers: {
          Authorization: `Bearer ${Auth.token}`,
        },
      },
    );

    user.value.firstName = data.firstName ?? "";
    user.value.surname = data.lastName ?? "";
  } catch (err) {
    console.log(err);
  }
};

const fullName = computed(() =>
  `${user.value.firstName} ${user.value.surname}`.trim().toUpperCase(),
);

const router = useRouter();
const route = useRoute();
const auth = useAuthStore();

const navItems = [
  { icon: User, label: "Personal Profile", path: "/profile" },
  { icon: Users, label: "Profiles", path: "/persons-profile" },
  { icon: Calendar, label: "Start an Application", path: "/application/new" },
  { icon: FileSearch, label: "Manage Existing Application", path: "/application/manage" },
  { icon: BookText, label: "Application History", path: "/application/history" },
  { icon: CheckSquare, label: "Privacy Policy", path: "/privacy-policy" },
];

const isActive = (path) => route.path === path;

const goTo = (path) => {
  if (path && route.path !== path) router.push(path);
};

const logout = () => {
  localStorage.removeItem("menu_open_states");
  auth.logout?.();
  router.push("/login");
};

onMounted(fetchPersonal);
</script>

<style scoped>
.left-menu {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: 280px;
  background: #06195e;
  display: flex;
  flex-direction: column;
  color: white;
  box-shadow: 4px 0 10px rgba(0, 0, 0, 0.2);
  z-index: 1000;
}

.logo-section {
  padding: 25px 20px;
  cursor: pointer;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  flex-direction: column;
  gap: 10px;
  flex-shrink: 0;
}

.official-logo {
  width: 55px;
  height: auto;
}

.brand-text {
  display: flex;
  flex-direction: column;
}

.gov-text {
  font-size: 0.65rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  opacity: 0.8;
  font-weight: 400;
  white-space: nowrap;
}

.city-text {
  font-size: 1.3rem;
  font-weight: 700;
  line-height: 1.2;
  white-space: nowrap;
}

.nav-menu {
  flex: 1;
  overflow-y: auto;
  padding: 10px 12px;
}

.nav-menu ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.nav-menu li {
  padding: 2px 5px;
  cursor: pointer;
  border-radius: 8px;
  transition: all 0.2s ease;
  font-size: 0.95rem;
  color: rgba(255, 255, 255, 0.75);
  display: flex;
  align-items: center;
  gap: 10px;
  white-space: nowrap;
  border-left: 3px solid transparent;
}

.nav-menu li:hover {
  background: #243e90;
  color: white;
  transform: translateX(4px);
}

.nav-menu li.active {
  background: #243e90;
  color: white;
  border-left-color: #60a5fa;
  font-weight: 600;
}

.nav-icon {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

.menu-footer {
  padding: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex-shrink: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 2px;
}

.username {
  font-weight: 600;
  font-size: 0.85rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.logout-button {
  padding: 12px;
  cursor: pointer;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.05);
  color: #ffbaba;
  border: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
  text-transform: uppercase;
  transition: all 0.2s ease;
}

.logout-button:hover {
  background: #c53030;
  color: white;
  border-color: #c53030;
}
</style>
