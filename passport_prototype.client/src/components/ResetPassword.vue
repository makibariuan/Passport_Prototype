<template>
  <div class="auth-page-wrapper">
    <div class="logo-container">
      <img src="@/assets/makati-logo.png" alt="Makati Logo" class="main-logo" />
      <div class="page-title">
        <h2>Welcome to</h2>
        <h1>Makati Onboarding System</h1>
      </div>
    </div>

    <div class="auth-card">
      <div class="card-header">
        <h2>{{ !isReset ? 'Forgot Password' : 'Reset Password' }}</h2>
        <p class="subtitle">
          {{ !isReset ? 'Enter your email to receive a reset link' : 'Set a strong password to secure your account' }}
        </p>
      </div>

      <div v-if="!isReset" class="form-wrapper">
        <label class="input-label">Email Address</label>
        <input v-model="forgotEmail"
               type="email"
               placeholder="name@example.com"
               class="auth-input" />
        <button @click="handleForgotPassword" class="auth-button main-btn">
          Send Reset Link
        </button>
      </div>

      <div v-else class="form-wrapper">
        <label class="input-label">New Password</label>
        <input v-model="newPassword"
               type="password"
               placeholder="New Password"
               class="auth-input" />

        <label class="input-label">Confirm Password</label>
        <input v-model="confirmPassword"
               type="password"
               placeholder="Confirm New Password"
               class="auth-input" />

        <div class="password-validation-box">
          <p class="validation-title">Your Password Must Have:</p>
          <ul class="password-rules">
            <li :class="{ 'rule-valid': hasUppercase }">
              <i :class="['fas', hasUppercase ? 'fa-check-circle' : 'fa-times-circle']"></i>
              One uppercase letter
            </li>
            <li :class="{ 'rule-valid': hasLowercase }">
              <i :class="['fas', hasLowercase ? 'fa-check-circle' : 'fa-times-circle']"></i>
              One lowercase letter
            </li>
            <li :class="{ 'rule-valid': hasNumber }">
              <i :class="['fas', hasNumber ? 'fa-check-circle' : 'fa-times-circle']"></i>
              At least one number
            </li>
            <li :class="{ 'rule-valid': noSpaces }">
              <i :class="['fas', noSpaces ? 'fa-check-circle' : 'fa-times-circle']"></i>
              No spaces
            </li>
            <li :class="{ 'rule-valid': minLength }">
              <i :class="['fas', minLength ? 'fa-check-circle' : 'fa-times-circle']"></i>
              8 or more characters
            </li>
          </ul>
        </div>

        <button @click="handleResetPassword"
                class="auth-button main-btn"
                :disabled="!isPasswordValid">
          Reset Password
        </button>
      </div>

      <div class="card-footer">
        <router-link to="/login" class="back-link">
          <i class="fas fa-arrow-left"></i> Back to Login
        </router-link>
      </div>
    </div>

    <DialogBox :show="showDialog"
               :title="dialogTitle"
               :message="dialogMessage"
               @close="showDialog = false" />
  </div>
</template>

