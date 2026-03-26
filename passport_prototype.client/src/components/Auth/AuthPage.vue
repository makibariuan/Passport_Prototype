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
        <input v-model="username" type="text" placeholder="Email Address" />

        <div class="password-group">
          <input v-model="loginPassword" type="password" placeholder="Password" />
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
        <h2 class="info-title">Passport Online Registration & Application System</h2>
        <p class="info-disclaimer">
          It is advisable NOT to purchase outbound travel tickets until your passports are actually
          in your possession...
        </p>
        <div class="contact-info">
          <span><i class="fas fa-phone"></i> +632 8234 3488</span>
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

        <div class="form-field">
          <label>First Name</label>
          <input v-model="firstName" type="text" placeholder="First Name" />
        </div>

        <div class="form-field">
          <label>Middle Name</label>
          <div class="toggle-btns">
            <button :class="{ active: hasMiddleName }" @click="hasMiddleName = true">Yes</button>

            <button :class="{ active: !hasMiddleName }" @click="hasMiddleName = false">No</button>
          </div>

          <input
            v-if="hasMiddleName"
            v-model="middleName"
            type="text"
            placeholder="Middle Name"
            style="margin-top: 8px"
          />
        </div>

        <div class="form-field">
          <label>Last Name</label>
          <input v-model="lastName" type="text" placeholder="Last Name" />
        </div>

        <div class="form-field">
          <label>Suffix</label>
          <input type="text" placeholder="Suffix" />
        </div>

        <div class="input-with-action">
          <input v-model="email" type="email" placeholder="Email Address" />
          <button @click="sendVerificationCode" class="action-btn" :disabled="isLoading">
            {{ isLoading ? "Sending..." : "Send verification code" }}
          </button>
        </div>

        <div class="form-field">
          <label style="padding-top: 30px">Verification Code</label>
          <div class="otp-wrapper">
            <input
              v-model="otp"
              type="text"
              maxlength="6"
              class="otp-hidden-input"
              autocomplete="one-time-code"
            />

            <div class="otp-slots-container">
              <div
                v-for="i in 6"
                :key="i"
                class="otp-slot"
                :class="{ filled: otp.length >= i, 'verified-slot': isEmailVerified }"
              >
                {{ otp[i - 1] || "" }}
              </div>
            </div>
          </div>

          <div
            class="otp-action-bar"
            style="margin-top: 10px; display: flex; gap: 10px; align-items: center"
          >
            <button
              @click="verifyOtpCode"
              class="action-btn verify-btn"
              :disabled="otp.length < 6 || isLoading || isEmailVerified"
            >
              {{ isEmailVerified ? "Verified ✓" : "Verify Code" }}
            </button>

            <span v-if="otpError" class="error-text" style="color: #d9534f; font-size: 0.85rem">
              {{ otpError }}
            </span>
          </div>
        </div>

        <div class="form-field">
          <label>Password</label>
          <input v-model="password" type="password" placeholder="Enter Password" />
        </div>

        <div class="form-field">
          <label>Repeat Password</label>
          <input v-model="confirmPassword" type="password" placeholder="Repeat Password" />
        </div>

        <div class="captcha-placeholder">
          <Captcha @verified="onCaptchaVerified" />
        </div>

        <div class="terms-check">
          <input type="checkbox" v-model="isTermsAccepted" />
          <span>Read and Accept <a href="#">Terms of Service</a></span>
        </div>

        <button
          @click="handleRegister"
          class="submit-register-btn"
          :disabled="!canRegister || isLoading"
          :class="{ 'btn-disabled': !canRegister }"
        >
          {{ isLoading ? "Processing..." : "Sign Up" }}
        </button>
      </section>
    </main>

    <DialogBox :show="showOtpDialog" title="Login Verification" @close="showOtpDialog = false">
      <div class="otp-dialog-content">
        <p>A 6-digit verification code has been sent to your email.</p>
        <div class="otp-wrapper" style="margin: 20px 0">
          <input
            v-model="otp"
            type="text"
            maxlength="6"
            placeholder="000000"
            class="otp-input-simple"
          />
        </div>
        <button
          @click="handleVerifyLoginOtp"
          class="action-btn"
          :disabled="otp.length < 6 || isLoading"
        >
          {{ isLoading ? "Verifying..." : "Verify & Sign In" }}
        </button>
        <p v-if="otpError" class="error-text" style="color: #d9534f; margin-top: 10px">
          {{ otpError }}
        </p>
      </div>
    </DialogBox>
  </div>
