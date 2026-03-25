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

//const routes = [
//  { path: "/", component: AuthPage, name: "Login" },
//  { path: "/login", redirect: "/" },
//  { path: "/confirm-email", name: "ConfirmEmail", component: EmailConfirm },
//  { path: "/reset-password", name: "ResetPassword", component: ResetPassword },
//  { path: "/dashboard-system", component: Dashboard, meta: { requiresAuth: true, userRole: [2] } },
//  { path: "/pds", component: PersonalDataSheet, meta: { requiresAuth: true, userRole: [4, 6] } },
//  { path: "/timesheet", component: Timesheet, meta: { requiresAuth: true, userRole: [1, 2, 4, 6] } },
//  { path: "/dashboard-admin", component: DashboardAdmin, meta: { requiresAuth: true, userRole: [1] } },
//  //{ path: "/employee-id", component: EmployeeID, meta: { requiresAuth: true, userRole: [2, 3] } },
//  { path: "/manage-employee-ids", component: DetailsPage, meta: { requiresAuth: true, userRole: [1, 6] } },
//  { path: "/manage-kit-users", component: ManageKitUserPage, meta: { requiresAuth: true, userRole: [1] } },
//  { path: "/manage-system-users", component: ManageSystemUserPage, meta: { requiresAuth: true, userRole: [1] } },
//  { path: "/manage-citizens", component: ManageCitizenPage, meta: { requiresAuth: true, userRole: [1, 2] } },

//  { path: "/dashboard", component: DashboardUser, meta: { requiresAuth: true, userRole: [4, 5] } },
//  { path: "/dashboard-hr", component: DashboardHR, meta: { requiresAuth: true, userRole: [6] } },
//  { path: "/adjudication-details", component: AdjudicationDetailsPage, meta: { requiresAuth: true, userRole: [6] } },

//  { path: "/biometric-test", component: BiometricTest, name: "BiometricTest" },
//  { path: "/attendance-management", component: AttendanceManagement, meta: { requiresAuth: true, userRole: [6] } },
//  { path: "/id-details", component: IDDetails, meta: { requriesAuth: true, userRole: [4, 5, 6] } },

//  { path: "/:pathMatch(.*)*", redirect: "/" }
//];

const routes = [
  // ── Public / Auth ──────────────────────────────────────────────
  { path: "/", name: "Login", component: DashboardAdmin },
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