<script setup>
  import { ref, onMounted, computed } from "vue";
  import { useRoute, useRouter } from "vue-router";
  import api from "@/api";
  import DialogBox from "@/components/DialogBox.vue";

  // ---------------- REACTIVE STATE ----------------
  const forgotEmail = ref("");
  const newPassword = ref("");
  const confirmPassword = ref("");
  const token = ref("");
  const email = ref("");
  const isReset = ref(false);

  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");

  const route = useRoute();
  const router = useRouter();

  // ---------------- PASSWORD VALIDATION ----------------
  const hasUppercase = computed(() => /[A-Z]/.test(newPassword.value));
  const hasLowercase = computed(() => /[a-z]/.test(newPassword.value));
  const hasNumber = computed(() => /[0-9]/.test(newPassword.value));
  const noSpaces = computed(() => !/\s/.test(newPassword.value));
  const minLength = computed(() => newPassword.value.length >= 8);

  const isPasswordValid = computed(() =>
    hasUppercase.value &&
    hasLowercase.value &&
    hasNumber.value &&
    noSpaces.value &&
    minLength.value
  );

  // ---------------- INIT ----------------
  onMounted(() => {
    if (route.query.token && route.query.email) {
      token.value = route.query.token;
      email.value = route.query.email;
      isReset.value = true;
    }
  });

  // ---------------- FORGOT PASSWORD ----------------
  const handleForgotPassword = async () => {
    const emailInput = forgotEmail.value?.trim();
    if (!emailInput) return;

    try {
      await api.post("/auth/forgot-password", { email: emailInput });
      dialogTitle.value = "Success";
      dialogMessage.value = "✅ Check your email for password reset instructions.";
      showDialog.value = true;
      forgotEmail.value = "";
    } catch (err) {
      console.error(err);
      dialogTitle.value = "Error";
      dialogMessage.value = err.response?.data?.message || "Something went wrong.";
      showDialog.value = true;
    }
  };

  // ---------------- RESET PASSWORD ----------------
  const handleResetPassword = async () => {
    if (!isPasswordValid.value) {
      dialogTitle.value = "Password";
      dialogMessage.value = "Please ensure your password meets all the requirements.";
      showDialog.value = true;
      return;
    }

    if (!newPassword.value || !confirmPassword.value) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Please fill in all fields.";
      showDialog.value = true;
      return;
    }

    if (newPassword.value !== confirmPassword.value) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Passwords do not match.";
      showDialog.value = true;
      return;
    }

    try {
      await api.post("/auth/reset-password", {
        email: email.value,
        token: token.value,
        newPassword: newPassword.value,
      });

      dialogTitle.value = "Success";
      dialogMessage.value = "✅ Your password has been reset successfully.";
      showDialog.value = true;

      setTimeout(() => router.push("/login"), 2000);
    } catch (err) {
      console.error(err);
      dialogTitle.value = "Error";
      const msg =
        err.response?.data?.message ||
        err.message ||
        "Incorrect username or password.";

      if (msg.includes("Please confirm your email")) {
        router.push({ name: "ConfirmEmail", query: { notice: "pending" } });
        return;
      }

      dialogMessage.value = msg;
      showDialog.value = true;
    }
  };
</script>

<style scoped>
  .auth-page-wrapper {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    padding: 20px;
  }

  .logo-container {
    text-align: center;
    margin-bottom: 30px;
  }

  .main-logo {
    width: 120px;
    height: auto;
    margin-bottom: 15px;
  }

  .page-title h2 {
    font-size: 1.1rem;
    color: #555;
    margin: 0;
    font-weight: 400;
  }

  .page-title h1 {
    font-size: 1.8rem;
    color: #2c3e50;
    margin: 0;
    font-weight: 700;
  }

  /* Card Styling */
  .auth-card {
    width: 100%;
    max-width: 450px;
    background: #ffffff;
    padding: 40px;
    border-radius: 16px;
    box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  }

  .card-header {
    text-align: center;
    margin-bottom: 30px;
  }

    .card-header h2 {
      color: #2c3e50;
      margin-bottom: 10px;
    }

  .subtitle {
    color: #7f8c8d;
    font-size: 0.9rem;
  }

  /* Form Elements */
  .input-label {
    display: block;
    font-size: 0.85rem;
    font-weight: 600;
    color: #34495e;
    margin-bottom: 8px;
  }

  .auth-input {
    width: 100%;
    height: 42px; /* Standardized height */
    padding: 0 15px;
    margin-bottom: 20px;
    border: 1.5px solid #dce1e7;
    border-radius: 8px;
    font-size: 1rem;
    transition: border-color 0.3s;
    box-sizing: border-box;
  }

    .auth-input:focus {
      border-color: #3498db;
      outline: none;
    }

  .main-btn {
    width: 100%;
    height: 45px;
    background-color: #2c3e50; /* Makati Navy Blue */
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.3s;
  }

    .main-btn:hover:not(:disabled) {
      background-color: #34495e;
    }

    .main-btn:disabled {
      background-color: #bdc3c7;
      cursor: not-allowed;
    }

  /* Validation Rules Box */
  .password-validation-box {
    background: #f8f9fa;
    padding: 15px;
    border-radius: 8px;
    margin-bottom: 25px;
    border: 1px solid #edf2f7;
  }

  .validation-title {
    font-size: 0.8rem;
    font-weight: 700;
    color: #4a5568;
    margin-bottom: 10px;
    text-transform: uppercase;
  }

  .password-rules {
    list-style: none;
    padding: 0;
    margin: 0;
  }

    .password-rules li {
      font-size: 0.85rem;
      color: #a0aec0; /* Default Gray */
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 6px;
      transition: color 0.3s;
    }

    .password-rules i {
      font-size: 1rem;
    }

  /* Success State for Rules */
  .rule-valid {
    color: #38a169 !important; /* Green */
  }

  .card-footer {
    margin-top: 30px;
    text-align: center;
  }

  .back-link {
    color: #3498db;
    text-decoration: none;
    font-weight: 600;
    font-size: 0.9rem;
  }

    .back-link:hover {
      text-decoration: underline;
    }
</style>
