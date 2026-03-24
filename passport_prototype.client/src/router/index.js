// src/router.js
import { createRouter, createWebHistory } from "vue-router";
import AuthPage from "@/components/Auth/AuthPage.vue";
import Dashboard from "@/components/Dashboard.vue";
import PersonalDataSheet from "@/components/PersonalDataSheet.vue";
import PersonsDataSheet from "@/components/PersonsDataSheet.vue";
import EmailConfirm from "@/components/EmailConfirm.vue";
import ResetPassword from "@/components/ResetPassword.vue";
import DetailsPage from "@/components/EmployeeID.vue";
import DashboardAdmin from "@/components/Dashboard/DashboardAdmin.vue";
import DashboardHR from "@/components/Dashboard/DashboardHR.vue";
import DashboardUser from "@/components/Dashboard/DashboardUser.vue";
import ManageKitUserPage from "@/components/ManageKitUserPage.vue";
import ManageSystemUserPage from "@/components/ManageSystemUserPage.vue";
import ManageCitizenPage from "@/components/ManageCitizenPage.vue";
import AdjudicationDetailsPage from "@/components/HR/AdjudicationDetailsPage.vue";
import BiometricTest from "@/components/BiometricTest.vue";
import AttendanceManagement from "@/components/HR/AttendanceManagement.vue";
import IDDetails from "@/components/Employee/IDDetails.vue";
import CreateApplication from "@/components/CreateApplication.vue";

/*
  DEV MODE — auth guard disabled, navigate freely to any route.
  To re-enable auth, uncomment the beforeEach block at the bottom.

  userRole values:
    1 - Super Admin
    2 - System User
    3 - Kit User
    4 - Employee
    5 - Citizen
    6 - HR
*/

const routes = [
  // ── Public / Auth ──────────────────────────────────────────────
  { path: "/", name: "Login", component: AuthPage },
  { path: "/login", redirect: "/" },
  { path: "/confirm-email", name: "ConfirmEmail", component: EmailConfirm },
  { path: "/reset-password", name: "ResetPassword", component: ResetPassword },
  { path: "/biometric-test", name: "BiometricTest", component: BiometricTest },

  // ── Dashboards ─────────────────────────────────────────────────
  { path: "/dashboard", name: "DashboardUser", component: DashboardUser },
  { path: "/dashboard-admin", name: "DashboardAdmin", component: DashboardAdmin },
  { path: "/dashboard-hr", name: "DashboardHR", component: DashboardHR },
  { path: "/dashboard-system", name: "DashboardSystem", component: Dashboard },

  // ── Employee / PDS ─────────────────────────────────────────────
  { path: "/profile", name: "PersonalDataSheet", component: PersonalDataSheet },
  { path: "/persons-profile", name: "PersonsDataSheet", component: PersonsDataSheet },
  { path: "/application/new", name: "Application", component: CreateApplication },
  { path: "/id-details", name: "IDDetails", component: IDDetails },

  // ── Management ─────────────────────────────────────────────────
  { path: "/manage-employee-ids", name: "ManageEmployeeIDs", component: DetailsPage },
  { path: "/manage-kit-users", name: "ManageKitUsers", component: ManageKitUserPage },
  { path: "/manage-system-users", name: "ManageSystemUsers", component: ManageSystemUserPage },
  { path: "/manage-citizens", name: "ManageCitizens", component: ManageCitizenPage },

  // ── HR ─────────────────────────────────────────────────────────
  {
    path: "/adjudication-details",
    name: "AdjudicationDetails",
    component: AdjudicationDetailsPage,
  },
  { path: "/attendance-management", name: "AttendanceManagement", component: AttendanceManagement },

  // ── Catch-all ──────────────────────────────────────────────────
  { path: "/:pathMatch(.*)*", redirect: "/" },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// ── AUTH GUARD (disabled for dev — uncomment to re-enable) ────────
// router.beforeEach((to, from, next) => {
//   const auth = useAuthStore();
//
//   if (auth.isTokenExpired()) {
//     auth.idleLogoutAction();
//     if (to.meta.requiresAuth) return next("/");
//   }
//
//   if (to.meta.requiresAuth && !auth.isLoggedIn) {
//     return next("/");
//   }
//
//   if ((to.path === "/" || to.path === "/login") && auth.isLoggedIn) {
//     if      (auth.userRole == 1) return next("/dashboard-admin");
//     else if (auth.userRole == 2) return next("/dashboard-system");
//     else if (auth.userRole == 4) return next("/dashboard");
//     else if (auth.userRole == 5) return next("/dashboard");
//     else if (auth.userRole == 6) return next("/dashboard-hr");
//     else                         return next("/");
//   }
//
//   if (to.meta.requiresAuth && to.meta.userRole && auth.isLoggedIn) {
//     const userRole       = parseInt(auth.userRole);
//     const allowedRoles   = to.meta.userRole.map(Number);
//     if (!allowedRoles.includes(userRole)) {
//       if      (auth.userRole == 1) return next("/dashboard-admin");
//       else if (auth.userRole == 2) return next("/dashboard-system");
//       else if (auth.userRole == 5) return next("/dashboard");
//       else if (auth.userRole == 6) return next("/dashboard-hr");
//       else                         return next("/");
//     }
//   }
//
//   next();
// });

export default router;
