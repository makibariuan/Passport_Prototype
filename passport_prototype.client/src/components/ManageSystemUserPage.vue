<template>
  <div id="app-layout">
    <Header />

    <div class="main-body-wrapper">
      <LeftMenu :current="current" @navigate="onNavigate" />

      <div class="content-area">
        <div class="page-content">
          <h2 class="page-title">Manage System Users</h2>

          <div class="action-bar">
            <div class="search-group">
              <input type="text" v-model="searchTerm" placeholder="Search by username or ID..." class="search-input" />
              <i class="fas fa-search search-icon"></i>
            </div>
            <!--<button class="add-btn" @click="openAddForm">+ System User</button>-->
          </div>

          <div class="table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Username</th>
                  <th>First Name</th>
                  <th>Last Name</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in filteredUsers" :key="user.id">
                  <td>{{ user.id }}</td>
                  <td>{{ user.username }}</td>
                  <td>{{ user.firstName }}</td>
                  <td>{{ user.lastName }}</td>
                  <td>{{ user.roleDesc }}</td> <!-- Display Role Description -->
                  <td>
                    <span :class="{'status-active': user.status, 'status-inactive': !user.status}">
                      {{ user.status ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td class="action-cell">
                    <button class="action-btn reset-btn" @click="resetPassword(user)">Reset</button>
                    <button class="action-btn status-btn" @click="toggleUserStatus(user)">
                      {{ user.status ? 'Deactivate' : 'Activate' }}
                    </button>
                  </td>
                </tr>
              </tbody>
              <tfoot v-if="!filteredUsers.length">
                <tr>
                  <td colspan="7" class="text-center p-4 text-gray-600">
                    {{ isLoading ? 'Loading...' : 'No System Users found on this page.' }}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>

          <!-- Pagination Controls -->
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

          <!-- Modals (Form, Confirmation, Dialogs) remain here -->
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

                <label>Role</label>
                <select v-model.number="form.userRole" required>
                  <option :value="1">Super Admin</option>
                  <option :value="2">System User</option>
                </select>

                <div class="form-actions">
                  <button type="submit" class="save-btn">Save</button>
                  <button type="button" class="cancel-btn" @click="closeForm">Close</button>
                </div>
              </form>
            </div>
          </div>

          <!-- CONFIRMATION MODAL: Uses updated .form-popup styles -->
          <div v-if="showConfirmation" class="form-overlay" @click.self="handleConfirmation(false)">
            <div class="form-popup">
              <h3>Confirm Action</h3>
              <!-- The confirmation message text is now larger and black due to style updates -->
              <p>{{ confirmationMessage }}</p>
              <div class="form-actions">
                <button type="button" class="cancel-btn" @click="handleConfirmation(false)">Cancel</button>
                <button type="button" class="save-btn confirm-action-btn" @click="handleConfirmation(true)">Confirm</button>
              </div>
            </div>
          </div>

          <!-- DIALOG BOX: Assumes DialogBox.vue component has been updated per previous request -->
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
  //import Header from "./Header.vue";
  // NOTE: DialogBox.vue must be the updated version from the previous response
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "./LoadingDialog.vue";

  // --- Dialog & Loading ---
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const isLoading = ref(false);
  const searchTerm = ref("");

  // --- Pagination State ---
  const currentPage = ref(1);
  const pageSize = ref(10);
  // NOTE: totalUsers is a placeholder. You must update your API to return the total count
  // and update this ref's value upon successful data load.
  const totalUsers = ref(100);

  // Determine if there is a next page based on the number of users returned
  const hasNextPage = computed(() => {
    // If the number of users returned is equal to the requested page size,
    // there's potentially more data on the next page.
    return users.value.length === pageSize.value;
  });

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
    userType: 1,
    userName: "",
    firstName: "",
    lastName: "",
    userRole: 2,
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

  // --- Core Data Loading Function ---
  async function LoadSystemUsers() {
    try {
      isLoading.value = true;
      // Use the currentPage and pageSize in the API call
      const res = await api.get(`/admin/user-list/system?page=${currentPage.value}&pageSize=${pageSize.value}`);
      users.value = res.data;

      // TODO: When your backend API is updated to return the total count,
      // update the totalUsers ref here:
      // totalUsers.value = res.data.totalCount;

    } catch (err) {
      console.error("AxiosError:", err);
      if (err.response) {
        console.error("Status:", err.response.status);
        console.error("Data:", err.response.data);
        if (err.response.status === 404 && currentPage.value > 1) {
          // If 404 is returned on a page > 1, it means we navigated past the last page.
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
        dialogMessage.value = "Failed to load system users. Please log in again.";
      }
    } finally {
      isLoading.value = false;
    }
  }

  // --- Pagination Actions ---
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

  // --- Lifecycle Hooks and Watchers ---

  // Watch for changes in currentPage and reload data automatically
  watch(currentPage, () => {
    LoadSystemUsers();
  });

  // Load user + stats on mount
  onMounted(async () => {
    if (typeof auth.loadUser === "function") {
      await auth.loadUser();
    }
    // Initial load of users for page 1
    LoadSystemUsers();
  });

  // --- User Creation Functions (Unchanged) ---
  async function openAddForm() {
    form.userName = "";
    form.firstName = "";
    form.lastName = "";
    form.userType = 1;
    form.userRole = 2;
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
      dialogTitle.value = "User Created";
      dialogMessage.value = `Username: ${form.userName} \n
                             Password: ${form.password}`;

    } catch (err) {
      console.error("AxiosError:", err);
      let isHandledError = false;

      if (err.response) {
        console.error("Status:", err.response.status);
        console.error("Data:", err.response.data);

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
      // Reload users after successful creation, ideally returning to page 1
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
      dialogMessage.value = `${user.username}: ${newPassword}`;

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


  /* --- FIX: ADJUSTED LAYOUT STYLES FOR FIXED NAVBAR AND SIDEBAR --- */

  /* App Layout: Column flow */
  #app-layout {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
  }



  /* Main Body Wrapper: Pushes content down below the fixed Header (assuming Header height is ~60px) */
  .main-body-wrapper {
    display: flex;
    flex: 1;
    /* ADDED: PUSHES CONTENT BELOW THE FIXED HEADER */
    padding-top: 60px; /* Adjust this value if your Header height is different */
  }



  /* Content Area: Takes up remaining space and clears the fixed sidebar (assuming Sidebar width is ~250px) */
  .content-area {
    flex-grow: 1;
    overflow-y: auto;
    background-color: #f8f9fa;
    /* ADDED: PUSHES CONTENT PAST THE FIXED LEFT MENU/SIDEBAR */
    margin-left: 250px; /* Adjust this value if your LeftMenu width is different */
  }




  /* --- BASE PAGE CONTENT STYLES (Rest unchanged) --- */
  .page-content {
    padding: 30px;
    background-color: #f8f9fa; /* Light background for the page */
    min-height: 100%; /* Ensures the content block fills the content-area vertically */
  }

  .page-title {
    font-size: 2em;
    color: #0d47a1;
    margin-bottom: 25px;
    font-weight: 700;
    border-bottom: 2px solid #e0e0e0;
    padding-bottom: 10px;
  }



  /* --- ACTION BAR (Search and Add Button) --- */
  .action-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 25px;
    background-color: white;
    padding: 15px;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  }

  .search-group {
    position: relative;
    display: flex;
    align-items: center;
    width: 300px;
  }

  .search-input {
    width: 100%;
    padding: 10px 10px 10px 40px; /* Left padding for icon */
    border: 1px solid #ccc;
    border-radius: 6px;
    font-size: 1em;
    transition: border-color 0.2s, box-shadow 0.2s;
  }

    .search-input:focus {
      border-color: #0d47a1;
      box-shadow: 0 0 0 3px rgba(13, 71, 161, 0.2);
      outline: none;
    }

  .search-icon {
    position: absolute;
    left: 15px;
    color: #777;
    pointer-events: none;
  }

  .add-btn {
    background-color: #4caf50;
    color: white;
    border: none;
    padding: 10px 18px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s, transform 0.1s;
  }

    .add-btn:hover {
      background-color: #43a047;
      transform: translateY(-1px);
    }

    .add-btn:active {
      transform: translateY(0);
    }




  /* --- DATA TABLE --- */
  .table-container {
    overflow-x: auto;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .data-table {
    width: 100%;
    min-width: 800px; /* Ensure table is readable on small screens */
    border-collapse: separate;
    border-spacing: 0;
    background-color: white;
  }

    .data-table th, .data-table td {
      border: none;
      padding: 14px 18px;
      text-align: left;
    }


    /* Set the default color for all table cell text (td) to black/dark gray */
    .data-table td {
      color: #111;
    }

    .data-table th {
      background-color: #0d47a1;
      color: white;
      text-transform: uppercase;
      font-size: 0.85em;
      font-weight: 600;
      letter-spacing: 0.5px;
    }



    /* Rounded corners for the header row */
    .data-table thead tr th:first-child {
      border-top-left-radius: 8px;
    }

    .data-table thead tr th:last-child {
      border-top-right-radius: 8px;
    }

    .data-table tbody tr {
      border-bottom: 1px solid #eee; /* Light divider */
    }

      .data-table tbody tr:last-child {
        border-bottom: none;
      }

        .data-table tbody tr:last-child td:first-child {
          border-bottom-left-radius: 8px;
        }

        .data-table tbody tr:last-child td:last-child {
          border-bottom-right-radius: 8px;
        }

    .data-table tr:nth-child(even) {
      background-color: #f7f7ff; /* Very light blue tint */
    }

    .data-table tr:hover {
      background-color: #e3f2fd; /* Light blue highlight on hover */
    }



    /* Target the ID (1st column) and Username (2nd column) for dark, bold text */
    .data-table tbody tr td:nth-child(1),
    .data-table tbody tr td:nth-child(2) {
      color: #333;
      font-weight: 600;
    }

  .action-cell {
    white-space: nowrap;
  }



  /* --- BUTTON STYLES --- */
  .action-btn {
    font-size: 0.9em;
    font-weight: 500;
    padding: 8px 12px;
    margin: 0 4px;
    border-radius: 4px;
    cursor: pointer;
    border: none;
    color: white;
    transition: background-color 0.2s;
  }

  .status-btn {
    background-color: #ff9800; /* Orange for status toggle */
  }

    .status-btn:hover {
      background-color: #fb8c00;
    }

  .reset-btn {
    background-color: #2196f3; /* Blue for reset */
  }

    .reset-btn:hover {
      background-color: #1e88e5;
    }




  /* --- STATUS BADGES --- */
  .status-active, .status-inactive {
    padding: 4px 8px;
    border-radius: 4px;
    font-weight: bold;
    font-size: 0.85em;
  }

  .status-active {
    background-color: #e8f5e9; /* Light green */
    color: #388e3c; /* Dark green text */
  }

  .status-inactive {
    background-color: #ffebee; /* Light red */
    color: #d32f2f; /* Dark red text */
  }



  /* --- PAGINATION STYLES (NEW) --- */
  .pagination-controls {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 15px;
    padding: 15px 0;
  }

  .pagination-btn {
    background-color: #0d47a1;
    color: white;
    border: none;
    padding: 10px 15px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s;
  }

    .pagination-btn:hover:not(:disabled) {
      background-color: #1976d2;
    }

    .pagination-btn:disabled {
      background-color: #bdbdbd;
      cursor: not-allowed;
      opacity: 0.6;
    }

  .page-info {
    font-weight: 600;
    color: #333;
  }


  /* --- MODAL/FORM STYLES --- */
  .form-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .form-popup {
    background: white;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 6px 25px rgba(0, 0, 0, 0.3);
    width: 90%;
    /* INCREASED max-width for both form and confirmation popups */
    max-width: 500px;
    /* ADDED: Ensures all general inherited text in the popup is dark */
    color: #1a1a1a;
  }


    /* Confirmation message text is made larger and black */
    .form-popup p {
      font-size: 1.1em; /* Larger text for confirmation message */
      color: #1a1a1a; /* Explicitly black text */
      line-height: 1.5;
    }

    .form-popup h3 {
      margin-top: 0;
      color: #0d47a1;
      border-bottom: 1px solid #eee;
      padding-bottom: 10px;
      margin-bottom: 20px;
    }

    .form-popup label {
      display: block;
      margin-top: 10px;
      font-weight: 600;
      color: #333;
    }

    .form-popup input, .form-popup select {
      width: 100%;
      padding: 10px;
      margin-top: 5px;
      margin-bottom: 15px;
      border: 1px solid #ccc;
      border-radius: 6px;
      box-sizing: border-box;
      /* Ensure text typed/displayed inside inputs is dark */
      color: #1a1a1a;
    }

  .validation-error {
    color: #d32f2f;
    font-size: 0.85em;
    margin: 5px 0 15px;
  }

  .form-actions {
    margin-top: 25px;
    display: flex;
    justify-content: flex-end;
    gap: 10px;
  }

  .save-btn {
    background-color: #0d47a1;
    color: white;
    border: none;
    padding: 10px 15px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s;
  }

    .save-btn:hover {
      background-color: #1976d2;
    }

  .cancel-btn {
    background-color: #e0e0e0;
    color: #333;
    border: none;
    padding: 10px 15px;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    transition: background-color 0.2s;
  }

    .cancel-btn:hover {
      background-color: #bdbdbd;
    }
</style>
