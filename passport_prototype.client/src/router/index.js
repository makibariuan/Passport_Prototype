// src/router.js
import { createRouter, createWebHistory } from "vue-router";
import AuthPage from "@/components/Auth/AuthPage.vue";
import Dashboard from "@/components/Dashboard.vue";
import PersonalDataSheet from "@/components/PersonalDataSheet.vue";
import Timesheet from "@/components/Timesheet.vue";
import EmailConfirm from "@/components/EmailConfirm.vue";
import ResetPassword from "@/components/ResetPassword.vue";
import DetailsPage from "@/components/EmployeeID.vue";
import DashboardAdmin from "@/components/Dashboard/DashboardAdmin.vue";
import DashboardHR from "@/components/Dashboard/DashboardHR.vue";
import EmployeeID from "@/components/EmployeeID.vue";

import DashboardUser from "@/components/Dashboard/DashboardUser.vue";

import { useAuthStore } from "@/stores/auth";

import ManageKitUserPage from "@/components/ManageKitUserPage.vue";
import ManageSystemUserPage from "@/components/ManageSystemUserPage.vue";
import ManageCitizenPage from "@/components/ManageCitizenPage.vue";

import ManageHRApplicationsPage from "@/components/HR/ManageHRApplicationsPage.vue";
import ManageEmployees from "@/components/ManageEmployees.vue";
import AdjudicationDetailsPage from "@/components/HR/AdjudicationDetailsPage.vue";

import BiometricTest from "@/components/BiometricTest.vue";

import AttendanceManagement from "@/components/HR/AttendanceManagement.vue";

// EMPLOYEE
import IDDetails from "@/components/Employee/IDDetails.vue";

/*
  userRole values:
    1 - Super Admin
    2 - System User
    3 - Kit User (Not included in this website)
    4 - Employee
    5 - Citizen
    6 - HR
*/

const routes = [
  { path: "/", component: PersonalDataSheet, name: "Login" },
  { path: "/login", redirect: "/" },
  { path: "/confirm-email", name: "ConfirmEmail", component: EmailConfirm },
  { path: "/reset-password", name: "ResetPassword", component: ResetPassword },
  { path: "/dashboard-system", component: Dashboard, meta: { requiresAuth: true, userRole: [2] } },
  { path: "/pds", component: PersonalDataSheet, meta: { requiresAuth: true, userRole: [4, 6] } },
  {
    path: "/timesheet",
    component: Timesheet,
    meta: { requiresAuth: true, userRole: [1, 2, 4, 6] },
  },
  {
    path: "/dashboard-admin",
    component: DashboardAdmin,
    meta: { requiresAuth: true, userRole: [1] },
  },
  //{ path: "/employee-id", component: EmployeeID, meta: { requiresAuth: true, userRole: [2, 3] } },
  {
    path: "/manage-employee-ids",
    component: DetailsPage,
    meta: { requiresAuth: true, userRole: [1, 6] },
  },
  {
    path: "/manage-kit-users",
    component: ManageKitUserPage,
    meta: { requiresAuth: true, userRole: [1] },
  },
  {
    path: "/manage-system-users",
    component: ManageSystemUserPage,
    meta: { requiresAuth: true, userRole: [1] },
  },
  {
    path: "/manage-citizens",
    component: ManageCitizenPage,
    meta: { requiresAuth: true, userRole: [1, 2] },
  },

  { path: "/dashboard", component: DashboardUser, meta: { requiresAuth: true, userRole: [4, 5] } },
  { path: "/dashboard-hr", component: DashboardHR, meta: { requiresAuth: true, userRole: [6] } },
  {
    path: "/adjudication-details",
    component: AdjudicationDetailsPage,
    meta: { requiresAuth: true, userRole: [6] },
  },

  { path: "/biometric-test", component: BiometricTest, name: "BiometricTest" },
  {
    path: "/attendance-management",
    component: AttendanceManagement,
    meta: { requiresAuth: true, userRole: [6] },
  },
  { path: "/id-details", component: IDDetails, meta: { requriesAuth: true, userRole: [4, 5, 6] } },

  { path: "/:pathMatch(.*)*", redirect: "/" },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

// router.beforeEach((to, from, next) => {
//   const auth = useAuthStore();

//   // ✅ Token expired → force logout
//   if (auth.isTokenExpired()) {
//     auth.idleLogoutAction();
//     if (to.meta.requiresAuth) return next("/login");
//   }

//   // ✅ Not logged in but route requires auth → redirect
//   if (to.meta.requiresAuth && !auth.isLoggedIn) {
//     return next("/login");
//   }

//   // ✅ Already logged in → block login/register routes
//   if ((to.path === "/" || to.path === "/login" || to.path === "/register") && auth.isLoggedIn) {
//     if (auth.userRole == 1) return next("/dashboard-admin");
//     else if (auth.userRole == 2) return next("/dashboard-system");
//     else if (auth.userRole == 4) return next("/dashboard");
//     else if (auth.userRole == 5) return next("/dashboard");
//     else if (auth.userRole == 6) return next("/dashboard-hr");
//     else return next("/login");
//   }

//   // ✅ Check user type restrictions
//   if (to.meta.requiresAuth && to.meta.userRole && auth.isLoggedIn) {
//     const userRole = parseInt(auth.userRole); // ensure numeric
//     const allowedUserTypes = to.meta.userRole.map((t) => parseInt(t)); // ensure numeric

//     if (!allowedUserTypes.includes(userRole)) {
//       console.warn(`🚫 Access denied for role ${userRole} on route ${to.path}`);
//       if (auth.userRole == 1) return next("/dashboard-admin");
//       else if (auth.userRole == 2) return next("/dashboard-system");
//       else if (auth.userRole == 5) return next("/dashboard");
//       else if (auth.userRole == 6) return next("/dashboard-hr");
//       else return next("/");
//     }
//   }

//   next();
// });

export default router;