</template>

<script setup>
// NOTE: Assuming these components are available in the project structure
import Captcha from "@/components/Auth/Captcha.vue";
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import api from "@/api";
import { useAuthStore } from "@/stores/auth";
import DialogBox from "@/components/DialogBox.vue";
import LoadingDialog from "@/components/LoadingDialog.vue";
import TermsOfService from "@/components/TermsOfService.vue";
import AuthHeader from "@/components/Auth/AuthHeader.vue";

// Using @vueuse/head for script injection
import { useHead } from "@vueuse/head";

useHead({
  script: [
    {
      src: "https://www.google.com/recaptcha/api.js",
      async: true,
      defer: true,
    },
  ],
});

function onSubmit(token) {
  document.getElementById("demo-form").submit();
}

// Router & Auth
const router = useRouter();
const route = useRoute();
const auth = useAuthStore();

const hasMiddleName = ref(true); // Default to "Yes"
const middleName = ref("");

// captcha verification
const captchaVerified = ref(false);
const captchaKey = ref(0);

// terms of service
const showTerms = ref(false);
const isTermsAccepted = ref(false);

const currentPST = ref("");

const updatePST = () => {
  const options = {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    hour12: true,
  };
  currentPST.value = new Date().toLocaleString("en-US", options);
};

onMounted(() => {
  updatePST();
  timer = setInterval(updatePST, 1000);
});

// Login/Register State
const isLogin = ref(true);
const loginStep = ref("credentials");
const username = ref(""); // Dont forget to leave empty
const email = ref("");
const loginPassword = ref("");
const password = ref(""); // Dont forget to leave empty
const confirmPassword = ref("");
const otp = ref("");
const showPassword = ref(false); // Dont forget to leave to false
const showConfirm = ref(false);

// OTP animation and countdown reference

const cooldownSeconds = 60;
const countdown = ref(10);
const resendAttempts = ref(0);
const maxAttempts = 3;
const radius = 45;
const circumference = 2 * Math.PI * radius;

const strokeDashoffset = computed(() => {
  return circumference - (countdown.value / cooldownSeconds) * circumference;
});

let timer = null;

const lastName = ref("");
const firstName = ref("");

// 🔥 NEW: Employee registration state
const isEmployee = ref(false);

// Dialogs
const showDialog = ref(false);
const dialogTitle = ref("");
const dialogMessage = ref("");
const showForgotDialog = ref(false);
const showOtpDialog = ref(false);
const forgotEmail = ref("");
const otpError = ref("");
const isLoading = ref(false);
const resendLoading = ref(false);
const showResend = ref(false);

const otpArray = ref(["", "", "", "", "", ""]);

// Computed
const passwordMismatch = computed(
  () => confirmPassword.value && password.value !== confirmPassword.value,
);
const canLogin = computed(() => username.value && loginPassword.value);

// 🔥 UPDATED: Checks that employeeID is filled IF isEmployee is true.
const isEmployeeIdRequired = computed(() => !isEmployee.value || !!employeeID.value);

// 🔥 UPDATED: Check if all registration fields are valid/provided.
const canRegister = computed(() => {
  const isNameOrIdValid = isEmployee.value ? !!employeeID.value : !!firstName.value;

  const baseValid =
    isNameOrIdValid && // Check that either employeeID or firstName is filled
    lastName.value &&
    email.value &&
    isEmailVerified.value &&
    isPasswordValid.value &&
    !passwordMismatch.value &&
    isTermsAccepted.value &&
    captchaVerified.value;
  return baseValid;
});

const isEmailVerified = ref(false); // Track if OTP was accepted

