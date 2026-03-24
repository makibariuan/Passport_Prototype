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
          <input v-model="password" type="password" placeholder="Password" />
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
          It is advisable NOT to purchase outbound travel tickets until your passports are actually in your possession...
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

          <input v-if="hasMiddleName"
                 v-model="middleName"
                 type="text"
                 placeholder="Middle Name"
                 style="margin-top: 8px;" />
        </div>

        <div class="form-field">
          <label>Last Name</label>
          <input v-model="lastName" type="text" placeholder="Last Name" />
        </div>

        <div class="form-field">
          <label>Suffix</label>
          <input type="text" placeholder="Suffix" />
        </div>

        <div class="form-field">
          <label>Email (Will be your Username)</label>
          <div class="input-with-action">
            <input v-model="email" type="email" placeholder="Email Address" />
            <button class="action-btn">Send verification code</button>
          </div>
        </div>

        <div class="form-field">
          <label>Verification Code</label>
          <div class="otp-wrapper">
            <input v-model="otp"
                   type="text"
                   maxlength="6"
                   class="otp-hidden-input"
                   autocomplete="one-time-code" />

            <div class="otp-slots-container">
              <div v-for="i in 6" :key="i" class="otp-slot" :class="{ 'filled': otp.length >= i }">
                {{ otp[i-1] || '' }}
              </div>
            </div>
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

        <button @click="handleRegister" class="submit-register-btn">Sign Up</button>
      </section>
    </main>
  </div>
</template>

