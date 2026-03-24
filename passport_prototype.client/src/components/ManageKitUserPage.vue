<template>
  <div id="app-layout">
    <Header />

    <div class="main-body-wrapper">
      <LeftMenu :current="current" @navigate="onNavigate" />

      <div class="content-area">
        <div class="page-content">
          <h2 class="page-title">Manage Kit Users</h2>

          <div class="action-bar">
            <div class="search-group">
              <input type="text" v-model="searchTerm" placeholder="Search by username or ID..." class="search-input" />
              <i class="fas fa-search search-icon"></i>
            </div>
            <button class="add-btn" @click="openAddForm">+ Kit User</button>
          </div>

          <div class="table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Username</th>
                  <th>Kit Name</th>
                  <th>First Name</th>
                  <th>Last Name</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in filteredUsers" :key="user.id">
                  <td>{{ user.id }}</td>
                  <td>{{ user.username }}</td>
                  <td>{{ user.latestKitName }}</td>
                  <td>{{ user.firstName }}</td>
                  <td>{{ user.lastName }}</td>
                  <td>
                    <span :class="{'status-active': user.status, 'status-inactive': !user.status}">
                      {{ user.status ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="action-cell">
                    <button class="action-btn reset-btn" title="Reset Password" @click="resetPassword(user)">
                      <i class="fas fa-key"></i> Reset
                    </button>
                    <button class="action-btn status-btn" title="Toggle Status" @click="toggleUserStatus(user)">
                      <i :class="user.status ? 'fas fa-user-slash' : 'fas fa-user-check'"></i>
                      {{ user.status ? 'Deactivate' : 'Activate' }}
                    </button>
                  </td>
                </tr>
              </tbody>
              <tfoot v-if="!filteredUsers.length">
                <tr>
                  <td colspan="7" class="text-center p-4 text-gray-600">
                    {{ isLoading ? 'Loading...' : 'No Kit Users found on this page.' }}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>

          <div class="pagination-controls mt-6" v-if="users.length > 0 || currentPage > 1">
            <button @click="prevPage"
                    :disabled="currentPage === 1 || isLoading"
                    class="pagination-btn prev-btn">
              &larr; Previous
            </button>
            <span class="page-info">
              Page {{ currentPage }}
            </span>
            <button @click="nextPage"
                    :disabled="!hasNextPage || isLoading"
                    class="pagination-btn next-btn">
              Next &rarr;
            </button>
          </div>
          <div v-if="showForm" class="form-overlay" @click.self="closeForm">
            <div class="form-popup">
              <h3>{{ 'Create New User' }}</h3>
              <form @submit.prevent="saveUser">
                <label>Username</label>
                <input v-model="form.userName" required />
                <label>First Name</label>
                <input v-model="form.firstName" required />
                <label>Last Name</label>
                <input v-model="form.lastName" required />
                <p v-if="formValidation.username" class="validation-error">{{ formValidation.username }}</p>

                <div class="form-actions">
                  <button type="submit" class="save-btn">Save</button>
                  <button type="button" class="cancel-btn" @click="closeForm">Close</button>
                </div>
              </form>
            </div>
          </div>

          <div v-if="showConfirmation" class="form-overlay" @click.self="handleConfirmation(false)">
            <div class="form-popup">
              <h3>Confirm Action</h3>
              <p>{{ confirmationMessage }}</p>
              <div class="form-actions">
                <button type="button" class="cancel-btn" @click="handleConfirmation(false)">Cancel</button>
                <button type="button" class="save-btn confirm-action-btn" @click="handleConfirmation(true)">Confirm</button>
              </div>
            </div>
          </div>

          <DialogBox :show="showDialog" :title="dialogTitle" :message="dialogMessage" @close="showDialog = false" />

          <LoadingDialog :visible="isLoading" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, reactive, onMounted, computed, watch } from "vue";
  import { useAuthStore } from "@/stores/auth";

  import api from "@/api";
  // The components are imported here:
  import LeftMenu from "@/components/LeftMenu.vue";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "./LoadingDialog.vue";

  // --- Dialog & Loading ---
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);
  const searchTerm = ref("");

  // --- Pagination State (NEW) ---
  const currentPage = ref(1);
  const pageSize = ref(10);
  // NOTE: totalUsers is not used here but is kept for future API updates
  const totalUsers = ref(0);

  // Determine if there is a next page based on the number of users returned
  const hasNextPage = computed(() => {
    // If the number of users returned is equal to the requested page size,
    // there's potentially more data on the next page.
    return users.value.length === pageSize.value;
  });
  // --- End Pagination State ---

  // --- Confirmation Refs
  const showConfirmation = ref(false);
  const confirmationMessage = ref("");
  const confirmationAction = ref(null);

  // --- Auth store & user data ---
  const auth = useAuthStore();
  const currentUsername = computed(() => auth.Username ?? "User");
  const users = ref([]);

  // User creation
  const form = reactive({
    userType: 2,
    userName: "",
    firstName: "",
    lastName: "",
    userRole: 3, // Kit User Role
    password: "",
  });
  const showForm = ref(false);
  const formValidation = reactive({
    username: "",
  });

  // --- Navigation state ---
  const current = ref("admin");
  const onNavigate = (item) => {
    current.value = item;
  };

  // Filtered and Sorted users computed property
  const filteredUsers = computed(() => {
    let list = users.value;

    // 1. Filtering (only if search term exists)
    if (searchTerm.value) {
      const lowerCaseSearch = searchTerm.value.toLowerCase();
      list = list.filter(user =>
        String(user.id).includes(lowerCaseSearch) ||
        user.username.toLowerCase().includes(lowerCaseSearch) ||
        user.firstName?.toLowerCase().includes(lowerCaseSearch) ||
        user.lastName?.toLowerCase().includes(lowerCaseSearch)
      );
    }

    // 2. Sorting by ID (Numerical sort, ascending)
    // Use .slice() to create a copy of the array before sorting to avoid mutating the original data
    return list.slice().sort((a, b) => {
      // Convert IDs to numbers for proper numerical comparison (ascending)
      const idA = parseInt(a.id, 10);
      const idB = parseInt(b.id, 10);
      return idA - idB;
    });
  });

  // --- Core Data Loading Function (UPDATED) ---
  async function LoadSystemUsers() {
    try {
      isLoading.value = true;
      // Use the currentPage and pageSize in the API call
      const res = await api.get(`/admin/user-list/kit?page=${currentPage.value}&pageSize=${pageSize.value}`);
      users.value = res.data;

      // NOTE: Update totalUsers here if the API starts returning a total count.
    } catch (err) {
      console.error("AxiosError:", err);
      if (err.response) {
        console.error("Status:", err.response.status);
        console.error("Data:", err.response.data);
        if (err.response.status === 404 && currentPage.value > 1) {
          // Fallback: go to previous page and reload (or page 1 if current is 1)
          if (users.value.length === 0) {
            currentPage.value = Math.max(1, currentPage.value - 1);
            LoadSystemUsers(); // Reload the last valid page
            return; // Prevent the dialog from showing immediately
          }
        }
      } else if (err.request) {
        console.error("No response received from server", err.request);
      } else {
        console.error("Axios setup error:", err.message);
      }

      // Log out the user on critical error
      if (typeof auth.logout === "function") {
        auth.logout();
      } else {
        showDialog.value = true;
        dialogTitle.value = "Error";
        dialogMessage.value = "Failed to load kit users. Please log in again.";
      }
    } finally {
      isLoading.value = false;
    }
  }

  // --- Pagination Actions (NEW) ---
  function prevPage() {
    if (currentPage.value > 1) {
      currentPage.value--;
    }
  }

  function nextPage() {
    if (hasNextPage.value) {
      currentPage.value++;
    }
  }
  // --- End Pagination Actions ---

  // --- Lifecycle Hooks and Watchers (UPDATED) ---

  // Watch for changes in currentPage and reload data automatically
  watch(currentPage, () => {
    LoadSystemUsers();
  });

  // Initial load of users for page 1 on mount
  onMounted(async () => {
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
    LoadSystemUsers();
  });

  // --- User Creation Functions (Minimal change for userRole) ---
  async function openAddForm() {
    form.userName = "";
    form.firstName = "";
    form.lastName = "";
    form.userType = 2; // Kit User Type
    form.userRole = 3; // Kit User Role
    form.password = "";
    showForm.value = true;
  }

  async function saveUser() {
    try {
      if (!validateForm()) {
        return;
      }

      formValidation.username = "";
      formValidation.lastName = "";

      isLoading.value = true;
      const body = {
        userType: form.userType,
        userName: form.userName,
        firstName: form.firstName,
        lastName: form.lastName,
        userRole: form.userRole,
      };

      const res = await api.post("/admin/create-user", body);

      form.password = res.data.initialPassword;
      showForm.value = false;

      showDialog.value = true;
      dialogTitle.value = "Success";
      dialogMessage.value = `✅ User ${form.userName} created successfully with password: ${form.password}`;

    } catch (err) {
      console.error("AxiosError:", err);
      let isHandledError = false;

      if (err.response) {
        if (err.response.data && err.response.data.message === "Username already exists.") {
          formValidation.username = err.response.data.message;
          isHandledError = true;
        }
      }

      if (!isHandledError) {
        showDialog.value = true;
        dialogTitle.value = "Error";
        dialogMessage.value = "Failed to create user. Please check your inputs or network connection.";
      }

    } finally {
      isLoading.value = false;
      // Reset to page 1 and reload data after successful creation
      currentPage.value = 1;
      LoadSystemUsers();
    }
  }

  function validateForm() {
    let isValid = true;
    formValidation.username = "";
    formValidation.lastName = "";

    if (form.userName) {
      form.userName = form.userName.trimStart();
    }

    if (!form.userName || form.userName.length < 3) {
      formValidation.username = 'Username must be at least 3 characters long.';
      isValid = false;
    } else if (/\s/.test(form.userName)) {
      formValidation.username = 'Username cannot contain any spaces.';
      isValid = false;
    }

    if (form.lastName) {
      form.lastName = form.lastName.trimStart();
    }
    return isValid;
  }

  function closeForm() {
    showForm.value = false;
    formValidation.username = "";
  }

  // --- Confirmation Handles (Unchanged) ---
  function openConfirmation(message, action) {
    confirmationMessage.value = message;
    confirmationAction.value = action;
    showConfirmation.value = true;
  }

  async function handleConfirmation(confirmed) {
    showConfirmation.value = false;
    if (confirmed && typeof confirmationAction.value === 'function') {
      await confirmationAction.value();
    }
    confirmationAction.value = null;
  }

  // --- Toggle User Status Logic (Unchanged) ---
  function toggleUserStatus(user) {
    const newStatus = !user.status;
    const action = newStatus ? 'Activate' : 'Deactivate';

    openConfirmation(
      `Are you sure you want to ${action} user ${user.username}?`,
      () => executeToggleUserStatus(user, action)
    );
  }

  async function executeToggleUserStatus(user, action) {
    if (currentUsername.value == user.username && action === 'Deactivate') {
      showDialog.value = true;
      dialogTitle.value = "Error";
      dialogMessage.value = "You cannot deactivate your own account.";
      return;
    }

    try {
      isLoading.value = true;
      await api.post("/admin/toggle-status", {
        UserId: user.id,
      });

      showDialog.value = true;
      dialogTitle.value = "Success";
      dialogMessage.value = `User ${user.username} successfully ${action}d.`;

    } catch (err) {
      console.error("AxiosError:", err);
      showDialog.value = true;
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to toggle user status.";
    } finally {
      isLoading.value = false;
      // Reload the current page to reflect the status change
      LoadSystemUsers();
    }
  }

  // --- Reset Password Logic (Unchanged) ---
  function resetPassword(user) {
    openConfirmation(
      `Are you sure you want to reset the password for user ${user.username}?`,
      () => executeResetPassword(user)
    );
  }

  async function executeResetPassword(user) {
    let newPassword = null;

    try {
      isLoading.value = true;

      const res = await api.post("/admin/reset-user-password", {
        Username: user.username,
      });

      newPassword = res.data?.resetToken;

      if (!newPassword) {
        throw new Error("Temporary password field missing: API returned success but 'resetToken' was not found.");
      }

      showDialog.value = true;
      dialogTitle.value = "Password Reset Success";
      dialogMessage.value = `✅ New Password for user ${user.username}: ${newPassword}`;

    } catch (err) {
      console.error("Reset Password Failed:", err);

      showDialog.value = true;
      dialogTitle.value = "Error";

      let errorMessage = "Failed to reset password due to an unexpected error.";

      if (err.message.includes("Temporary password field missing")) {
        errorMessage = `Password was reset successfully on the server, but the temporary password field (**resetToken**) was missing from the response data.`;
      }
      else if (err.response && err.response.status === 400 && err.response.data && err.response.data.errors) {
        const errors = err.response.data.errors;
        const errorKeys = Object.keys(errors);
        if (errorKeys.length > 0) {
          errorMessage = `Validation Error: ${errorKeys[0]} - ${errors[errorKeys[0]][0]}`;
        }
      }

      dialogMessage.value = errorMessage;

    } finally {
      isLoading.value = false;
      LoadSystemUsers();
    }
  }