// password validation
// Individual checks
const hasUppercase = computed(() => /[A-Z]/.test(password.value));
const hasLowercase = computed(() => /[a-z]/.test(password.value));
const hasNumber = computed(() => /[0-9]/.test(password.value));
const noSpaces = computed(() => !/\s/.test(password.value));
const minLength = computed(() => password.value.length >= 8);

const isPasswordValid = computed(
  () =>
    hasUppercase.value &&
    hasLowercase.value &&
    hasNumber.value &&
    noSpaces.value &&
    minLength.value,
);

const validatePassword = () => {
  // UI update handled by computed properties
};

// Forgot Password
const openForgotPasswordDialog = () => {
  forgotEmail.value = username.value || "";
  showForgotDialog.value = true;
};

const handleForgotPassword = async () => {
  if (!forgotEmail.value) return;
  try {
    isLoading.value = true;
    await api.post("/auth/forgot-password", { email: forgotEmail.value });
    dialogTitle.value = "Success";
    dialogMessage.value = "✅ Check your email for password reset instructions.";
    showDialog.value = true;
  } catch (err) {
    dialogTitle.value = "Error";
    dialogMessage.value = err.response?.data?.message || "Something went wrong.";
    showDialog.value = true;
  } finally {
    isLoading.value = false;
    showForgotDialog.value = false;
  }
};

watch(isLogin, (newVal) => {
  if (newVal) {
    // Switched to Login
    username.value = "";
    loginPassword.value = "";
    otp.value = "";
    forgotEmail.value = "";
    otpError.value = "";
  } else {
    // Switched to Registration
    resetRegistrationForm();
  }
});

const onCaptchaVerified = (status) => {
  captchaVerified.value = status;
};

const resetRegistrationForm = () => {
  employeeID.value = "";
  birthday.value = "";
  firstName.value = "";
  lastName.value = "";
  email.value = "";
  password.value = "";
  confirmPassword.value = "";
  isEmployee.value = false; // 🔥 UPDATED: Reset employee state
  govIDType.value = "";
  govIDNumber.value = "";
  removeID();
  isTermsAccepted.value = false;
  captchaVerified.value = false;
  showTerms.value = false;
  captchaKey.value++; // refresh CAPTCHA
};

// --- New Error State ---
const fieldErrors = ref({
  email: false,
  employeeID: false,
  firstName: false,
  lastName: false,
  password: false,
  birthday: false, // Added for BirthDate validation
});

const clearErrors = () => {
  fieldErrors.value = {
    email: false,
    employeeID: false,
    firstName: false,
    lastName: false,
    password: false,
    birthday: false,
  };
};

watch(otp, (newVal) => {
  if (!newVal) {
    otpError.value = ""; // clear error if otp is empty
  }
});