<script setup>
  // NOTE: Assuming these components are available in the project structure
  import Captcha from '@/components/Auth/Captcha.vue'
  import { ref, computed, watch, nextTick, onMounted, onUnmounted } from "vue";
  import { useRouter, useRoute } from "vue-router";
  import api from "@/api";
  import { useAuthStore } from "@/stores/auth";
  import DialogBox from "@/components/DialogBox.vue";
  import LoadingDialog from "@/components/LoadingDialog.vue";
  import TermsOfService from "@/components/TermsOfService.vue";
  import AuthHeader from "@/components/Auth/AuthHeader.vue"

  // Using @vueuse/head for script injection
  import { useHead } from '@vueuse/head';

  useHead({
    script: [
      {
        src: 'https://www.google.com/recaptcha/api.js',
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

  const hasMiddleName = ref(true) // Default to "Yes"
  const middleName = ref('')

  // captcha verification
  const captchaVerified = ref(false);
  const captchaKey = ref(0);

  // terms of service
  const showTerms = ref(false);
  const isTermsAccepted = ref(false);

  const currentPST = ref("");

  const updatePST = () => {
    const options = {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    };
    currentPST.value = new Date().toLocaleString('en-US', options);
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

  // Role selection
  // const userRole = ref("Employee"); // <--- REMOVED: Replaced by isEmployee

  const employeeID = ref("");
  const lastName = ref("");
  const firstName = ref("");
  const birthday = ref("");

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

  const otpArray = ref(['', '', '', '', '', '']);

  // Computed
  const passwordMismatch = computed(() => confirmPassword.value && password.value !== confirmPassword.value);
  const canLogin = computed(() => username.value && password.value);

  // 🔥 UPDATED: Checks that employeeID is filled IF isEmployee is true.
  const isEmployeeIdRequired = computed(() => !isEmployee.value || !!employeeID.value);

  // 🔥 UPDATED: Check if all registration fields are valid/provided.
  const canRegister = computed(() => {
    const isNameOrIdValid = isEmployee.value ? !!employeeID.value : !!firstName.value;

    const baseValid = (
      isNameOrIdValid && // Check that either employeeID or firstName is filled
      lastName.value &&
      birthday.value &&
      email.value &&
      isPasswordValid.value &&
      !passwordMismatch.value &&
      //govIDType.value &&
      //govIDNumber.value &&
      idImageBase64.value && // Ensure ID image is captured
      isTermsAccepted.value &&
      captchaVerified.value
    );
    return baseValid;
  });


  // password validation
  // Individual checks
  const hasUppercase = computed(() => /[A-Z]/.test(password.value));
  const hasLowercase = computed(() => /[a-z]/.test(password.value));
  const hasNumber = computed(() => /[0-9]/.test(password.value));
  const noSpaces = computed(() => !/\s/.test(password.value));
  const minLength = computed(() => password.value.length >= 8);

  const isPasswordValid = computed(() =>
    hasUppercase.value &&
    hasLowercase.value &&
    hasNumber.value &&
    noSpaces.value &&
    minLength.value
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

  // --- New Gov ID State ---
  const govIDType = ref("");
  const govIDNumber = ref("");
  const idImageBase64 = ref("");
  const idFileExtension = ref("");
  const validIDOriginalName = ref(null);
  const documentTypeToUpload = ref(""); // Needed for trigger logic
  const requiredDocFile = ref(null);    // Needed for input ref


  watch(isLogin, (newVal) => {
    if (newVal) {
      // Switched to Login
      username.value = "";
      password.value = "";
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
    birthday: false // Added for BirthDate validation
  });

  const clearErrors = () => {
    fieldErrors.value = {
      email: false,
      employeeID: false,
      firstName: false,
      lastName: false,
      password: false,
      birthday: false
    };
  };

  // Register
  const handleRegister = async () => {
    clearErrors();

    // 1. FRONT-END VALIDATION: Check required fields upfront
    if (!email.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "Email Address is required.";
      showDialog.value = true;
      return;
    }

    // 🔥 UPDATED: Check for the relevant ID/Name field
    if (isEmployee.value && !employeeID.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "Employee ID is required for employee registration.";
      showDialog.value = true;
      return;
    } else if (!isEmployee.value && !firstName.value) {
      dialogTitle.value = "Required Field";
      dialogMessage.value = "First Name is required for citizen registration.";
      showDialog.value = true;
      return;
    }

    if (!isPasswordValid.value) {
      dialogTitle.value = "Password";
      dialogMessage.value = "Please ensure your password meets all the requirements.";
      showDialog.value = true;
      return;
    }

    if (passwordMismatch.value) {
      dialogTitle.value = "Password Mismatch";
      dialogMessage.value = "The confirmed password does not match the password.";
      showDialog.value = true;
      return;
    }


    if (!isTermsAccepted.value) {
      dialogTitle.value = "Terms of Service";
      dialogMessage.value = "Please accept the Terms of Service before registering.";
      showDialog.value = true;
      return;
    }


    if (!captchaVerified.value) {
      dialogTitle.value = "Captcha Required";
      dialogMessage.value = "Please verify the captcha before signing up.";
      showDialog.value = true;
      return;
    }


    try {
      isLoading.value = true;

      // 2. DATA PAYLOAD PREPARATION
      const registrationData = {
        username: email.value,
        email: email.value,
        password: password.value,
        firstName: firstName.value,
        lastName: lastName.value,
        employeeID: isEmployee.value ? employeeID.value : "",
        birthDate: birthday.value ? new Date(birthday.value).toISOString() : null,
        userRole: isEmployee.value ? 1 : 0,
        govIDType: govIDType.value,
        govIDNumber: govIDNumber.value,
        idImageBase64: idImageBase64.value,
        idFileExtension: idFileExtension.value
      };


      const res = await api.post("/auth/register", registrationData);

      dialogTitle.value = "Success";
      dialogMessage.value = res.data.message || "Registration successful ✅";
      showDialog.value = true;

      isLogin.value = true;
      resetRegistrationForm();
      username.value = email.value;
    } catch (err) {
      console.error("FULL REGISTRATION ERROR:", err);
      dialogTitle.value = "Registration Failed";

      let msg = "Registration failed.";
      if (err.response?.data?.message) {
    msg = err.response.data.message;
    const lowerMsg = msg.toLowerCase();

    // 1. Check for Birthdate first (Specific mismatch)
    if (lowerMsg.includes("birth date") || lowerMsg.includes("birthdate")) {
      fieldErrors.value.birthday = true;
    }
    // 2. Check for Name mismatches
    else if (lowerMsg.includes("first name")) {
      fieldErrors.value.firstName = true;
    }
    else if (lowerMsg.includes("last name") || lowerMsg.includes("surname")) {
      fieldErrors.value.lastName = true;
    }
    // 3. Check for Email
    else if (lowerMsg.includes("email")) {
      fieldErrors.value.email = true;
    }
    // 4. Check for Employee ID (General catch-all for ID errors)
    else if (lowerMsg.includes("employee id")) {
      fieldErrors.value.employeeID = true;
    }

  } else if (err.response?.data?.errors) {
    // Handle ASP.NET ModelState errors (Dictionary based)
    const errorData = err.response.data.errors;
    msg = Object.values(errorData).flat().join("\n");

    if (errorData.Email) fieldErrors.value.email = true;
    if (errorData.EmployeeID) fieldErrors.value.employeeID = true;
    if (errorData.FirstName) fieldErrors.value.firstName = true;
    if (errorData.LastName || errorData.Surname) fieldErrors.value.lastName = true;
    if (errorData.BirthDate) fieldErrors.value.birthday = true;
  }
      captchaKey.value++; // refresh CAPTCHA
      dialogMessage.value = msg;
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
  };

  // Login Step 1: credentials → send OTP
  // AuthPage.vue: Inside handleLogin function
  const handleLogin = async () => {
    try {
      isLoading.value = true;
      otpArray.value = ['', '', '', '', '', ''];
      await api.post("/auth/login", {
        email: username.value, // Make sure this key is 'email'
        password: password.value,
      });
      showOtpDialog.value = true;
      startCooldown(); // 🔥 start 60 second timer

    } catch (err) {
      dialogTitle.value = "Login Failed";

      // 1. Initialize 'msg' with a strong fallback string
      let msg = "Login failed. A connection or unknown error occurred.";

      if (err.response?.data?.message) {
        msg = err.response.data.message;
      } else if (err.response?.data?.errors) {
        // Handle ASP.NET ModelState errors
        const errors = Object.values(err.response.data.errors).flat();
        msg = errors.join("\n");
      } else if (err.message) {
        // Handle general network errors (e.g., Axios timeout)
        msg = err.message;
      }

      // 2. CRITICAL STEP: Explicitly ensure a string is assigned
      dialogMessage.value = String(msg);
      showDialog.value = true;
    } finally {
      isLoading.value = false;
    }
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
  const fullOtp = otpArray.value.join('');

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
    if (role === 4 || role === 5 || role === 6) {
      router.push("/dashboard");
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

      console.log (err);
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
      email: username.value
    });

    resendAttempts.value++;
    startCooldown();

  } catch (err) {
    otpError.value =
      err.response?.data?.message || "Failed to resend OTP";
  } finally {
    resendLoading.value = false;
  }
};



  // Handle typing in a box
  const handleOtpInput = (event, index) => {
    const value = event.target.value;
    // Ensure only numbers
    if (!/^\d$/.test(value)) {
      otpArray.value[index] = '';
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
    const pasteData = event.clipboardData.getData('text').slice(0, 6);
    if (!/^\d+$/.test(pasteData)) return;

    const digits = pasteData.split('');
    digits.forEach((d, i) => {
      if (i < 6) otpArray.value[i] = d;
    });

    // Focus the last filled box or the 6th box
    const targetIndex = digits.length === 6 ? 5 : digits.length;
    document.getElementById(`otp-${targetIndex}`)?.focus();
  };

  // --- File Methods ---
  function triggerValidIDUpload() {
    documentTypeToUpload.value = 'ValidID';
    if (requiredDocFile.value) requiredDocFile.value.click();
  }

  // ------------------ Upload Handler ------------------
  /**
 * Reads the file, extracts extension and converts to Base64
 */
  async function handleGenericFileSelection(event) {
    const file = event.target.files[0];
    if (!file) return;

    idFileExtension.value = file.name.substring(file.name.lastIndexOf('.'));
    validIDOriginalName.value = file.name;

    const reader = new FileReader();
    reader.onload = (e) => {
      idImageBase64.value = e.target.result.split(',')[1];
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

      if (docType === 'ValidID') {
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
</script>