</script>

<style scoped>
  /* --- LAYOUT WRAPPER --- */
  /* --- LAYOUT CONSISTENCY --- */
  #app-layout {
    display: grid;
    /* 280px for Sidebar, 1fr for the rest of the screen */
    grid-template-columns: 280px 1fr;
    min-height: 100vh;
    background-color: #f4f7f9;
  }

  /* Header usually spans the whole top, but if it's inside the grid: */
  .header {
    grid-column: 1 / -1;
    height: 60px;
  }

  .main-body-wrapper {
    display: contents; /* Allows children to participate in the #app-layout grid */
  }

  .content-area {
    /* Remove margin-left: 250px; the grid handles this now */
    padding: 40px;
    background-color: #f4f7f9;
    overflow-y: auto;
  }

  /* Ensure the LeftMenu takes up the first column */
  .leftMenu {
    grid-column: 1;
    height: 100vh;
    position: sticky;
    top: 0;
  }

  /* --- TYPOGRAPHY & HEADER --- */
  .page-title {
    font-size: 2rem;
    color: #06195e;
    margin-bottom: 30px;
    font-weight: 800;
    letter-spacing: -0.025em;
  }

  /* --- ACTION BAR --- */
  .action-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    gap: 20px;
  }

  .search-group {
    position: relative;
    flex: 1;
    max-width: 400px;
  }

  .search-input {
    width: 100%;
    padding: 12px 12px 12px 45px;
    border: 1px solid #e2e8f0;
    border-radius: 12px;
    font-size: 0.95rem;
    background-color: white;
    transition: all 0.2s ease;
  }

    .search-input:focus {
      border-color: #4299e1;
      box-shadow: 0 0 0 3px rgba(66, 153, 225, 0.15);
      outline: none;
    }

  .search-icon {
    position: absolute;
    left: 16px;
    top: 50%;
    transform: translateY(-50%);
    color: #a0aec0;
  }

  .add-btn {
    background-color: #48bb78;
    color: white;
    border: none;
    padding: 12px 24px;
    border-radius: 12px;
    cursor: pointer;
    font-weight: 700;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.2s ease;
    box-shadow: 0 4px 6px -1px rgba(72, 187, 120, 0.2);
  }

    .add-btn:hover {
      background-color: #38a169;
      transform: translateY(-1px);
    }

  /* --- DATA TABLE CARD --- */
  .table-container {
    background: white;
    border-radius: 20px;
    border: 1px solid rgba(0, 0, 0, 0.05);
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
    overflow: hidden;
  }

  .data-table {
    width: 100%;
    border-collapse: collapse;
  }

    .data-table th {
      background-color: #f8fafc;
      color: #4a5568;
      font-weight: 700;
      text-transform: uppercase;
      font-size: 0.75rem;
      letter-spacing: 0.05em;
      padding: 16px 24px;
      text-align: left;
      border-bottom: 2px solid #edf2f7;
    }

    .data-table td {
      padding: 16px 24px;
      color: #2d3748;
      font-size: 0.9rem;
      border-bottom: 1px solid #edf2f7;
    }

    .data-table tbody tr:hover {
      background-color: #f7fafc;
    }

    /* Bold IDs and Usernames */
    .data-table td:nth-child(1) {
      font-weight: 700;
      color: #718096;
    }

    .data-table td:nth-child(2) {
      font-weight: 600;
      color: #06195e;
    }

  /* --- STATUS PILLS --- */
  .status-active, .status-inactive {
    padding: 6px 12px;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
  }

  .status-active {
    background-color: #c6f6d5;
    color: #22543d;
  }

  .status-inactive {
    background-color: #fed7d7;
    color: #822727;
  }

  /* --- ACTION BUTTONS --- */
  .action-cell {
    display: flex;
    gap: 8px;
  }

  .action-btn {
    padding: 8px 16px;
    border-radius: 8px;
    font-size: 0.8rem;
    font-weight: 700;
    cursor: pointer;
    border: none;
    transition: all 0.2s;
  }

  .reset-btn {
    background-color: #ebf8ff;
    color: #3182ce;
  }

    .reset-btn:hover {
      background-color: #bee3f8;
    }

  .status-btn {
    background-color: #fffaf0;
    color: #dd6b20;
  }

    .status-btn:hover {
      background-color: #feebc8;
    }

  /* --- PAGINATION --- */
  .pagination-controls {
    margin-top: 30px;
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 20px;
  }

  .pagination-btn {
    background-color: white;
    border: 1px solid #e2e8f0;
    color: #4a5568;
    padding: 8px 16px;
    border-radius: 10px;
    font-weight: 600;
    transition: all 0.2s;
  }

    .pagination-btn:hover:not(:disabled) {
      background-color: #f7fafc;
      border-color: #cbd5e0;
    }

    .pagination-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

  /* --- MODALS (POPUPS) --- */
  .form-popup {
    background: white;
    padding: 32px;
    border-radius: 24px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    width: 100%;
    max-width: 400px;
  }

    .form-popup h3 {
      font-size: 1.5rem;
      color: #06195e;
      margin-bottom: 20px;
      font-weight: 800;
    }

    .form-popup input {
      margin-bottom: 16px;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 12px;
    }

  .save-btn {
    background-color: #06195e;
    color: white;
    border-radius: 10px;
    padding: 12px 20px;
  }

  .cancel-btn {
    background-color: #f7fafc;
    color: #4a5568;
    border-radius: 10px;
    padding: 12px 20px;
  }
</style>