onMounted(() => {
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

onUnmounted(() => {
  if (timer) clearInterval(timer);
});

// OTP animation for countdown 1//

// Show resend after 60 seconds
const startCooldown = () => {
  showResend.value = false;
  countdown.value = 60;

  if (timer) clearInterval(timer);

  timer = setInterval(() => {
    countdown.value--;

    if (countdown.value <= 0) {
      clearInterval(timer);
      showResend.value = true; // 🔥 button appears after 60s
    }
  }, 1000);
};

const updateCountdown = () => {
  const endTime = localStorage.getItem("otpCooldownEnd");
  if (!endTime) return;

  const remaining = Math.floor((endTime - Date.now()) / 1000);

  if (remaining <= 0) {
    clearInterval(timer);
    localStorage.removeItem("otpCooldownEnd");
    showResend.value = true;
    countdown.value = 0;
  } else {
    countdown.value = remaining;
  }
};

const handleVerifyOtp = async () => {
  const fullOtp = otpArray.value.join("");

  if (!fullOtp) return;

  if (fullOtp.length !== 6) {
    otpError.value = "OTP must be 6 digits";
    return;
  }

  isLoading.value = true;
  otpError.value = ""; // clear previous errors
  try {
    const res = await api.post("/auth/verify-otp", {
      email: username.value,
      otp: fullOtp,
    });

    auth.login({ token: res.data.token });
    showOtpDialog.value = false;

    // Role-based redirection
    const role = parseInt(auth.userRole);
    if (role === 5 || role === 6) {
      router.push("/profile");
    } else if (role === 4) {
      router.push("/applicationassessment");
    } else if (role === 1) {
      router.push("/dashboard-admin");
    } else {
      router.push("/");
    }
  } catch (err) {
    // Set the error message returned from backend
    otpError.value =
      err.response?.data?.message ||
      err.response?.data?.error ||
      err.message ||
      "Invalid or expired OTP";

    console.log(err);
  } finally {
    isLoading.value = false;
  }
};

const handleResendOtp = async () => {
  if (resendAttempts.value >= maxAttempts) {
    otpError.value = "Maximum resend attempts reached.";
    return;
  }

  resendLoading.value = true;
  otpError.value = "";

  try {
    await api.post("/auth/resend-otp", {
      email: username.value,
    });

    resendAttempts.value++;
    startCooldown();
  } catch (err) {
    otpError.value = err.response?.data?.message || "Failed to resend OTP";
  } finally {
    resendLoading.value = false;
  }
};

// Handle typing in a box
const handleOtpInput = (event, index) => {
  const value = event.target.value;
  // Ensure only numbers
  if (!/^\d$/.test(value)) {
    otpArray.value[index] = "";
    return;
  }

  // Move to next input
  if (value && index < 5) {
    const nextInput = document.getElementById(`otp-${index + 1}`);
    nextInput?.focus();
  }
};

// Handle backspace
const handleOtpDelete = (event, index) => {
  if (!otpArray.value[index] && index > 0) {
    const prevInput = document.getElementById(`otp-${index - 1}`);
    prevInput?.focus();
  }
};

// Handle pasting the whole code
const handleOtpPaste = (event) => {
  const pasteData = event.clipboardData.getData("text").slice(0, 6);
  if (!/^\d+$/.test(pasteData)) return;

  const digits = pasteData.split("");
  digits.forEach((d, i) => {
    if (i < 6) otpArray.value[i] = d;
  });

  // Focus the last filled box or the 6th box
  const targetIndex = digits.length === 6 ? 5 : digits.length;
  document.getElementById(`otp-${targetIndex}`)?.focus();
};

// --- File Methods ---
function triggerValidIDUpload() {
  documentTypeToUpload.value = "ValidID";
  if (requiredDocFile.value) requiredDocFile.value.click();
}

// ------------------ Upload Handler ------------------
/**
 * Reads the file, extracts extension and converts to Base64
 */
async function handleGenericFileSelection(event) {
  const file = event.target.files[0];
  if (!file) return;

  idFileExtension.value = file.name.substring(file.name.lastIndexOf("."));
  validIDOriginalName.value = file.name;

  const reader = new FileReader();
  reader.onload = (e) => {
    idImageBase64.value = e.target.result.split(",")[1];
  };
  reader.readAsDataURL(file);
  event.target.value = null;
}

function removeID() {
  idImageBase64.value = "";
  idFileExtension.value = "";
  validIDOriginalName.value = null;
}

// ------------------ Delete Logic ------------------
async function deleteFile(docType, fileKey) {
  if (!fileKey) return;

  isLoading.value = true;
  dialogTitle.value = "Deleting";
  showDialog.value = true;

  try {
    // Registration files are always temporary until the user is created
    await api.delete(`/employee/deleteupload?fileKey=${fileKey}`);

    if (docType === "ValidID") {
      validIDFileKey.value = null;
      validIDOriginalName.value = null;
    }

    dialogTitle.value = "Success";
    dialogMessage.value = "ID removed successfully.";
  } catch (err) {
    dialogTitle.value = "Error";
    dialogMessage.value = "Failed to remove the file.";
  } finally {
    isLoading.value = false;
  }
}

// --- API Integration Functions ---

// 1. Send Verification Code (Initiate)
const sendVerificationCode = async () => {
  if (!email.value) {
    alert("Please enter an email address.");
    return;
  }

  isLoading.value = true;
  try {
    // Matches [HttpPost("initiate-registration")]
    const response = await api.post("Auth/initiate-registration", {
      email: email.value,
    });
    alert(response.data.message);
    // Start your countdown timer here
  } catch (err) {
    console.error("Full Error Object:", err); // Look at this in F12 Chrome DevTools
    console.log("Status Code:", err.response?.status);
    alert(err.response?.data?.message || "Failed to send code.");
  } finally {
    isLoading.value = false;
  }
};

// 2. Verify OTP (Before filling the rest of the form)
const verifyOtpCode = async () => {
  if (otp.value.length !== 6) {
    otpError.value = "Please enter the 6-digit code.";
    return;
  }

  isLoading.value = true;
  otpError.value = "";

  try {
    // Matches your [HttpPost("verify-registration-otp")]
    const response = await api.post("/auth/verify-registration-otp", {
      email: email.value,
      verificationCode: otp.value,
    });

    isEmailVerified.value = true;
    alert("Email Verified! You can now complete your registration.");
  } catch (err) {
    otpError.value = err.response?.data?.message || "Invalid or expired code.";
    isEmailVerified.value = false;
  } finally {
    isLoading.value = false;
  }
};

// 3. Final Sign Up (Complete)
const handleRegister = async () => {
  if (!canRegister.value) return;

  isLoading.value = true;
  try {
    const payload = {
      email: email.value,
      password: password.value,
      firstName: firstName.value,
      middleName: hasMiddleName.value ? middleName.value : "",
      lastName: lastName.value,
      suffix: "", // Add a ref if you want to capture this
      verificationCode: otp.value,
      // The other fields (Gender, BirthCity, etc.) can be omitted
      // or sent as null since the DTO is now nullable.
    };

    console.log("Sending Clean Payload:", payload);
    const response = await api.put("/Auth/complete-registration", payload);

    alert("Registration Successful!");
    router.push("/login");
  } catch (err) {
    console.error("Registration Error:", err.response?.data);
    alert(err.response?.data?.message || "Registration failed.");
  } finally {
    isLoading.value = false;
  }
};

// 4. Login Logic
const handleLogin = async () => {
  if (!username.value || !loginPassword.value) {
    alert("Please enter credentials.");
    return;
  }

  isLoading.value = true;
  otp.value = ""; // Clear old OTP input
  otpError.value = "";

  try {
    const response = await api.post("/auth/login", {
      email: username.value,
      password: loginPassword.value,
    });

    console.log("Login API Success, showing dialog...");
    showOtpDialog.value = true;
    console.log("Current showOtpDialog state:", showOtpDialog.value);
  } catch (err) {
    alert(err.response?.data?.message || "Login failed.");
  } finally {
    isLoading.value = false;
  }
};

const handleVerifyLoginOtp = async () => {
  if (otp.value.length !== 6) return;

  isLoading.value = true;
  otpError.value = "";

  try {
    // This calls your backend endpoint that checks LoginOtp and LoginOtpExpiry
    const response = await api.post("/auth/verify-otp", {
      email: username.value,
      verificationCode: otp.value,
    });

    // 1. Save the token to your Pinia store
    auth.login({
      token: response.data.token,
      user: response.data.user,
    });

    showOtpDialog.value = false;
    alert("Login Successful!");

    // 2. Redirect based on user role (matching your store logic)
    const role = parseInt(auth.userRole);
    if (role === 1) {
      router.push("/dashboard-admin");
    } else if (role === 4) {
      router.push("/applicationassessment");
    } else {
      router.push("/profile");
    }
  } catch (err) {
    otpError.value = err.response?.data?.message || "Invalid or expired OTP.";
  } finally {
    isLoading.value = false;
  }
};
</script>

<style>
/* Add this outside of any scoped blocks to test */
.otp-dialog-content,
[class*="dialog"],
[class*="modal"] {
  z-index: 9999 !important;
  position: relative;
}
</style>
