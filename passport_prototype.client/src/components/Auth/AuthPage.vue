<template>
  <div class="dfa-wrapper">
    <header class="dfa-header">
      <div class="header-content">
        <img src="@/assets/dfa-logo.png" alt="DFA Logo" class="dfa-logo-img" />
        <div class="header-text">
          <p class="agency-subtext">REPUBLIC OF THE PHILIPPINES</p>
          <h1 class="agency-main-title">OFFICE OF CONSULAR AFFAIRS</h1>
          <p class="agency-location">ASEANA BUSINESS PARK, PARAÑAQUE CITY</p>
        </div>
      </div>
      <div class="top-login-bar">
        <input v-model="username" type="text" placeholder="Email Address" autocomplete="username" />
        <div class="password-group">
          <input v-model="loginPassword" type="password" placeholder="Password" autocomplete="current-password" />
          <p class="forgot-link">Forgot Password?</p>
        </div>
        <button @click="handleLogin" class="sign-in-btn">Sign In</button>
      </div>
    </header>

    <div class="ph-standard-time-bar">
      <div class="pst-content">
        <span class="pst-label">Philippine Standard Time:</span>
        <span class="pst-value">{{ currentPST }}</span>
      </div>
    </div>

    <main class="main-split-layout">
      <section class="info-column">
        <h3 class="info-header">OFFICE OF CONSULAR AFFAIRS</h3>
        <h2 class="info-title">Passport Online Registration &amp; Application System</h2>
        <p class="info-disclaimer">
          It is advisable NOT to purchase outbound travel tickets until your passports are actually
          in your possession. Department of Foreign Affairs will not be responsible for any rebooking charges,
          loss of income, and other financial compensation and/or personal losses arising from the applicant's
          trabel arrangements made while the passport has not been released.
        </p>
        <div class="contact-info">
          <span><i class="fas fa-phone"></i> +632 8234 3488</span><br>
          <span><i class="fas fa-envelope"></i> info@passport.gov.ph</span>
        </div>
      </section>

      <section class="form-column">
        <h2 class="form-title">Register</h2>

        <div class="input-group-row">
          <p class="label-text">Do you have an ePassport issued from 2009 to present?</p>
          <div class="toggle-btns">
            <button :class="{ active: isEmployee }" @click="isEmployee = true">Yes</button>
            <button :class="{ active: !isEmployee }" @click="isEmployee = false">No</button>
          </div>
        </div>

        <div class="name-suffix-row">
          <div class="form-field first-name-field">
            <label>Given Name</label>
            <input v-model="firstName" type="text" placeholder="First Name" autocomplete="given-name" />
          </div>

          <div class="form-field suffix-field">
            <label>Suffix (If applicable)</label>
            <input v-model="suffix" type="text" placeholder="e.g. Jr., III" autocomplete="honorific-suffix" />
          </div>
        </div>

        <div class="form-field">
          <label>Middle Name (If applicable)</label>
          <input v-model="middleName" type="text" placeholder="Middle Name" autocomplete="additional-name" />
        </div>

        <div class="form-field">
          <label>Last Name</label>
          <input v-model="lastName" type="text" placeholder="Last Name" autocomplete="family-name" />
        </div>

        <div class="form-field">
          <label>Email Address</label>
          <div class="input-with-action">
            <input v-model="email" type="email" placeholder="Email Address" autocomplete="email" />
            <button @click="sendVerificationCode" class="action-btn" :disabled="registerLoading">
              {{ registerLoading ? "Sending..." : "Send verification code" }}
            </button>
          </div>
        </div>

        <!-- ═══════════════════════════════════════════════════
       REGISTER OTP SLOTS
       Uses registerOtp ref only. No >= in the template —
       uses registerOtpSlots computed array instead.
  ═══════════════════════════════════════════════════ -->
        <div class="form-field">
          <label style="padding-top: 30px">Verification Code</label>
          <div class="otp-wrapper">
            <input :value="registerOtp"
                   type="text"
                   inputmode="numeric"
                   maxlength="6"
                   class="otp-hidden-input"
                   autocomplete="off"
                   @input="onRegisterOtpInput"
                   @keydown="blockNonDigits" />
            <div class="otp-slots-container">
              <!--
          FIX: No >= operator in template at all.
          registerOtpSlots is a computed array of 6 booleans
          that tells each slot whether it is filled or not.
        -->
              <div v-for="(slot, idx) in registerOtpSlots"
                   :key="idx"
                   class="otp-slot"
                   :class="{ filled: slot.filled, 'verified-slot': isEmailVerified }">
                {{ slot.char }}
              </div>
            </div>
          </div>

          <div class="otp-action-bar" style="margin-top: 10px; display: flex; gap: 10px; align-items: center">
            <button @click="verifyOtpCode"
                    class="action-btn verify-btn"
                    :disabled="registerOtpNotReady || registerLoading || isEmailVerified">
              {{ isEmailVerified ? "Verified ✓" : "Verify Code" }}
            </button>
            <span v-if="registerOtpError" class="error-text" style="color: #d9534f; font-size: 0.85rem">
              {{ registerOtpError }}
            </span>
          </div>
        </div>

        <div class="form-field">
          <label>Password</label>
          <input v-model="password" type="password" placeholder="Enter Password" autocomplete="new-password" />
        </div>

        <div class="form-field">
          <label>Confirm Password</label>
          <input v-model="confirmPassword" type="password" placeholder="Repeat Password" autocomplete="new-password" />
        </div>

        <div class="password-rules-container">
          <label>Your Password Must Have:</label>
          <ul class="password-rules">
            <li><i :class="['fa', hasUppercase ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> One uppercase letter</li>
            <li><i :class="['fa', hasLowercase ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> One lowercase letter</li>
            <li><i :class="['fa', hasNumber ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> At least one number</li>
            <li><i :class="['fa', noSpaces ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> No spaces</li>
            <li><i :class="['fa', minLength ? 'fa-check-circle valid' : 'fa-times-circle invalid']"></i> 8 or more characters</li>
          </ul>
        </div>

        <div class="captcha-placeholder">
          <Captcha @verified="onCaptchaVerified" />
        </div>

        <div class="terms-check">
          <input type="checkbox" v-model="isTermsAccepted" />
          <span>Read and Accept <a href="#">Terms of Service</a></span>
        </div>

        <button @click="handleRegister"
                class="submit-register-btn"
                :disabled="!canRegister || registerLoading"
                :class="{ 'btn-disabled': !canRegister }">
          {{ registerLoading ? "Processing..." : "Sign Up" }}
        </button>
      </section>
    </main>

    <!-- ═══════════════════════════════════════════════════════════
       LOGIN OTP DIALOG — uses loginOtp ref only.
       Completely isolated from register form above.
  ═══════════════════════════════════════════════════════════ -->
    <DialogBox :show="showOtpDialog" title="Login Verification" @close="closeLoginDialog">
      <div class="otp-dialog-content">
        <p>A 6-digit verification code has been sent to your email.</p>
        <div class="otp-wrapper" style="margin: 20px 0">
          <input :value="loginOtp"
                 type="text"
                 inputmode="numeric"
                 maxlength="6"
                 placeholder="000000"
                 class="otp-input-simple"
                 autocomplete="off"
                 @input="onLoginOtpInput"
                 @keydown="blockNonDigits" />
        </div>
        <button @click="handleVerifyLoginOtp"
                class="action-btn"
                :disabled="loginOtpNotReady || loginLoading">
          {{ loginLoading ? "Verifying..." : "Verify &amp; Sign In" }}
        </button>
        <p v-if="loginOtpError" class="error-text" style="color: #d9534f; margin-top: 10px">
          {{ loginOtpError }}
        </p>
      </div>
    </DialogBox>

    <LoadingDialog :visible="registerLoading || loginLoading || resendLoading" />

    <DialogBox :show="showMessageDialog"
               :title="messageTitle"
               @close="showMessageDialog = false">
      <div class="dialog-message-container">
        <p class="dialog-message-line">{{ messageBody }}</p>
        <button @click="showMessageDialog = false" class="auth-btn-style">OK</button>
      </div>
    </DialogBox>
  </div>
</template>

<script setup>
  import Captcha from "@/components/Auth/Captcha.vue";
  import { ref, computed, watch, nextTick, onMounted, onUnmounted } from "vue";
  import { useRouter, useRoute } from "vue-router";
  import api from "@/api";
  import { useAuthStore } from "@/stores/auth";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import TermsOfService from "@/components/TermsOfService.vue";
  import AuthHeader from "@/components/Auth/AuthHeader.vue";
  import { useHead } from "@vueuse/head";

  useHead({
    script: [
      { src: "https://www.google.com/recaptcha/api.js", async: true, defer: true },
    ],
  });

  function onSubmit(token) {
    document.getElementById("demo-form").submit();
  }

  const router = useRouter();
  const route = useRoute();
  const auth = useAuthStore();

  // ── Form state ──────────────────────────────────────────────────────
  const hasMiddleName = ref(true);
  const middleName = ref("");
  const captchaVerified = ref(false);
  const captchaKey = ref(0);
  const showTerms = ref(false);
  const isTermsAccepted = ref(false);
  const currentPST = ref("");

  const updatePST = () => {
    const options = {
      weekday: "long", year: "numeric", month: "long", day: "numeric",
      hour: "2-digit", minute: "2-digit", second: "2-digit", hour12: true,
    };
    currentPST.value = new Date().toLocaleString("en-US", options);
  };

  // ── Auth / login state ──────────────────────────────────────────────
  const isLogin = ref(true);
  const loginStep = ref("credentials");
  const username = ref("");
  const email = ref("");
  const loginPassword = ref("");
  const password = ref("");
  const confirmPassword = ref("");
  const showPassword = ref(false);
  const showConfirm = ref(false);
  const lastName = ref("");
  const firstName = ref("");
  const isEmployee = ref(false);

  // ── Employee / ID state ─────────────────────────────────────────────
  const employeeID = ref("");
  const birthday = ref("");
  const govIDType = ref("");
  const govIDNumber = ref("");

  // ── File upload state ───────────────────────────────────────────────
  const idImageBase64 = ref("");
  const idFileExtension = ref("");
  const validIDOriginalName = ref(null);
  const validIDFileKey = ref(null);
  const documentTypeToUpload = ref("");
  const requiredDocFile = ref(null);

  // ── Dialog state ────────────────────────────────────────────────────
  const showDialog = ref(false);
  const dialogTitle = ref("");
  const dialogMessage = ref("");
  const showForgotDialog = ref(false);
  const showOtpDialog = ref(false);
  const forgotEmail = ref("");
  const registerLoading = ref(false);  // register form ONLY
  const loginLoading = ref(false);  // login dialog ONLY
  const resendLoading = ref(false);
  const showResend = ref(false);

  // ── Countdown ───────────────────────────────────────────────────────
  const cooldownSeconds = 60;
  const countdown = ref(10);
  const resendAttempts = ref(0);
  const maxAttempts = 3;
  const radius = 45;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = computed(
    () => circumference - (countdown.value / cooldownSeconds) * circumference
  );
  let timer = null;

  const showMessageDialog = ref(false);
  const messageTitle = ref("");
  const messageBody = ref("");

  // Helper function to trigger the dialog easily
  const showAlert = (title, message) => {
    messageTitle.value = title;
    messageBody.value = message;
    showMessageDialog.value = true;
  };

  // ════════════════════════════════════════════════════════════════════
  // ROOT CAUSE OF THE BUG + THE TEMPLATE PARSE ERROR
  //
  // Bug 1 — single shared ref:
  //   Original code used ONE ref called `otp` bound via v-model to BOTH
  //   the register OTP slots AND the login dialog input. Any keystroke in
  //   the login dialog updated `otp`, which re-rendered the register slots
  //   live — they were the same variable.
  //
  // Bug 2 — >= in template:
  //   The fix used `registerOtp.length >= i` directly in :class="{}".
  //   Vue's template compiler delegates HTML tokenisation to an HTML
  //   parser before it ever runs the JS parser. The HTML parser sees `>`
  //   and treats it as the closing bracket of the tag — so it cuts the
  //   attribute short and throws "Unexpected token".
  //
  // THE FIX for both:
  //   1. Two separate refs: registerOtp (register form) and loginOtp (login dialog).
  //   2. ZERO comparison operators inside the template.
  //      Instead we use a computed array `registerOtpSlots` — a plain
  //      array of 6 objects { char, filled } — so the template only reads
  //      properties, never evaluates operators.
  // ════════════════════════════════════════════════════════════════════

  /** Verification code for the REGISTER form only */
  const registerOtp = ref("");

  /** OTP for the LOGIN dialog only */
  const loginOtp = ref("");

  /** Error text shown below the register OTP slots */
  const registerOtpError = ref("");

  /** Error text shown inside the login OTP dialog */
  const loginOtpError = ref("");

  /**
   * Computed array of 6 slot-descriptor objects.
   * Used by v-for in the register OTP slots.
   * No comparison operator ever appears in the template.
   *
   * Example when registerOtp = "123":
   *   [
   *     { char: "1", filled: true  },
   *     { char: "2", filled: true  },
   *     { char: "3", filled: true  },
   *     { char: "",  filled: false },
   *     { char: "",  filled: false },
   *     { char: "",  filled: false },
   *   ]
   */
  const registerOtpSlots = computed(() => {
    return Array.from({ length: 6 }, (_, i) => ({
      char: registerOtp.value[i] ?? "",
      filled: i < registerOtp.value.length,
    }));
  });

  /** true when register OTP is shorter than 6 digits (used for :disabled) */
  const registerOtpNotReady = computed(() => registerOtp.value.length < 6);

  /** true when login OTP is shorter than 6 digits (used for :disabled) */
  const loginOtpNotReady = computed(() => loginOtp.value.length < 6);

  // ── Password validation ─────────────────────────────────────────────
  const hasUppercase = computed(() => /[A-Z]/.test(password.value));
  const hasLowercase = computed(() => /[a-z]/.test(password.value));
  const hasNumber = computed(() => /[0-9]/.test(password.value));
  const noSpaces = computed(() => !/\s/.test(password.value));
  const minLength = computed(() => password.value.length >= 8);
  const isPasswordValid = computed(
    () => hasUppercase.value && hasLowercase.value && hasNumber.value &&
      noSpaces.value && minLength.value
  );

  const passwordMismatch = computed(
    () => confirmPassword.value && password.value !== confirmPassword.value
  );

  const isEmailVerified = ref(false);

  const canLogin = computed(() => username.value && loginPassword.value);

  const isEmployeeIdRequired = computed(() => !isEmployee.value || !!employeeID.value);

  const canRegister = computed(() => {
    const isNameOrIdValid = isEmployee.value ? !!employeeID.value : !!firstName.value;
    return (
      isNameOrIdValid &&
      lastName.value &&
      email.value &&
      isEmailVerified.value &&
      isPasswordValid.value &&
      !passwordMismatch.value &&
      isTermsAccepted.value &&
      captchaVerified.value
    );
  });

  // ── Field errors ────────────────────────────────────────────────────
  const fieldErrors = ref({
    email: false, employeeID: false, firstName: false,
    lastName: false, password: false, birthday: false,
  });
  const clearErrors = () => {
    fieldErrors.value = {
      email: false, employeeID: false, firstName: false,
      lastName: false, password: false, birthday: false,
    };
  };

  // ── Lifecycle ───────────────────────────────────────────────────────
  onMounted(() => {
    updatePST();
    timer = setInterval(updatePST, 1000);

    const endTime = localStorage.getItem("otpCooldownEnd");
    if (endTime) {
      const remaining = Math.floor((endTime - Date.now()) / 1000);
      if (remaining > 0) {
        countdown.value = remaining;
        showResend.value = false;
        timer = setInterval(updateCountdown, 1000);
      } else {
        localStorage.removeItem("otpCooldownEnd");
        showResend.value = true;
      }
    }
  });

  onUnmounted(() => { if (timer) clearInterval(timer); });

  // ── Watchers ────────────────────────────────────────────────────────
  watch(isLogin, (newVal) => {
    if (newVal) {
      username.value = "";
      loginPassword.value = "";
      loginOtp.value = "";
      loginOtpError.value = "";
      forgotEmail.value = "";
    } else {
      resetRegistrationForm();
    }
  });

  const onCaptchaVerified = (status) => { captchaVerified.value = status; };

  const resetRegistrationForm = () => {
    employeeID.value = "";
    birthday.value = "";
    firstName.value = "";
    lastName.value = "";
    email.value = "";
    password.value = "";
    confirmPassword.value = "";
    isEmployee.value = false;
    govIDType.value = "";
    govIDNumber.value = "";
    removeID();
    isTermsAccepted.value = false;
    captchaVerified.value = false;
    showTerms.value = false;
    captchaKey.value++;
    registerOtp.value = "";   // register OTP only
    registerOtpError.value = "";
    isEmailVerified.value = false;
    clearErrors();
  };

  // ── Forgot password ─────────────────────────────────────────────────
  const openForgotPasswordDialog = () => {
    forgotEmail.value = username.value || "";
    showForgotDialog.value = true;
  };

  const handleForgotPassword = async () => {
    if (!forgotEmail.value) return;
    try {
      registerLoading.value = true;
      await api.post("/auth/forgot-password", { email: forgotEmail.value });
      dialogTitle.value = "Success";
      dialogMessage.value = "✅ Check your email for password reset instructions.";
      showDialog.value = true;
    } catch (err) {
      dialogTitle.value = "Error";
      dialogMessage.value = err.response?.data?.message || "Something went wrong.";
      showDialog.value = true;
    } finally {
      registerLoading.value = false;
      showForgotDialog.value = false;
    }
  };

  // ── Countdown helpers ───────────────────────────────────────────────
  const startCooldown = () => {
    showResend.value = false;
    countdown.value = 60;
    if (timer) clearInterval(timer);
    timer = setInterval(() => {
      countdown.value--;
      if (countdown.value <= 0) { clearInterval(timer); showResend.value = true; }
    }, 1000);
  };

  const updateCountdown = () => {
    const endTime = localStorage.getItem("otpCooldownEnd");
    if (!endTime) return;
    const remaining = Math.floor((parseInt(endTime) - Date.now()) / 1000);
    if (remaining <= 0) {
      clearInterval(timer);
      localStorage.removeItem("otpCooldownEnd");
      showResend.value = true;
      countdown.value = 0;
    } else {
      countdown.value = remaining;
    }
  };

  // ── Legacy OTP array helpers (kept for backward compat) ─────────────
  const otpArray = ref(["", "", "", "", "", ""]);

  const handleOtpInput = (event, index) => {
    const value = event.target.value;
    if (!/^\d$/.test(value)) { otpArray.value[index] = ""; return; }
    if (value && index < 5) document.getElementById(`otp-${index + 1}`)?.focus();
  };
  const handleOtpDelete = (event, index) => {
    if (!otpArray.value[index] && index > 0)
      document.getElementById(`otp-${index - 1}`)?.focus();
  };
  const handleOtpPaste = (event) => {
    const pasteData = event.clipboardData.getData("text").slice(0, 6);
    if (!/^\d+$/.test(pasteData)) return;
    const digits = pasteData.split("");
    digits.forEach((d, i) => { if (i < 6) otpArray.value[i] = d; });
    document.getElementById(`otp-${digits.length === 6 ? 5 : digits.length}`)?.focus();
  };

  // ════════════════════════════════════════════════════════════════════
  // OTP INPUT HANDLERS
  //
  // :value + @input pattern (not v-model) so we are the sole authority
  // over each ref and no Vue reactivity race can overwrite a digit.
  // Each handler writes ONLY to its own isolated ref.
  // ════════════════════════════════════════════════════════════════════

  /** Register OTP input handler — writes to registerOtp ONLY */
  const onRegisterOtpInput = (e) => {
    const cleaned = e.target.value.replace(/\D/g, "").slice(0, 6);
    registerOtp.value = cleaned;
    if (e.target.value !== cleaned) e.target.value = cleaned;
    if (registerOtpError.value) registerOtpError.value = "";
  };

  /** Login OTP input handler — writes to loginOtp ONLY */
  const onLoginOtpInput = (e) => {
    const cleaned = e.target.value.replace(/\D/g, "").slice(0, 6);
    loginOtp.value = cleaned;
    if (e.target.value !== cleaned) e.target.value = cleaned;
    if (loginOtpError.value) loginOtpError.value = "";
  };

  /**
   * Shared keydown guard.
   * Allows: 0-9, Backspace, Delete, Tab, Arrow keys, Ctrl/Cmd combos.
   * Blocks everything else before it reaches the input.
   */
  const blockNonDigits = (e) => {
    const allowed = ["Backspace", "Delete", "Tab", "ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown"];
    const isCtrlCmd = e.ctrlKey || e.metaKey;
    const isCombo = isCtrlCmd && ["a", "c", "v", "x"].includes(e.key.toLowerCase());
    if (allowed.includes(e.key) || isCombo) return;
    if (!/^\d$/.test(e.key)) e.preventDefault();
  };

  // ── Close login dialog — clears loginOtp only ───────────────────────
  const closeLoginDialog = () => {
    showOtpDialog.value = false;
    loginOtp.value = "";   // login OTP only — never touches registerOtp
    loginOtpError.value = "";
  };

  // ── Resend OTP ──────────────────────────────────────────────────────
  const handleResendOtp = async () => {
    if (resendAttempts.value >= maxAttempts) {
      loginOtpError.value = "Maximum resend attempts reached.";
      return;
    }
    resendLoading.value = true;
    loginOtpError.value = "";
    try {
      await api.post("/auth/resend-otp", { email: username.value });
      resendAttempts.value++;
      startCooldown();
    } catch (err) {
      loginOtpError.value = err.response?.data?.message || "Failed to resend OTP";
    } finally {
      resendLoading.value = false;
    }
  };

  // ── File helpers ────────────────────────────────────────────────────
  function triggerValidIDUpload() {
    documentTypeToUpload.value = "ValidID";
    if (requiredDocFile.value) requiredDocFile.value.click();
  }
  async function handleGenericFileSelection(event) {
    const file = event.target.files[0];
    if (!file) return;
    idFileExtension.value = file.name.substring(file.name.lastIndexOf("."));
    validIDOriginalName.value = file.name;
    const reader = new FileReader();
    reader.onload = (e) => { idImageBase64.value = e.target.result.split(",")[1]; };
    reader.readAsDataURL(file);
    event.target.value = null;
  }
  function removeID() {
    idImageBase64.value = "";
    idFileExtension.value = "";
    validIDOriginalName.value = null;
  }
  async function deleteFile(docType, fileKey) {
    if (!fileKey) return;
    registerLoading.value = true;
    dialogTitle.value = "Deleting";
    showDialog.value = true;
    try {
      await api.delete(`/employee/deleteupload?fileKey=${fileKey}`);
      if (docType === "ValidID") { validIDFileKey.value = null; validIDOriginalName.value = null; }
      dialogTitle.value = "Success";
      dialogMessage.value = "ID removed successfully.";
    } catch (err) {
      dialogTitle.value = "Error";
      dialogMessage.value = "Failed to remove the file.";
    } finally {
      registerLoading.value = false;
    }
  }

  // ════════════════════════════════════════════════════════════════════
  // API CALLS
  // ════════════════════════════════════════════════════════════════════

  // 1. Send registration verification email
  const sendVerificationCode = async () => {
    if (!email.value) { showAlert("Please enter an email address."); return; }
    registerLoading.value = true;
    registerOtp.value = "";   // clear so user types a fresh code
    registerOtpError.value = "";
    isEmailVerified.value = false;
    try {
      const response = await api.post("Auth/initiate-registration", { email: email.value });
      showAlert("Code Sent", response.data.message);
    } catch (err) {
      showAlert("Error", err.response?.data?.message || "Failed to send code.");
    } finally {
      registerLoading.value = false;
    }
  };

  // 2. Verify registration OTP — reads registerOtp ONLY
  const verifyOtpCode = async () => {
    const code = registerOtp.value.trim();
    if (code.length !== 6) { registerOtpError.value = "Please enter the 6-digit code."; return; }
    registerLoading.value = true;
    registerOtpError.value = "";
    try {
      await api.post("/auth/verify-registration-otp", {
        email: email.value,
        verificationCode: code,        // registerOtp — never loginOtp
      });
      isEmailVerified.value = true;
      showAlert("Success", "Email Verified! You can now complete your registration.");
    } catch (err) {
      registerOtpError.value = err.response?.data?.message || "Invalid or expired code.";
      isEmailVerified.value = false;
    } finally {
      registerLoading.value = false;
    }
  };

  // 3. Complete registration — sends registerOtp in payload
  const handleRegister = async () => {
    if (!canRegister.value) return;
    registerLoading.value = true;
    try {
      const payload = {
        email: email.value,
        password: password.value,
        firstName: firstName.value,
        middleName: hasMiddleName.value ? middleName.value : "",
        lastName: lastName.value,
        suffix: "",
        verificationCode: registerOtp.value.trim(), // registerOtp — never loginOtp
      };
      console.log("Sending Clean Payload:", payload);
      await api.put("/Auth/complete-registration", payload);
      showAlert("Registration Successful!","You can now log in using your new account.");
      router.push("/login");
    } catch (err) {
      console.error("Registration Error:", err.response?.data);
      showAlert("Registration Failed", err.response?.data?.message || "Registration failed.");
    } finally {
      registerLoading.value = false;
    }
  };

  // 4. Login — always clears loginOtp before opening dialog
  const handleLogin = async () => {
    if (!username.value || !loginPassword.value) { alert("Please enter credentials."); return; }
    loginLoading.value = true;
    loginOtp.value = "";   // blank slate every time dialog opens
    loginOtpError.value = "";
    try {
      await api.post("/auth/login", {
        email: username.value,
        password: loginPassword.value,
      });
      console.log("Login API Success, showing dialog...");
      showOtpDialog.value = true;
    } catch (err) {
      showAlert("Login Failed", err.response?.data?.message || "Login failed.");
    } finally {
      loginLoading.value = false;
    }
  };

  // 5. Verify login OTP — reads loginOtp ONLY
  const handleVerifyLoginOtp = async () => {
    if (loginOtp.value.length !== 6) return;
    loginLoading.value = true;
    loginOtpError.value = "";
    try {
      const response = await api.post("/auth/verify-otp", {
        email: username.value,
        verificationCode: loginOtp.value,  // loginOtp — never registerOtp
      });
      auth.login({ token: response.data.token, user: response.data.user });
      closeLoginDialog();
      showAlert("Success", "Login Successful!");
      const role = parseInt(auth.userRole);
      if (role === 1) router.push("/dashboard-admin");
      else if (role === 4) router.push("/applicationassessment");
      else router.push("/profile");
    } catch (err) {
      loginOtpError.value = err.response?.data?.message || "Invalid or expired OTP.";
    } finally {
      loginLoading.value = false;
    }
  };
</script>

<style>
  .otp-dialog-content,
  [class*="dialog"],
  [class*="modal"] {
    z-index: 9999 !important;
/*    position: relative;*/
  }
</style>
